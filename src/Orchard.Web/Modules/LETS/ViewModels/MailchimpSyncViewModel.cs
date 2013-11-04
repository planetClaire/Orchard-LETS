using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LETS.ViewModels
{
    public class MailchimpSyncViewModel
    {
        public IEnumerable<MailchimpMemberViewModel> MissingOrchardMembers { get; set; }
        public IEnumerable<MailchimpMemberViewModel> MissingListMembers { get; set; }

    }
}