using System.Web.Mvc;
using LETS.Models;
using LETS.Services;
using LETS.ViewModels;
using Orchard.ContentManagement;
using Orchard.Security;
using Orchard.UI.Admin;

namespace LETS.Controllers
{
    public class NoticesAdminController : Controller
    {
        private readonly INoticeService _noticeService;
        private readonly IContentManager _contentManager;

        public NoticesAdminController(INoticeService noticeService, IContentManager contentManager) {
            _noticeService = noticeService;
            _contentManager = contentManager;
        }

        [Admin]
        public ActionResult List(int id) {
            var member = _contentManager.Get(id, VersionOptions.Latest).As<MemberPart>();
            var memberNoticesViewModel = new MemberNoticesViewModel { Notices = _noticeService.GetMemberNoticeShapes(id), ArchivedNotices = _noticeService.GetMemberArchivedNoticeShapes(id), Member = member, AdminIsViewing = true };
            return View(memberNoticesViewModel);
        }
    }
}
