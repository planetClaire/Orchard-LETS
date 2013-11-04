using System.Collections.Generic;
using LETS.Models;

namespace LETS.ViewModels
{
    public class MemberListContentsViewModel
    {
        public string ContentTypeName { get; set; }
        public MemberPart MemberPart { get; set; }
        public IEnumerable<dynamic> ContentSummaries { get; set; }
    }
}