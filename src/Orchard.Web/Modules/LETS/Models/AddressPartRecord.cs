using Orchard.ContentManagement.Records;

namespace LETS.Models
{
    public class AddressPartRecord : ContentPartRecord
    {
        public virtual LocalityPartRecord LocalityPartRecord { get; set; }
        public virtual string StreetAddress { get; set; }
        public virtual string LatLong { get; set; }
    }
}