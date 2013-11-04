using System;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;

namespace LETS.Models
{
    public class TransactionPart : ContentPart<TransactionPartRecord>
    {
        public DateTime? TransactionDate
        {
            get { return Record.TransactionDate; }
            set { if (value != null) Record.TransactionDate = (DateTime)value; }
        }

        public string Description
        {
            get { return Record.Description; }
            set { Record.Description = value; }
        }

        public NoticePartRecord Notice
        {
            get { return Record.NoticePartRecord; }
            set { Record.NoticePartRecord = value; }
        }

        [Required(ErrorMessage = "The seller is required")]
        public MemberPartRecord Seller
        {
            get { return Record.SellerMemberPartRecord; }
            set { Record.SellerMemberPartRecord = value; }
        }

        public MemberPartRecord Buyer
        {
            get { return Record.BuyerMemberPartRecord; }
            set { Record.BuyerMemberPartRecord = value; }
        }

        public TransactionType TransactionType
        {
            get { return Record.TransactionType; }
            set { Record.TransactionType = value; }
        }

        public int Value
        {
            get { return Record.Value; }
            set { Record.Value = value; }
        }

        public int CreditValue
        {
            get { return Record.CreditValue; }
            set { Record.CreditValue = value; }
        }
    }
}