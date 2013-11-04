using System.Web.Routing;
using LETS.Models;
using LETS.Services;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Feeds;
using Orchard.Localization;

namespace LETS.Drivers
{
    public class CommentsWidgetPartDriver : ContentPartDriver<CommentsWidgetPart>
    {
        private readonly ILETSService _letsService;
        private readonly IFeedManager _feedManager;
        public Localizer T { get; set; }

        public CommentsWidgetPartDriver(ILETSService letsService, IFeedManager feedManager) {
            _letsService = letsService;
            _feedManager = feedManager;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(CommentsWidgetPart part, string displayType, dynamic shapeHelper)
        {

            _feedManager.Register(T("Comments").ToString(), "rss", new RouteValueDictionary { { "comments", "all" } });
            return ContentShape("CommentsWidget", () => shapeHelper.CommentsWidget(
                LatestComments: _letsService.GetLatestComments(3)
                ));
        }
    }
}