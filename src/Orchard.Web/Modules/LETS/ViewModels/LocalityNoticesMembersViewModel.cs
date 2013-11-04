using System.Collections.Generic;
using LETS.Models;

namespace LETS.ViewModels
{
    public class LocalityNoticesMembersViewModel
    {
        public LocalityPart Locality { get; set; }
        public IEnumerable<dynamic> Notices { get; set; }
        public IEnumerable<dynamic> Members { get; set; }
    }
}