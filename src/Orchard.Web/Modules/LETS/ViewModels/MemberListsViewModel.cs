using System.Collections.Generic;

namespace LETS.ViewModels
{
    public class MemberListsViewModel
    {
        public List<MemberListViewModel> MemberLists { get; set; }

        public MemberListsViewModel() {
            MemberLists = new List<MemberListViewModel>();
        }
    }
}