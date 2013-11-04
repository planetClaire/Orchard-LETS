using Orchard.ContentManagement.Records;

namespace LETS.Models
{
    public class LocalityPartRecord : ContentPartRecord
    {
        public virtual string Postcode { get; set; }
    }
}