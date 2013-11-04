using System.Collections.Generic;
using LETS.Models;
using LETS.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.UI.Navigation;

namespace LETS.Services
{
    public interface ITransactionService : IDependency
    {
        void SaveTransaction(ContentItem contentItem, EditTransactionViewModel model);
        IEnumerable<MemberTransactionViewModel> GetTransactions(int idMember, int count, int page = 1);
        int GetTransactionCount(int idMember);
        void CorrectMemberCreditValues(int idMember);
        void ProcessDemurrage();
        IEnumerable<DemurrageTransactionViewModel> GetDemurrages();
        List<DemurrageTransactionsViewModel> ForecastDemurrage(int idMember = 0);
        void DeleteCreditUsage(int idCreditUsage);
        void DeleteDemurrageCreditUsages(int idTransaction);
        IList<CreditUsageRecord> FindOrphanedDemurrageCreditUsages();
    }
}