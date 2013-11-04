using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;

namespace LETS.Models
{
    public class LocalityPart : ContentPart<LocalityPartRecord>
    {
        public string Title
        {
            get { return this.As<TitlePart>().Title; }
        }

        [Required]
        public string Postcode
        {
            get { return Record.Postcode; }
            set { Record.Postcode = value; }
        }

    }
}