using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using LETS.Helpers;

namespace LETS.ViewModels
{
    public class RegisterNoticeTypesViewModel
    {
        public RegisterNoticeTypesViewModel()
        {
            NoticeTypes = new List<RegisterNoticesViewModel>();
        }

        public List<RegisterNoticesViewModel> NoticeTypes { get; set; }

        public IEnumerable<GroupedSelectListItem> CategoryTerms { get; set; }
    }

    public class RegisterNoticesViewModel
    {
        public RegisterNoticesViewModel() { }

        public RegisterNoticesViewModel(int requiredCount)
        {
            RequiredCount = requiredCount;
            Notices = new List<RegisterNoticeViewModel>();
            for (var i=0 ; i<requiredCount; i++)
            {
                Notices.Add(new RegisterNoticeViewModel());
            }
        }

        public int? IdNoticeType { get; set; }

        public string NoticeTypeName { get; set; }

        public int RequiredCount { get; set; }

        public List<RegisterNoticeViewModel> Notices { get; set; }

    }

    public class RegisterNoticeViewModel
    {
        [Required(ErrorMessage="You must provide a notice here")]
        public string Title { get; set; }

        [Required, DisplayName("Category")]
        public int IdCategoryTerm { get; set; }
    }
}