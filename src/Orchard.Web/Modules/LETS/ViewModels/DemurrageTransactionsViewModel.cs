using System;
using System.Collections.Generic;

namespace LETS.ViewModels
{
    public class DemurrageTransactionsViewModel
    {
        public DateTime DemurrageDate { get; set; }
        public int TradeValue { get; set; }
        public int UnspentCreditValue { get; set; }
        public int ToBeDeducted { get; set; }
        public IList<DemurrageTransactionViewModel> DemurrageTransactions { get; set; }
    }
}