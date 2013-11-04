using System.Web.Mvc;
using LETS.Services;
using LETS.ViewModels;
using Orchard;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Shapes;
using Orchard.Themes;
using Orchard.UI.Navigation;

namespace LETS.Controllers {
    public class NoticeTypeController : Controller {
        private readonly INoticeService _noticeService;
        private readonly IOrchardServices _orchardServices;
        dynamic Shape { get; set; }

        public NoticeTypeController(INoticeService noticeService, IOrchardServices orchardServices, IShapeFactory shapeFactory)
        {
            _noticeService = noticeService;
            _orchardServices = orchardServices;
            Shape = shapeFactory;
        }

        [Themed]
        public ActionResult Index() {
            return View(_noticeService.GetNoticeTypes());
        }

        [Themed]
        public ActionResult List(int id, PagerParameters pagerParameters)
        {
            var pager = new Pager(_orchardServices.WorkContext.CurrentSite, pagerParameters);
            var pagerShape = Shape.Pager(pager).TotalItemCount(_noticeService.GetNoticeCountByType(id));
            var noticeTypeNoticesViewModel = new NoticeTypeNoticesViewModel {
                Notices = _noticeService.GetNoticeShapesByType(id, pager.Page, pager.PageSize),
                Pager = pagerShape,
                NoticeTypeTitle = _noticeService.GetNoticeTypeTitle(id)
            };

            return View(noticeTypeNoticesViewModel);
        }
    }
}