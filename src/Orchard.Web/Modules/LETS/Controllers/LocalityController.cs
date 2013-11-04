using System.Web.Mvc;
using LETS.Models;
using LETS.Services;
using LETS.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.DisplayManagement.Shapes;
using Orchard.Mvc;
using Orchard.Themes;

namespace LETS.Controllers
{
    public class LocalityController : Controller
    {
        private readonly IOrchardServices _orchardServices;
        private readonly INoticeService _noticeService;
        private readonly IContentManager _contentManager;
        private readonly IMemberService _memberService;

        public LocalityController(IOrchardServices orchardServices, INoticeService noticeService, IContentManager contentManager, IMemberService memberService) {
            _orchardServices = orchardServices;
            _noticeService = noticeService;
            _contentManager = contentManager;
            _memberService = memberService;
        }

        [Themed]
        public ActionResult Index(int id) {
            return View();
        }

        [Themed]
        public ActionResult Notices(int id) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.AccessMemberContent))
                return new HttpUnauthorizedResult();

            var notices = _noticeService.GetNoticesByLocality(id);
            var locality = _contentManager.Get<LocalityPart>(id);
            var members = _memberService.GetMembersByLocality(id);

            var localityNoticesMembersViewModel = new LocalityNoticesMembersViewModel { Notices = notices, Locality = locality, Members = members };

            return View(localityNoticesMembersViewModel);
        }
    }
}