using System;
using System.Web.Mvc;
using System.Xml.Linq;
using JetBrains.Annotations;
using LETS.Models;
using LETS.Services;
using Orchard.Core.Feeds;
using Orchard.Core.Feeds.Models;
using Orchard.Localization;
using Orchard.Utility.Extensions;

namespace LETS.Feeds
{
    [UsedImplicitly]
    public class MemberFeedQuery: IFeedQueryProvider, IFeedQuery
    {
        private readonly IMemberService _memberService;
        public Localizer T;

        public MemberFeedQuery(IMemberService memberService)
        {
            _memberService = memberService;
            T = NullLocalizer.Instance;
        }

        public FeedQueryMatch Match(FeedContext context)
        {
            if (context.ValueProvider.GetValue("members") != null)
            {
                return new FeedQueryMatch { Priority = -1, FeedQuery = this };
            }
            return null;
        }

        public void Execute(FeedContext context)
        {
            var limit = 20;
            var limitValue = context.ValueProvider.GetValue("limit");
            if (limitValue != null)
                limit = (int)limitValue.ConvertTo(typeof(int));

            var title = T("Latest members");
            var description = T("Latest LETS members");
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

            var members =
                _memberService.QueryMembers().OrderByDescending<MemberAdminPartRecord>(m => m.JoinDate).Slice
                    (0, limit);

            foreach (var member in members)
            {
                context.Builder.AddItem(context, member);
            }
        }
    }
}