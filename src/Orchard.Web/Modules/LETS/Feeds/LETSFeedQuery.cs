using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Xml.Linq;
using LETS.Models;
using LETS.Services;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Feeds;
using Orchard.Core.Feeds.Models;
using Orchard.Core.Feeds.StandardBuilders;
using Orchard.Localization;
using Orchard.Utility.Extensions;

namespace LETS.Feeds
{
    public class LETSFeedQuery : IFeedQueryProvider, IFeedQuery
    {
        private readonly IMemberService _memberService;
        private readonly ILETSService _letsService;
        private readonly INoticeService _noticeService;
        public Localizer T;

        public LETSFeedQuery(IMemberService memberService, ILETSService letsService, INoticeService noticeService) {
            _memberService = memberService;
            _letsService = letsService;
            _noticeService = noticeService;
            T = NullLocalizer.Instance;
        }

        public FeedQueryMatch Match(FeedContext context) {
            if (context.ValueProvider.GetValue("lets") != null)
            {
                return new FeedQueryMatch { Priority = -1, FeedQuery = this };
            }
            return null;
        }

        public void Execute(FeedContext context) {
            var limit = 20;
            var limitValue = context.ValueProvider.GetValue("limit");
            if (limitValue != null)
                limit = (int)limitValue.ConvertTo(typeof(int));

            var title = T("Latest Notices, new Members, latest Comments");
            var description = T("Latest LETS members, notices and comments");
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
                context.Builder.AddProperty(context, null, "title", title.ToString());
                context.Builder.AddProperty(context, null, "description", description.ToString());
                context.Response.Contextualize(requestContext =>
                {
                    var urlHelper = new UrlHelper(requestContext);
                    context.Builder.AddProperty(context, null, "link", urlHelper.RouteUrl("/"));
                });
            }

            var members = _memberService.QueryMembers().OrderByDescending<MemberAdminPartRecord>(m => m.JoinDate).Slice(0, limit);
            var comments = _letsService.GetLatestComments(limit);
            var notices = _noticeService.GetNotices().OrderByDescending<CommonPartRecord>(c => c.PublishedUtc).Slice(0, limit);

            var items = new List<IContent>();
            items.AddRange(members);
            items.AddRange(comments);
            items.AddRange(notices);
            items.Sort((content, content1) => ContentDate(content1).CompareTo(ContentDate(content)));

            foreach (var item in items)
            {
                context.Builder.AddItem(context, item);
            }

        }

        private static DateTime ContentDate(IContent content) {
            var contentDate = DateTime.Now;
            if (content.Has<MemberPart>()) {
                var joinDate = content.As<MemberAdminPart>().JoinDate;
                if (joinDate != null) {
                    contentDate = (DateTime) joinDate;
                }
            }
            else if (content.Has<CommonPart>()) {
                var publishedUtc = content.As<CommonPart>().PublishedUtc;
                if (publishedUtc != null) {
                    contentDate = (DateTime) publishedUtc;
                }
            }
            return contentDate;
        }
    }
}