using System;

namespace LETS.Models
{
    public class CreditUsageRecordSimulation 
    {
        public virtual int Id { get; set; }
        public virtual int IdCreditUsage { get; set; }
        public virtual DateTime RecordedDate { get; set; }
        public virtual int IdTransactionEarnt { get; set; }
        public virtual int IdTransactionSpent { get; set; }
        public virtual string TransactionType { get; set; }
        public virtual int Value { get; set; }
    }
}