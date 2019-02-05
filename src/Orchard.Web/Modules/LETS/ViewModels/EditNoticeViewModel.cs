using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using LETS.Models;

namespace LETS.ViewModels
{
    public class EditNoticeViewModel 
    {
        [Required(ErrorMessage="The Notice Type is required")]
        public int? IdNoticeType { get; set; }

        [Range(1, 500, ErrorMessage = "Please enter a number between 1 and 500")]
        public int? Price { get; set; }

        public string CurrencyUnit { get; set; }

        public IEnumerable<NoticeTypePart> NoticeTypes { get; set; }

    }
}