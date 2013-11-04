using System;

namespace LETS.ViewModels
{
    public class MemberTransactionViewModel
    {
        public int Id { get; set; }
        public DateTime TransactionDate { get; set; }
        public string TransactionType { get; set; }
        public int IdTradingPartner { get; set; }
        public string TradingPartner { get; set; }
        public string UserName { get; set; }
        public string Description { get; set; }
        public int Value { get; set; }
        public int CreditValue { get; set; }
        public int RunningTotal { get; set; }
    }
}