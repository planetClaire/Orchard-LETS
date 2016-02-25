using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace LETS.ViewModels
{
    public class NoticeCategoryViewModel
    {
        [Required, DisplayName("Category")]
        public int IdCategory { get; set; }

        public IEnumerable<SelectListItem> Categories { get; set; }

    }
}