using System.Collections.Generic;
using LETS.Models;

namespace LETS.ViewModels
{
    public class MemberNoticesViewModel
    {
        public bool AdminIsViewing { get; set; }
        public MemberPart Member { get; set; }
        public IEnumerable<dynamic> Notices { get; set; }
        public IEnumerable<dynamic> ArchivedNotices { get; set; }
    }
}