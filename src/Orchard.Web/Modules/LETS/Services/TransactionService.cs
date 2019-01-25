using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using LETS.Models;
using LETS.ViewModels;
using NHibernate;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.Environment;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Notify;

namespace LETS.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly IRepository<MemberPartRecord> _memberRepository;
        private readonly IMemberService _memberService;
        private readonly IOrchardServices _orchardServices;
        private readonly IRepository<TransactionPartRecord> _transactionRepository;
        public readonly Localizer T;
        private readonly INotifier _notifier;
        private readonly IRepository<CreditUsageRecord> _creditUsageRepository;
        private readonly Work<ISessionLocator> _sessionLocator;
        private int _idDemurrageRecipient;
        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;
        private readonly Lazy<CultureInfo> _cultureInfo;
        private readonly IMembershipService _membershipService;
        private readonly IRepository<TransactionRecordSimulation> _transactionRecordSimulationRepository;
        private readonly IRepository<CreditUsageRecordSimulation> _creditUsageRecordSimulationRepository;
        private readonly IRepository<CreditUsageRecordSimulation> _creditUsageSimulationRepository;

        public TransactionService(IRepository<MemberPartRecord> memberRepository, IMemberService memberService, IOrchardServices orchardServices, IRepository<TransactionPartRecord> transactionRepository, INotifier notifier, IRepository<CreditUsageRecord> creditUsageRepository, Work<ISessionLocator> sessionLocator, ICacheManager cacheManager, ISignals signals, IMembershipService membershipService, IRepository<TransactionRecordSimulation> transactionRecordSimulationRepository, IRepository<CreditUsageRecordSimulation> creditUsageRecordSimulationRepository, IRepository<CreditUsageRecordSimulation> creditUsageSimulationRepository)
        {
            _memberRepository = memberRepository;
            _memberService = memberService;
            _orchardServices = orchardServices;
            _transactionRepository = transactionRepository;
            _notifier = notifier;
            _creditUsageRepository = creditUsageRepository;
            _sessionLocator = sessionLocator;
            _cacheManager = cacheManager;
            _signals = signals;
            _membershipService = membershipService;
            _transactionRecordSimulationRepository = transactionRecordSimulationRepository;
            _creditUsageRecordSimulationRepository = creditUsageRecordSimulationRepository;
            _creditUsageSimulationRepository = creditUsageSimulationRepository;
            _cacheManager = cacheManager;
            T = NullLocalizer.Instance;
            _cultureInfo = new Lazy<CultureInfo>(() => CultureInfo.GetCultureInfo(_orchardServices.WorkContext.CurrentCulture));
        }

        public void SaveTransaction(ContentItem contentItem, EditTransactionViewModel model)
        {
            var transactionPart = contentItem.As<TransactionPart>();
            var fullDateTime = model.TransactionDate + " " + model.TransactionTime;
            DateTime value;
            if (DateTime.TryParse(fullDateTime, _cultureInfo.Value, DateTimeStyles.None, out value))
            {
                transactionPart.TransactionDate = value;
            }
            transactionPart.TransactionType = model.TransactionType;
            transactionPart.Seller = _memberRepository.Get(m => m.Id == model.IdSeller);
            transactionPart.Buyer = _memberRepository.Get(m => m.Id == model.IdBuyer);
            transactionPart.Description= model.Description;
            Debug.Assert(model.Value != null, "model.Value != null, validation ensures a value is submitted");
            transactionPart.Value = (int)model.Value;
            if (contentItem.VersionRecord.Number.Equals(1))
            {
                transactionPart.CreditValue = GetCreditValueForNewTransaction(model);
            }
            UpdateCreditUsage(transactionPart);
            _signals.Trigger("letsMemberListChanged");
            _signals.Trigger(string.Format("letsMemberBal{0}Changed", model.IdSeller));
            _signals.Trigger(string.Format("letsMemberBal{0}Changed", model.IdBuyer));
            _signals.Trigger(string.Format("letsMemberTransactionCount{0}Changed", model.IdSeller));
            _signals.Trigger(string.Format("letsMemberTransactionCount{0}Changed", model.IdBuyer));
            _signals.Trigger("letsTotalTurnoverChanged");
            if (transactionPart.TransactionType.Equals(TransactionType.Trade))
            {
                _signals.Trigger(string.Format("letsMemberTurnover{0}Changed", model.IdSeller));
                _signals.Trigger(string.Format("letsMemberTurnover{0}Changed", model.IdBuyer));
            }
        }

        private void UpdateCreditUsage(TransactionPart transactionPart)
        {
            var idBuyer = transactionPart.Buyer.Id;
            var buyerBalance = _memberService.GetMemberBalance(idBuyer, true);
            if (buyerBalance > 0)
            {
                var creditsToUse = Math.Min(transactionPart.Value, buyerBalance);
                UseCredits(creditsToUse, idBuyer, transactionPart.Id, transactionPart.TransactionType);
            }
        }

        public int GetTransactionCount(int idMember) {
            return _cacheManager.Get(string.Format("letsMemberTransactionCount{0}", idMember), ctx => {
                ctx.Monitor(_signals.When(string.Format("letsMemberTransactionCount{0}Changed", idMember)));
                    return _orchardServices.ContentManager.Query<TransactionPart, TransactionPartRecord>().Where(t => t.BuyerMemberPartRecord.Id.Equals(idMember) || t.SellerMemberPartRecord.Id.Equals(idMember)).Count();
                });
        }

        public void CorrectMemberCreditValues(int idMember)
        {
            var memberBalance = _memberService.GetMemberBalance(idMember, true);
            var memberCreditValueTotal = _memberService.GetCreditValueTotal(idMember);
            if (memberBalance > 0)
            {
                if (memberBalance < memberCreditValueTotal)
                {
                    UseCredits(memberCreditValueTotal - memberBalance, idMember, 0, TransactionType.Adjustment);
                }
                else
                {
                    RestoreCredits(memberBalance - memberCreditValueTotal, idMember);
                }
            }
            else if (memberBalance <= 0 && memberCreditValueTotal > 0)
            {
                UseCredits(memberCreditValueTotal, idMember, 0, TransactionType.Adjustment);
            }
        }

        private void RestoreCredits(int creditsToRestore, int idMember)
        {
            var newestSalesWithoutFullCredit =
                _orchardServices.ContentManager.Query<TransactionPart, TransactionPartRecord>().Where(
                    t => t.SellerMemberPartRecord.Id.Equals(idMember) && t.CreditValue < t.Value).OrderByDescending(
                        t => t.TransactionDate).List().GetEnumerator();
            var creditsRestored = 0;
            while (newestSalesWithoutFullCredit.MoveNext() && creditsRestored < creditsToRestore)
            {
                var nextNewestSale = newestSalesWithoutFullCredit.Current;
                var creditsToRestoreToThisSale = Math.Min(nextNewestSale.Value, creditsToRestore - creditsRestored);
                creditsRestored += creditsToRestoreToThisSale;
                nextNewestSale.CreditValue += creditsToRestoreToThisSale;
                _transactionRepository.Update(nextNewestSale.Record);

            }
        }

        private void UseCredits(int creditsToUse, int idMember, int idTransactionSpent, TransactionType transactionType)
        {
            var oldestTransactionsWithCreditValue =
                _orchardServices.ContentManager.Query<TransactionPart, TransactionPartRecord>().Where(
                    t => t.SellerMemberPartRecord.Id.Equals(idMember) && t.CreditValue > 0).OrderBy
                    (t => t.TransactionDate).List().GetEnumerator();
            var creditsUsed = 0;
            while (oldestTransactionsWithCreditValue.MoveNext() && creditsUsed < creditsToUse)
            {
                var nextOldestTransaction = oldestTransactionsWithCreditValue.Current;
                var creditToUseFromThisTransaction = Math.Min(nextOldestTransaction.CreditValue, creditsToUse - creditsUsed);
                creditsUsed += creditToUseFromThisTransaction;
                nextOldestTransaction.CreditValue -= creditToUseFromThisTransaction;
                Debug.Assert(nextOldestTransaction.CreditValue >= 0);
                _transactionRepository.Update(nextOldestTransaction.Record);

                var creditUsageRecord = new CreditUsageRecord
                                            {
                                                IdTransactionEarnt = nextOldestTransaction.Id,
                                                IdTransactionSpent = idTransactionSpent,
                                                RecordedDate = DateTime.Now,
                                                TransactionType = transactionType,
                                                Value = creditToUseFromThisTransaction
                                            };
                _creditUsageRepository.Create(creditUsageRecord);
            }
        }

        public IEnumerable<MemberTransactionViewModel> GetTransactions(int idMember, int count, int page = 1)
        {
            var session = _sessionLocator.Value.For(null);
            var query = session.CreateSQLQuery(string.Format(@"EXEC dbo.LETS_GetMemberTransactions :@idMember, :@pageSize, :@pageNumber"));
            query.SetString("@idMember", Convert.ToString(idMember));
            query.SetString("@pageSize", Convert.ToString(count));
            query.SetString("@pageNumber", Convert.ToString(page));
            var transactions = query.List<object[]>()
                .Select(record => new MemberTransactionViewModel
                {
                    Id = (int)record[0],
                    TransactionDate = (DateTime)record[1],
                    IdTradingPartner = (int)record[2],
                    TradingPartner = (string)record[3],
                    UserName = (string)record[4],
                    Description = (string)record[5],
                    Value = (int)record[6],
                    CreditValue = (int)record[7],
                    TransactionType = (string)record[8],
                    RunningTotal = (int)record[9]
                }
                ).ToList();


            return transactions;
        }

        public IEnumerable<DemurrageTransactionViewModel> GetDemurrages()
        {
            var session = _sessionLocator.Value.For(null);
            var query = session.CreateSQLQuery(@"
                select cu.IdTransactionSpent, cu.RecordedDate, mRecipient.Id, mRecipient.FirstName, mRecipient.LastName, mOriginalSeller.Id as mId, mOriginalSeller.FirstName as mFirstName, 
                    mOriginalSeller.LastName as mLastName, cu.Value, te.Id as teId, te.Description, te.Value as teValue, te.CreditValue, 
                    mOriginalBuyer.Id as mOriginalBuyerId, mOriginalBuyer.FirstName as mOriginalBuyerFirstName, mOriginalBuyer.LastName as mOriginalBuyerLastName,
                    te.TransactionDate
                from LETS_CreditUsageRecord cu
                join LETS_TransactionPartRecord ts on ts.Id = cu.IdTransactionSpent
                join LETS_MemberPartRecord mRecipient on mRecipient.Id = ts.SellerMemberPartRecord_Id
                join LETS_MemberPartRecord mOriginalSeller on mOriginalSeller.Id = ts.BuyerMemberPartRecord_Id
                join LETS_TransactionPartRecord te on te.Id = cu.IdTransactionEarnt
                join LETS_MemberPartRecord mOriginalBuyer on mOriginalBuyer.Id = te.BuyerMemberPartRecord_Id
                where cu.TransactionType = 'Demurrage'
                order by cu.RecordedDate desc
                ");
            var list = RemoveUnpublishedResults(query.List<object[]>())
                .Select(record => new DemurrageTransactionViewModel
                                      {
                                          IdDemurrageTransaction = (int)record[0],
                                          RecordedDate = (DateTime)record[1],
                                          IdDemurrageRecipient = (int)record[2],
                                          NameDemurrageRecipient = string.Format("{0} {1}", record[3], record[4]),
                                          IdSellerTransactionEarnt = (int)record[5],
                                          NameSellerTransactionEarnt = string.Format("{0} {1}", record[6], record[7]),
                                          ValueDeducted = (int)record[8],
                                          IdTransactionEarnt = (int)record[9],
                                          DescriptionTransactionEarnt = (string)record[10],
                                          ValueTransactionEarnt = (int)record[11],
                                          CreditValueTransactionEarnt = (int)record[12],
                                          IdBuyerTransactionEarnt = (int)record[13],
                                          NameBuyerTransactionEarnt = string.Format("{0} {1}", record[14], record[15]),
                                          TransactionDateEarnt = (DateTime)record[16]
                                      }
                ).ToList();
            return list;
        }

        public List<DemurrageTransactionsViewModel> ForecastDemurrage(int idMember = 0)
        {
            var letsSettings = _orchardServices.WorkContext.CurrentSite.As<LETSSettingsPart>();
            var demurrageDays = letsSettings.DemurrageTimeIntervalDays;
            var demurrageStartDate = letsSettings.DemurrageStartDate ?? DateTime.MinValue;
            var steps = letsSettings.DemurrageStepsList.ToList();
            var demurrageTransactions = new List<DemurrageTransactionsViewModel>();
            var session = _sessionLocator.Value.For(null);
            var joinMemberTable = string.Empty;
            var whereMember = string.Empty;
            if (idMember != 0)
            {
                joinMemberTable = " join LETS_MemberPartRecord m on m.id = t.SellerMemberPartRecord_Id ";
                whereMember = string.Format(" and m.Id = '{0}' ", idMember);
            }
            //var queryTradesWithCreditValue = _session.CreateSQLQuery(string.Format(
            var queryTradesWithCreditValue = session.CreateSQLQuery(string.Format(
                @"
                select t.Id, t.TransactionDate, t.SellerMemberPartRecord_Id, t.CreditValue, t.Description, t.TransactionType, t.Value, t.BuyerMemberPartRecord_Id from LETS_TransactionPartRecord t
                {0}
                where t.TransactionType != 'Demurrage'
                and t.CreditValue > 0
                {1}
            ", joinMemberTable, whereMember));
            DeleteTradesToSimulate(idMember);
            CopyTradesToSimulate(RemoveUnpublishedResults(queryTradesWithCreditValue.List<object[]>()));
            if (idMember != 0)
            {
                whereMember = string.Format(" and ts.SellerMemberPartRecord_Id = '{0}' ", idMember);
            }
            var queryDemurrageCreditUsages = session.CreateSQLQuery(string.Format(
                @"
                SELECT cu.Id, ts.Id as IdTransaction, cu.TransactionType, cu.Value, cu.RecordedDate, cu.IdTransactionSpent
                FROM LETS_CreditUsageRecord cu
                join LETS_TransactionRecordSimulation ts on ts.IdTransaction = cu.IdTransactionEarnt
                WHERE cu.TransactionType = 'Demurrage'
                {0}
            ", whereMember));
            DeleteDemurrageCreditUsagesToSimulate(session, idMember);
            CopyDemurrageCreditUsagesToSimulate(queryDemurrageCreditUsages.List<object[]>());
            var daysToBegin = (int)Math.Ceiling((demurrageStartDate - DateTime.Now).TotalDays) + demurrageDays;
            if (daysToBegin < 0) daysToBegin = 0;
            for (var day = 0; day <= (steps.Count + 1) * demurrageDays + daysToBegin; day++)
            {
                var today = DateTime.Now.AddDays(day);
                var transactionsForDemurrageToZero = GetTransactionsForDemurrageToZero(session, demurrageDays, steps, demurrageStartDate, today, "LETS_TransactionRecordSimulation", "LETS_CreditUsageRecordSimulation").ToList();
                var simulatedDemurrageTransactions = new List<DemurrageTransactionViewModel>();
                var toBeDeducted = 0;
                var tradeValue = 0;
                var unspentCreditValue = 0;
                if (transactionsForDemurrageToZero.Any())
                {
                    simulatedDemurrageTransactions.AddRange(SimulateDemurrageTransactions(transactionsForDemurrageToZero, null, today, out toBeDeducted, out tradeValue, out unspentCreditValue));
                }
                for (var stepNumber = steps.Count - 1; stepNumber >= 0; stepNumber--)
                {
                    var transactionsForDemurrageStep =
                        GetTransactionsForDemurrageStep(session, demurrageDays, stepNumber, demurrageStartDate, today, "LETS_TransactionRecordSimulation", "LETS_CreditUsageRecordSimulation").ToList();
                    if (transactionsForDemurrageStep.Any())
                    {
                        simulatedDemurrageTransactions.AddRange(SimulateDemurrageTransactions(transactionsForDemurrageStep, steps[stepNumber], today, out toBeDeducted, out tradeValue, out unspentCreditValue));
                    }
                }
                demurrageTransactions.Add(new DemurrageTransactionsViewModel
                    {
                        DemurrageDate = today,
                        ToBeDeducted = toBeDeducted,
                        TradeValue = tradeValue,
                        UnspentCreditValue = unspentCreditValue,
                        DemurrageTransactions = simulatedDemurrageTransactions
                    });
            }
            return demurrageTransactions;
        }

        public void DeleteCreditUsage(int idCreditUsage)
        {
            _creditUsageRepository.Delete(_creditUsageRepository.Get(idCreditUsage));
        }

        public void DeleteDemurrageCreditUsages(int idTransaction)
        {
            var demurrageCreditUsages = _creditUsageRepository.Fetch(cu => cu.IdTransactionSpent.Equals(idTransaction));
            foreach (var demurrageCreditUsage in demurrageCreditUsages)
            {
                _creditUsageRepository.Delete(demurrageCreditUsage);
            }
            _creditUsageRepository.Flush();
        }

        public IList<CreditUsageRecord> FindOrphanedDemurrageCreditUsages()
        {
            var orphans = new List<CreditUsageRecord>();
            var demurrageCreditUsages = _creditUsageRepository.Fetch(cu => cu.TransactionType.ToString() == "Demurrage");
            foreach (var demurrageCreditUsage in demurrageCreditUsages)
            {
                var demurrageTransaction = _orchardServices.ContentManager.Get<TransactionPart>(demurrageCreditUsage.IdTransactionSpent, VersionOptions.Published);
                if (demurrageTransaction == null)
                {
                    var originalTransaction = _orchardServices.ContentManager.Get<TransactionPart>(demurrageCreditUsage.IdTransactionEarnt, VersionOptions.Published);
                    if (originalTransaction != null)
                    {
                        orphans.Add(demurrageCreditUsage);
                    }
                }
            }
            return orphans;
        }

        private void DeleteDemurrageCreditUsagesToSimulate(ISession session, int idMember)
        {
            var whereMember = string.Empty;
            if (idMember != 0)
            {
                whereMember = string.Format("JOIN LETS_TransactionRecordSimulation ts on ts.Id = cu.IdTransactionEarnt WHERE ts.SellerMemberPartRecord_Id = '{0}'", idMember);
            }
            var querySimulatedDemurrageCreditUsages = session.CreateSQLQuery(string.Format(
                @"
                DELETE FROM LETS_CreditUsageRecordSimulation where Id in 
                (SELECT cu.Id
                FROM LETS_CreditUsageRecordSimulation cu
                {0})
                ", whereMember));
            querySimulatedDemurrageCreditUsages.ExecuteUpdate();
        }

        private void CopyDemurrageCreditUsagesToSimulate(IEnumerable<object[]> queryDemurrageCreditUsages)
        {
            foreach (var queryDemurrageCreditUsage in queryDemurrageCreditUsages)
            {
                var creditUsageRecordSimulation = new CreditUsageRecordSimulation
                {
                    IdCreditUsage = (int)queryDemurrageCreditUsage[0],
                    IdTransactionEarnt = (int)queryDemurrageCreditUsage[1],
                    TransactionType = (string)queryDemurrageCreditUsage[2],
                    Value = (int)queryDemurrageCreditUsage[3],
                    RecordedDate = (DateTime)queryDemurrageCreditUsage[4],
                    IdTransactionSpent = (int)queryDemurrageCreditUsage[5]
                };

                _creditUsageSimulationRepository.Create(creditUsageRecordSimulation);
            }
            _creditUsageSimulationRepository.Flush();
        }

        private void DeleteTradesToSimulate(int idMember)
        {
            var simulatedTrades = idMember == 0
                                      ? _transactionRecordSimulationRepository.Table
                                      : _transactionRecordSimulationRepository.Fetch(
                                          t => t.SellerMemberPartRecord_Id.Equals(idMember));
            foreach (var simulatedTrade in simulatedTrades)
            {
                _transactionRecordSimulationRepository.Delete(simulatedTrade);
            }
            _transactionRecordSimulationRepository.Flush();
        }

        private void CopyTradesToSimulate(IEnumerable<object[]> trades)
        {
            foreach (var trade in trades)
            {
                var transactionRecordSimulation = new TransactionRecordSimulation
                    {
                        // select t.Id, t.TransactionDate, t.SellerMemberPartRecord_Id, t.CreditValue, t.Description, t.TransactionType, t.Value, t.BuyerMemberPartRecord_Id from LETS_TransactionPartRecord t
                        IdTransaction = (int) trade[0],
                        TransactionDate = (DateTime) trade[1],
                        SellerMemberPartRecord_Id = (int) trade[2],
                        CreditValue = (int)trade[3],
                        Description = (string) trade[4],
                        TransactionType = (string)trade[5],
                        Value = (int)trade[6],
                        BuyerMemberPartRecord_Id = (int)trade[7],
                    };

                _transactionRecordSimulationRepository.Create(transactionRecordSimulation);
            }
            _transactionRecordSimulationRepository.Flush();
        }


        public void ProcessDemurrage()
        {
            var letsSettings = _orchardServices.WorkContext.CurrentSite.As<LETSSettingsPart>();
            var demurrageEnabled = letsSettings.UseDemurrage;
            var demurrageStartDate = letsSettings.DemurrageStartDate ?? DateTime.MinValue;
            if (!demurrageEnabled)
            {
                _notifier.Error(T("Demurrage is not enabled"));
                return;
            }
            if (DateTime.Now < demurrageStartDate)
            {
                _notifier.Error(T("Demurrage hasn't started yet, check the LETS settings"));
                return;
            }
            var demurrageDays = letsSettings.DemurrageTimeIntervalDays;
            var steps = letsSettings.DemurrageStepsList.ToList();
            _idDemurrageRecipient = letsSettings.IdDemurrageRecipient.Value;
            var session = _sessionLocator.Value.For(null);
            var resultsCreditsToZero = GetTransactionsForDemurrageToZero(session, demurrageDays, steps, demurrageStartDate, DateTime.Now, "LETS_TransactionPartRecord", "LETS_CreditUsageRecord");
            var chargesRecorded = RecordDemurrageTransactions(RemoveUnpublishedResults(resultsCreditsToZero), null);
            _notifier.Information(T("Credits zeroed from {0} transactions", chargesRecorded));
            for (var stepNumber = steps.Count - 1; stepNumber >= 0; stepNumber--)
            {
                var resultsStep = GetTransactionsForDemurrageStep(session, demurrageDays, stepNumber, demurrageStartDate, DateTime.Now, "LETS_TransactionPartRecord", "LETS_CreditUsageRecord");
                chargesRecorded = RecordDemurrageTransactions(RemoveUnpublishedResults(resultsStep), steps[stepNumber]);
                _notifier.Information(T("Demurrage step {2}: charged on {0} transactions at the rate of {1}%",
                                            chargesRecorded, steps[stepNumber], stepNumber + 1));
            }
        }

        private ICollection<object[]> GetTransactionsForDemurrageStep(ISession session, int demurrageDays, int stepNumber, DateTime demurrageStartDate, DateTime today, string transactionTableName, string creditUsageTableName)
        {
            var queryStep = session.CreateSQLQuery(string.Format(
                @"
                    select t.Id, t.TransactionDate, t.SellerMemberPartRecord_Id, t.CreditValue, t.Description, 
                    mBuyer.LastName as mBuyerLastName, mSeller.LastName as mSellerLastName, 
                    t.Value, mBuyer.Id as mBuyerId from {0} t
                    join LETS_MemberPartRecord mBuyer on mBuyer.Id = t.BuyerMemberPartRecord_Id
                    join LETS_MemberPartRecord mSeller on mSeller.Id = t.SellerMemberPartRecord_Id
                    join LETS_MemberAdminPartRecord maSeller on maSeller.Id = t.SellerMemberPartRecord_Id
                    cross apply dbo.LETS_CustomMaxDateFunction(t.transactionDate, :demurrageStartDate) as mdf
                    where t.TransactionType != 'Demurrage'
                    and maSeller.MemberType = 'Member'
                    and t.CreditValue > 0
                    and DATEDIFF(DAY, mdf.MaxDate, :today) > :days
                    and (select COUNT(id) from {1} cu where cu.TransactionType = 'Demurrage' and cu.IdTransactionEarnt = t.Id) = :step
                ", transactionTableName, creditUsageTableName))
                                   .SetParameter("today", today.ToString("yyyy-MM-dd"))
                                   .SetParameter("demurrageStartDate", demurrageStartDate.ToString("yyyy-MM-dd"))
                                   .SetParameter("days", demurrageDays*(stepNumber + 1))
                                   .SetParameter("step", stepNumber);
            var resultsStep = queryStep.List<object[]>();
            return resultsStep;
        }

        private ICollection<object[]> GetTransactionsForDemurrageToZero(ISession session, int demurrageDays, ICollection steps, DateTime demurrageStartDate, DateTime today, string transactionTableName, string creditUsageTableName)
        {
            var queryCreditsToZero = session.CreateSQLQuery(string.Format(
                @"
                    select t.Id, t.TransactionDate, t.SellerMemberPartRecord_Id, t.CreditValue, t.Description, 
                    mBuyer.LastName as mBuyerLastName, mSeller.LastName as mSellerLastName, 
                    t.Value, mBuyer.Id as mBuyerId from {0} t
                    join LETS_MemberPartRecord mBuyer on mBuyer.Id = t.BuyerMemberPartRecord_Id
                    join LETS_MemberPartRecord mSeller on mSeller.Id = t.SellerMemberPartRecord_Id
                    join LETS_MemberAdminPartRecord maSeller on maSeller.Id = t.SellerMemberPartRecord_Id
                    cross apply dbo.LETS_CustomMaxDateFunction(t.transactionDate, :demurrageStartDate) as mdf
                    where t.TransactionType != 'Demurrage'
                    and maSeller.MemberType = 'Member'
                    and t.CreditValue > 0
                    and DATEDIFF(DAY, mdf.MaxDate, :today) > :days
                    and (select COUNT(id) from {1} cu where cu.TransactionType = 'Demurrage' and cu.IdTransactionEarnt = t.Id) = :step
                ", transactionTableName, creditUsageTableName))
                                            .SetParameter("today", today.ToString("yyyy-MM-dd"))
                                            .SetParameter("demurrageStartDate", demurrageStartDate.ToString("yyyy-MM-dd"))
                                            .SetParameter("days", demurrageDays*(steps.Count + 1))
                                            .SetParameter("step", steps.Count);
            var resultsCreditsToZero = queryCreditsToZero.List<object[]>();
            return resultsCreditsToZero;
        }

        private IEnumerable<object[]> RemoveUnpublishedResults(ICollection<object[]> resultsStep)
        {
            // remove unpublished or deleted results using orchard content query
            foreach (var result in from result in resultsStep.ToList()
                                   let oldTransaction = _orchardServices.ContentManager.Get((int)result[0], VersionOptions.Published)
                                   where oldTransaction == null
                                   select result)
            {
                resultsStep.Remove(result);
            }
            return resultsStep;
        }

        private int RecordDemurrageTransactions(IEnumerable<object[]> oldTransactionRecords, int? percent)
        {
            var chargesRecorded = 0;
            var demurrageRecipient = _memberRepository.Get(m => m.Id == _idDemurrageRecipient);
            var demurrageRecipientBalance = _memberService.GetMemberBalance(_idDemurrageRecipient);
            foreach (var record in oldTransactionRecords)
            {
                var oldTransactionRecord = record;
                var demurrageTransactionItem =
                    _orchardServices.ContentManager.Create<TransactionPart>("Transaction").ContentItem;
                demurrageTransactionItem.As<CommonPart>().Owner = _membershipService.GetUser(_orchardServices.WorkContext.CurrentSite.SuperUser);
                var valueToDemur = (int)oldTransactionRecord[3];
                if (percent != null) valueToDemur = (int) Math.Ceiling((decimal) (valueToDemur*percent/100.0));
                var demurrageTransactionPart = demurrageTransactionItem.As<TransactionPart>();
                demurrageTransactionPart.TransactionDate = DateTime.Now;
                demurrageTransactionPart.TransactionType = TransactionType.Demurrage;
                demurrageTransactionPart.Seller = demurrageRecipient;
                demurrageTransactionPart.Buyer = _memberRepository.Get(m => m.Id == (int)oldTransactionRecord[2]);
                demurrageTransactionPart.Description =
                    T("Unspent credits expired ({0}...)", oldTransactionRecord[4]).ToString();
                demurrageTransactionPart.Value = valueToDemur;
                var newDemurrageRecipientBalance = demurrageRecipientBalance + valueToDemur;
                var creditValue = 0;
                if (newDemurrageRecipientBalance > 0)
                {
                    creditValue = Math.Min(valueToDemur, newDemurrageRecipientBalance);
                }
                demurrageTransactionPart.CreditValue = creditValue;
                var transactionToDemurPartRecord = _transactionRepository.Get((int) oldTransactionRecord[0]);
                transactionToDemurPartRecord.CreditValue -= valueToDemur;
                Debug.Assert(transactionToDemurPartRecord.CreditValue >= 0);
                _transactionRepository.Update(transactionToDemurPartRecord);
                _signals.Trigger(string.Format("letsMemberBal{0}Changed", demurrageTransactionPart.Seller.Id));
                _signals.Trigger(string.Format("letsMemberBal{0}Changed", demurrageTransactionPart.Buyer.Id));
                _signals.Trigger(string.Format("letsMemberTransactionCount{0}Changed", demurrageTransactionPart.Seller.Id));
                _signals.Trigger(string.Format("letsMemberTransactionCount{0}Changed", demurrageTransactionPart.Buyer.Id));

                var creditUsageRecord = new CreditUsageRecord
                                            {
                                                IdTransactionEarnt = transactionToDemurPartRecord.Id,
                                                IdTransactionSpent = demurrageTransactionPart.Id,
                                                RecordedDate = DateTime.Now,
                                                TransactionType = TransactionType.Demurrage,
                                                Value = valueToDemur
                                            };
                _creditUsageRepository.Create(creditUsageRecord);
                chargesRecorded++;
            }
            return chargesRecorded;
        }

        private IList<DemurrageTransactionViewModel> SimulateDemurrageTransactions(IEnumerable<object[]> oldTransactionRecords, int? percent, DateTime today, out int toBeDeducted, out int tradeValue, out int unspentCreditValue)
        {
            var simulatedDemurrageTransactions = new List<DemurrageTransactionViewModel>();
            toBeDeducted = tradeValue = unspentCreditValue = 0;
            foreach (var record in oldTransactionRecords)
            {
                var oldTransactionRecord = record;
                var valueToDemur = (int)oldTransactionRecord[3];
                var creditValue = (int) oldTransactionRecord[3];
                unspentCreditValue += creditValue;
                var transactionValue = (int) oldTransactionRecord[7];
                tradeValue += transactionValue;
                var deductionExplanation = T("Remainder of unspent credit");
                if (percent != null)
                {
                    valueToDemur = (int)Math.Ceiling((decimal)(valueToDemur * percent / 100.0));
                    deductionExplanation = T("{0}% of {1}", percent, creditValue);
                }
                toBeDeducted += valueToDemur;
                var demurrageTransactionSimulationItem = new DemurrageTransactionViewModel
                    {
                        RecordedDate = today,
                        IdTransactionEarnt = (int)oldTransactionRecord[0],
                        TransactionDateEarnt = (DateTime)oldTransactionRecord[1],
                        IdSellerTransactionEarnt = (int) oldTransactionRecord[2],
                        CreditValueTransactionEarnt = creditValue,
                        DescriptionTransactionEarnt = (string) oldTransactionRecord[4],
                        NameBuyerTransactionEarnt = (string) oldTransactionRecord[5],
                        NameSellerTransactionEarnt = (string)oldTransactionRecord[6],
                        ValueTransactionEarnt = transactionValue,
                        IdBuyerTransactionEarnt = (int)oldTransactionRecord[8],
                        ValueDeducted = valueToDemur,
                        DeductionExplanation = deductionExplanation
                    };
                var transactionToDemurRecord = _transactionRecordSimulationRepository.Get((int)oldTransactionRecord[0]);
                transactionToDemurRecord.CreditValue -= valueToDemur;
                _transactionRecordSimulationRepository.Update(transactionToDemurRecord);
                _transactionRecordSimulationRepository.Flush();
                var creditUsageRecordSimulation = new CreditUsageRecordSimulation
                {
                    IdTransactionEarnt = transactionToDemurRecord.Id,
                    RecordedDate = today,
                    TransactionType = TransactionType.Demurrage.ToString(),
                    Value = valueToDemur
                };
                _creditUsageRecordSimulationRepository.Create(creditUsageRecordSimulation);
                _creditUsageRecordSimulationRepository.Flush();
                simulatedDemurrageTransactions.Add(demurrageTransactionSimulationItem);
            }
            return simulatedDemurrageTransactions;
        }

        private int GetCreditValueForNewTransaction(EditTransactionViewModel model)
        {
            Debug.Assert(model.IdSeller != null, "model.IdSeller != null because validation ensures that an id is submitted");
            Debug.Assert(model.Value != null, "model.Value != null because validation ensures a value is submitted");
            var value = (int) model.Value;
            var newSellerBalance = _memberService.GetMemberBalance((int) model.IdSeller) + value;
            var creditValue = 0;
            if (newSellerBalance > 0)
            {
                creditValue = Math.Min(value, newSellerBalance);
            }
            return creditValue;
        }
    }
}