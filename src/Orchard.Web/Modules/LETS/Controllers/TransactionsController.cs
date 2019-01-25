using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using LETS.Models;
using LETS.Services;
using LETS.ViewModels;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Settings.Models;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Localization.Services;
using Orchard.Security;
using Orchard.Themes;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;

namespace LETS.Controllers
{
    public class TransactionsController : Controller, IUpdateModel
    {
        private readonly IOrchardServices _orchardServices;
        private readonly IWorkContextAccessor _workContext;
        private readonly IMemberService _memberService;
        private readonly IContentManager _contentManager;
        private readonly ITransactionService _transactionService;
        private readonly IDateTimeFormatProvider _dateTimeLocalization;
        private readonly IAuthorizationService _authorizationService;
        private readonly ISignals _signals;
        public Localizer T { get; set; }
        dynamic Shape { get; set; }

        public TransactionsController(IOrchardServices orchardServices, IWorkContextAccessor workContext, IMemberService memberService, IContentManager contentManager, ITransactionService transactionService, IDateTimeFormatProvider dateTimeLocalization, IShapeFactory shapeFactory, IAuthorizationService authorizationService, ISignals signals)
        {
            _orchardServices = orchardServices;
            _workContext = workContext;
            _memberService = memberService;
            _contentManager = contentManager;
            _transactionService = transactionService;
            _dateTimeLocalization = dateTimeLocalization;
            T = NullLocalizer.Instance;
            Shape = shapeFactory;
            _authorizationService = authorizationService;
            _signals = signals;
        }

        [Themed]
        public ActionResult Index(PagerParameters pagerParameters)
        {
            if (!_orchardServices.Authorizer.Authorize(Permissions.AccessMemberContent))
            {
                return new HttpUnauthorizedResult();
            }
            var currentUser = _workContext.GetContext().CurrentUser;
            var idMember = currentUser.Id;
            var memberTypes = new List<MemberType> { MemberType.Member, MemberType.Admin, MemberType.LETSystem };
            var editTransactionViewModel = new EditTransactionViewModel
            {
                CurrencyUnit = _workContext.GetContext().CurrentSite.As<LETSSettingsPart>().CurrencyUnit,
                IdSeller = idMember,
                Members = _memberService.GetGroupedMembers(memberTypes, idMember),
                TransactionType = TransactionType.Trade
            };
            var pager = new Pager(_orchardServices.WorkContext.CurrentSite, pagerParameters);
            var pagerShape = Shape.Pager(pager).TotalItemCount(_transactionService.GetTransactionCount(idMember));
            var balance = _memberService.GetMemberBalance(idMember, true);
            var transactions = _transactionService.GetTransactions(idMember, pager.PageSize, pager.Page).ToList();
            var firstTransaction = transactions.FirstOrDefault();
            var memberTransactionsViewModel = new MemberTransactionsViewModel {
                AdminIsViewing = firstTransaction != null && _authorizationService.TryCheckAccess(Permissions.AdminMemberContent, currentUser, _contentManager.Get(firstTransaction.Id)),
                Transactions = transactions,
                Member = _memberService.GetMember(idMember),
                Balance = balance,
                OldestCreditValueTransaction = balance > 0 ? _memberService.GetOldestCreditValueTransaction(idMember) : null,
                NewTransaction = editTransactionViewModel,
                Pager = pagerShape
            };
            return View(memberTransactionsViewModel);
        }

        [Themed, HttpPost, ActionName("Index")]
        public ActionResult AddPOST(MemberTransactionsViewModel memberTransactionsViewModel)
        {
            if (!_orchardServices.Authorizer.Authorize(Permissions.AccessMemberContent))
            {
                return new HttpUnauthorizedResult();
            }
            if (ModelState.IsValid)
            {
                var newTransaction = memberTransactionsViewModel.NewTransaction;
                newTransaction.TransactionTime = DateTime.Now.ToString(_dateTimeLocalization.ShortTimeFormat); 
                _transactionService.SaveTransaction(_contentManager.Create<TransactionPart>("Transaction").ContentItem, newTransaction);
                _orchardServices.Notifier.Information(T("Your trade has been recorded."));
                return RedirectToAction("Index");
            }
            var idMember = _workContext.GetContext().CurrentUser.Id;
            memberTransactionsViewModel.Transactions = _transactionService.GetTransactions(idMember, _workContext.GetContext().CurrentSite.As<SiteSettingsPart>().PageSize);
            var memberTypes = new List<MemberType> { MemberType.Member, MemberType.Admin, MemberType.LETSystem };
            memberTransactionsViewModel.NewTransaction.Members = _memberService.GetGroupedMembers(memberTypes, idMember);
            memberTransactionsViewModel.NewTransaction.CurrencyUnit = _workContext.GetContext().CurrentSite.As<LETSSettingsPart>().CurrencyUnit;
            return View(memberTransactionsViewModel);
        }


        [Themed]
        public ActionResult Edit(int id) {
            var transaction = _contentManager.Get(id, VersionOptions.Latest);
            if (!_authorizationService.TryCheckAccess(Orchard.Core.Contents.Permissions.EditContent, _orchardServices.WorkContext.CurrentUser, transaction))
            {
                return new HttpUnauthorizedResult();
            }
            var transactionModel = _contentManager.BuildEditor(transaction);
            return View((object)transactionModel);
        }

        [Themed]
        [HttpPost, ActionName("Edit")]
        public ActionResult EditPOST(int id)
        {
            var transaction = _contentManager.Get(id, VersionOptions.Latest);
            if (!_authorizationService.TryCheckAccess(Orchard.Core.Contents.Permissions.EditContent, _orchardServices.WorkContext.CurrentUser, transaction))
            {
                return new HttpUnauthorizedResult();
            }
            var transactionModel = _contentManager.UpdateEditor(transaction, this);
            if (!ModelState.IsValid)
            {
                return View((object)transactionModel);
            }
            _transactionService.CorrectMemberCreditValues(transaction.As<TransactionPart>().Seller.Id);
            _transactionService.CorrectMemberCreditValues(transaction.As<TransactionPart>().Buyer.Id);
            _signals.Trigger("letsMemberListChanged");
            _signals.Trigger(string.Format("letsMemberBal{0}Changed", transaction.As<TransactionPart>().Seller.Id));
            _signals.Trigger(string.Format("letsMemberBal{0}Changed", transaction.As<TransactionPart>().Buyer.Id));
            _signals.Trigger(string.Format("letsMemberTransactionCount{0}Changed", transaction.As<TransactionPart>().Seller.Id));
            _signals.Trigger(string.Format("letsMemberTransactionCount{0}Changed", transaction.As<TransactionPart>().Buyer.Id));
            _signals.Trigger(string.Format("letsTotalTurnoverChanged"));
            if (transaction.As<TransactionPart>().TransactionType.Equals(TransactionType.Trade))
            {
                _signals.Trigger(string.Format("letsMemberTurnover{0}Changed", transaction.As<TransactionPart>().Seller.Id));
                _signals.Trigger(string.Format("letsMemberTurnover{0}Changed", transaction.As<TransactionPart>().Buyer.Id));
            }
            _orchardServices.Notifier.Information(T("Your Transaction has been updated."));
            return RedirectToAction("Index", "Transactions", new { area = "LETS" });
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties)
        {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage)
        {
            ModelState.AddModelError(key, errorMessage.ToString());
        }

        public ActionResult Delete(int id) {
            var transaction = _contentManager.Get<TransactionPart>(id, VersionOptions.Latest);
            if (!_authorizationService.TryCheckAccess(Orchard.Core.Contents.Permissions.DeleteContent, _orchardServices.WorkContext.CurrentUser, transaction))
            {
                return new HttpUnauthorizedResult();
            }
            var idUser = transaction.As<CommonPart>().Owner.Id;
            _signals.Trigger("letsMemberListChanged");
            _signals.Trigger(string.Format("letsMemberBal{0}Changed", transaction.Seller.Id));
            _signals.Trigger(string.Format("letsMemberBal{0}Changed", transaction.Buyer.Id));
            _signals.Trigger(string.Format("letsMemberTransactionCount{0}Changed", transaction.Seller.Id));
            _signals.Trigger(string.Format("letsMemberTransactionCount{0}Changed", transaction.Buyer.Id));
            _signals.Trigger(string.Format("letsTotalTurnoverChanged"));
            if (transaction.As<TransactionPart>().TransactionType.Equals(TransactionType.Trade))
            {
                _signals.Trigger(string.Format("letsMemberTurnover{0}Changed", transaction.Seller.Id));
                _signals.Trigger(string.Format("letsMemberTurnover{0}Changed", transaction.Buyer.Id));
            }
            _contentManager.Remove(transaction.ContentItem);
            if (transaction.TransactionType.Equals(TransactionType.Demurrage))
            {
                _transactionService.DeleteDemurrageCreditUsages(transaction.Id);
            }
            //_contentManager.Flush();
            _transactionService.CorrectMemberCreditValues(transaction.Seller.Id);
            _transactionService.CorrectMemberCreditValues(transaction.Buyer.Id);
            _orchardServices.Notifier.Information(T("Your Trade has been deleted."));
            return RedirectToAction("Index", new { id = idUser });
        }

        [Themed]
        public ActionResult Demurrage()
        {
            if (!_orchardServices.Authorizer.Authorize(Permissions.AccessMemberContent))
            {
                return new HttpUnauthorizedResult();
            }
            var idMember = _workContext.GetContext().CurrentUser.Id;
            var demurrageForecastVM = new DemurrageForecastViewModel
                {
                    MemberBalance = _memberService.GetMemberBalance(idMember),
                    DemurrageEvents = _transactionService.ForecastDemurrage(idMember)
                };
            return View(demurrageForecastVM);
        }
    }
}
