using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using LETS.Helpers;

namespace LETS.ViewModels
{
    public class NoticeCategoryViewModel
    {
        [Required, DisplayName("Category")]
        public int IdCategory { get; set; }

        public IEnumerable<GroupedSelectListItem> Categories { get; set; }

    }
}