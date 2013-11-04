using System;
using Orchard.Localization;

namespace LETS.ViewModels
{
    public class DemurrageTransactionViewModel
    {
        public DateTime RecordedDate { get; set; }
        public DateTime TransactionDateEarnt { get; set; }
        public string DescriptionTransactionEarnt { get; set; }
        public int IdTransactionEarnt { get; set; }
        public int IdSellerTransactionEarnt { get; set; }
        public string NameSellerTransactionEarnt { get; set; }
        public int IdBuyerTransactionEarnt { get; set; }
        public string NameBuyerTransactionEarnt { get; set; }
        public int ValueTransactionEarnt { get; set; }
        public int ValueDeducted { get; set; }
        public int CreditValueTransactionEarnt { get; set; }
        public int IdDemurrageTransaction { get; set; }
        public int IdDemurrageRecipient { get; set; }
        public string NameDemurrageRecipient { get; set; }
        public LocalizedString DeductionExplanation { get; set; }
    }
}