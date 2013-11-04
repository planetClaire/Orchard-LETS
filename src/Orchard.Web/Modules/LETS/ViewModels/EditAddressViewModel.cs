using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace LETS.ViewModels
{
    public class EditAddressViewModel
    {
        [Required, DisplayName("Street address")]
        public string StreetAddress { get; set; }

        [Required, DisplayName("Locality")]
        public int IdLocality { get; set; }

        public IEnumerable<LocalityViewModel> Localities { get; set; }
    }
}