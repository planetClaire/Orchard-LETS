using System.Collections.Generic;
using LETS.Models;

namespace LETS.ViewModels
{
    public class MemberTransactionsViewModel
    {
        public bool AdminIsViewing { get; set; }
        public dynamic Pager { get; set; }
        public MemberPart Member { get; set; }
        public int Balance { get; set; }
        public TransactionPartRecord OldestCreditValueTransaction { get; set; }
        public IEnumerable<MemberTransactionViewModel> Transactions { get; set; }
        public EditTransactionViewModel NewTransaction { get; set; }
    }
}