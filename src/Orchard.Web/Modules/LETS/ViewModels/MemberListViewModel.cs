using System.Collections.Generic;

namespace LETS.ViewModels
{
    public class MemberListViewModel 
    {
        public string MemberType { get; set; }
        public IEnumerable<MemberViewModel> Members { get; set; }
    }
}