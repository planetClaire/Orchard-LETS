using LETS.Models;
using LETS.Services;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace LETS.Handlers
{
    public class TransactionPartHandler : ContentHandler
    {
        public TransactionPartHandler(IRepository<TransactionPartRecord> repository, ITransactionService transactionService)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }

}