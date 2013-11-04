using System;

namespace LETS.Models
{
    public class CreditUsageRecord
    {
        public virtual int Id { get; set; }
        public virtual DateTime RecordedDate { get; set; }
        public virtual int IdTransactionEarnt { get; set; }
        public virtual int IdTransactionSpent { get; set; }
        public virtual TransactionType TransactionType { get; set; }
        public virtual int Value { get; set; }
    }
}