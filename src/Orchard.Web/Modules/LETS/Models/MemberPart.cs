using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using Orchard.Users.Models;
using Orchard.ContentManagement.Utilities;

namespace LETS.Models
{
    public class MemberPart : ContentPart<MemberPartRecord>
    {
        [Required(ErrorMessage="Your first name is required"), DisplayName("First name/s")]
        public string FirstName
        {
            get { return Record.FirstName; }
            set { Record.FirstName = value; }
        }

        [Required(ErrorMessage = "Your last name is required")]
        public string LastName
        {
            get { return Record.LastName; }
            set { Record.LastName = value; }
        }

        [Required(ErrorMessage = "A telephone number is required")]
        public string Telephone
        {
            get { return Record.Telephone; }
            set { Record.Telephone = value; }
        }

        public UserPart User
        {
            get { return this.As<UserPart>(); }
        }

        public string FirstLastName
        {
            get { return string.Format("{0} {1}", FirstName, LastName); }
        }

        public string LastFirstName
        {
            get { return string.Format("{0}, {1}", LastName, FirstName); }
        }

        private readonly LazyField<string> _locality = new LazyField<string>();
        internal LazyField<string> LocalityField { get { return _locality; } }
        public string Locality { get { return _locality.Value; } }
                
    }
}