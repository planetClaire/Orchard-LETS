using Orchard.ContentManagement.Records;

namespace LETS.Models
{
    public class MemberPartRecord : ContentPartRecord
    {
        public virtual string FirstName { get; set; }
        public virtual string LastName { get; set; }
        public virtual string Telephone { get; set; }
    }
}