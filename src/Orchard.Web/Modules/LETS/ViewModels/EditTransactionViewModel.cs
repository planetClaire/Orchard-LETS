using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using LETS.Helpers;
using LETS.Models;

namespace LETS.ViewModels
{
    public class EditTransactionViewModel
    {
        [Required]
        public string TransactionDate { get; set; }

        public string TransactionTime { get; set; }

        [Required]
        public TransactionType TransactionType { get; set; }

        [Required]
        public string Description { get; set; }

        [Required(ErrorMessage = "The Seller is required")]
        public int? IdSeller { get; set; }

        [Required(ErrorMessage = "The Buyer is required")]
        public int? IdBuyer { get; set; }

        public IEnumerable<GroupedSelectListItem> Members { get; set; }

        public string CurrencyUnit { get; set; }

        [Required, Range(1, 999999, ErrorMessage = "Please enter a number between 1 and 999999")]
        public int? Value { get; set; }

        public int CreditValue { get; set; }
    }
}