using System;
using System.Web.Mvc;
using System.Xml.Linq;
using JetBrains.Annotations;
using LETS.Services;
using Orchard.Core.Common.Models;
using Orchard.Core.Feeds;
using Orchard.Core.Feeds.Models;
using Orchard.Localization;
using Orchard.Utility.Extensions;

namespace LETS.Feeds
{
    [UsedImplicitly]
    public class NoticeTypeFeedQuery : IFeedQueryProvider, IFeedQuery
    {
        private readonly INoticeService _noticeService;
        public Localizer T;

        public NoticeTypeFeedQuery(INoticeService noticeService)
        {
            _noticeService = noticeService;
            T = NullLocalizer.Instance;
        }

        public FeedQueryMatch Match(FeedContext context)
        {
            if (context.ValueProvider.GetValue("idnoticetype") != null)
            {
                return new FeedQueryMatch { Priority = -1, FeedQuery = this };
            }
            return null;
        }

        public void Execute(FeedContext context)
        {
            var idNoticeType = (int)context.ValueProvider.GetValue("idnoticetype").ConvertTo(typeof(int));

            var limit = 20;
            var limitValue = context.ValueProvider.GetValue("limit");
            if (limitValue != null)
                limit = (int)limitValue.ConvertTo(typeof(int));

            var title = string.Format("{0} {1}", T("Latest notices: "), _noticeService.GetNoticeTypeTitle(idNoticeType));
            var description = T("Latest LETS notices");
            if (context.Format == "rss")
            {
                var link = new XElement("link");
                context.Response.Element.SetElementValue("title", title);
                context.Response.Element.Add(link);
                context.Response.Element.SetElementValue("description", description);

                context.Response.Contextualize(requestContext =>
                {
                    var urlHelper = new UrlHelper(requestContext);
                    var uriBuilder = new UriBuilder(urlHelper.RequestContext.HttpContext.Request.ToRootUrlString());
                    link.Add(uriBuilder.Uri.OriginalString);
                });
            }
            else
            {
                context.Builder.AddProperty(context, null, "title", title);
                context.Builder.AddProperty(context, null, "description", description.ToString());
                context.Response.Contextualize(requestContext =>
                {
                    var urlHelper = new UrlHelper(requestContext);
                    context.Builder.AddProperty(context, null, "link", urlHelper.RouteUrl("/"));
                });
            }

            var notices =
                _noticeService.GetNotices(idNoticeType)
                              .OrderByDescending<CommonPartRecord>(c => c.PublishedUtc)
                              .Slice(0, limit);

            foreach (var notice in notices)
            {
                context.Builder.AddItem(context, notice);
            }
        }
    }
}