using System;
using Orchard.ContentManagement.Records;

namespace LETS.Models
{
    public class TransactionPartRecord : ContentPartRecord
    {
        public virtual DateTime? TransactionDate { get; set; }
        public virtual string Description { get; set; }
        public virtual NoticePartRecord NoticePartRecord { get; set; }
        public virtual MemberPartRecord SellerMemberPartRecord { get; set; }
        public virtual MemberPartRecord BuyerMemberPartRecord { get; set; }
        public virtual TransactionType TransactionType { get; set; }
        public virtual int Value { get; set; }
        public virtual int CreditValue { get; set; }
    }
}