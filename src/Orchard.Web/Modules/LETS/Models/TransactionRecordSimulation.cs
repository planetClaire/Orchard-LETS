using System;

namespace LETS.Models
{
    public class TransactionRecordSimulation
    {
        public virtual int Id { get; set; }
        public virtual int IdTransaction { get; set; }
        public virtual DateTime? TransactionDate { get; set; }
        public virtual string Description { get; set; }
        public virtual int SellerMemberPartRecord_Id { get; set; }
        public virtual int BuyerMemberPartRecord_Id { get; set; }
        public virtual string TransactionType { get; set; }
        public virtual int Value { get; set; }
        public virtual int CreditValue { get; set; }
    }
}