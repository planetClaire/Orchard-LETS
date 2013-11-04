using System.Collections.Generic;
using LETS.Models;

namespace LETS.ViewModels
{
    public class NoticeTypeNoticesViewModel
    {
        public IEnumerable<dynamic> Notices { get; set; }
        public string NoticeTypeTitle { get; set; }
        public dynamic Pager { get; set; }
    }
}