using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using Orchard.Users.Models;

namespace LETS.Models
{
    public class AddressPart : ContentPart<AddressPartRecord>
    {
        public UserPart User
        {
            get { return this.As<UserPart>(); }
        }

        [Required]
        public string StreetAddress
        {
            get { return Record.StreetAddress; }
            set { Record.StreetAddress = value; }
        }

        public string LatLong {
            get { return Record.LatLong; }
            set { Record.LatLong = value; }
        }

        [Required]
        public LocalityPartRecord Locality
        {
            get { return Record.LocalityPartRecord; }
            set { Record.LocalityPartRecord= value; }
        }
    }
}