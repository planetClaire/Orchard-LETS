using LETS.Models;
using Orchard.Localization;

namespace LETS.ViewModels
{
    public class AdminAlertViewModel
    {
        public LocalizedString ReferenceLinkText { get; set; }
        public string ReferenceLinkHref { get; set; }
        public LocalizedString Message { get; set; }
        public LocalizedString ActionLinkText { get; set; }
        public string ActionLinkHref { get; set; }
    }
}