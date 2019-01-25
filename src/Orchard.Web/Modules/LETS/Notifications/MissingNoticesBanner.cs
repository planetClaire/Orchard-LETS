using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using LETS.Models;
using LETS.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Notify;

namespace LETS.Notifications
{
    public class MissingNoticesBanner : INotificationProvider
    {
        private readonly IOrchardServices _orchardServices;
        private readonly INoticeService _noticeService;
        public Localizer T { get; set; }

        public MissingNoticesBanner(IOrchardServices orchardServices, INoticeService noticeService)
        {
            _orchardServices = orchardServices;
            _noticeService = noticeService;
            T = NullLocalizer.Instance;
        }


        public IEnumerable<NotifyEntry> GetNotifications()
        {
            if (!_orchardServices.Authorizer.Authorize(Permissions.AccessMemberContent))
                yield break;

            var currentUser = _orchardServices.WorkContext.CurrentUser;
            if (currentUser.As<MemberAdminPart>().MemberType != MemberType.Member) {
                yield break;
            }
            var noticeTypes = _noticeService.GetRequiredNoticeTypes();
            foreach (var noticeType in noticeTypes)
            {
                var memberNoticesCount = _noticeService.GetMemberNotices(currentUser.Id, noticeType.Id).Count();
                if (memberNoticesCount.Equals(0))
                {
                    var urlHelper = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
                    yield return new NotifyEntry { Message = T("You don't have any {0}s, please <a href=\"{1}\">add some</a>.", noticeType.Title.ToLower(), urlHelper.Action("Create", "Notice", new { area = "LETS" })), Type = NotifyType.Error };
                }
                else if (memberNoticesCount < noticeType.RequiredCount)
                {
                    yield return new NotifyEntry { Message = T("You only have {0} {1} but you should have atleast {2}. Please add more.", memberNoticesCount, noticeType.Title, noticeType.RequiredCount), Type = NotifyType.Warning };
                }
            }

        }
    }
}