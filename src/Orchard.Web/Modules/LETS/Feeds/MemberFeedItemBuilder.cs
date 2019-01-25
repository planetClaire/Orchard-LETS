using System;
using System.Linq;
using System.Web.Mvc;
using System.Xml.Linq;
using LETS.Models;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Feeds;
using Orchard.Core.Feeds.Models;
using Orchard.Core.Feeds.StandardBuilders;
using Orchard.Localization;
using Orchard.Users.Models;
using Orchard.Utility.Extensions;

namespace LETS.Feeds
{
    public class MemberFeedItemBuilder: IFeedItemBuilder
    {
        private readonly IContentManager _contentManager;
        public Localizer T { get; set; }

        public MemberFeedItemBuilder(IContentManager contentManager)
        {
            _contentManager = contentManager;
            T = NullLocalizer.Instance;
        }

        public void Populate(FeedContext context) {
            foreach(var feedItem in context.Response.Items.Where(i => i.Item is UserPart)) {
                var contentItem = ((UserPart) feedItem.Item).ContentItem;
                var inspector = new ItemInspector(contentItem, _contentManager.GetItemMetadata(contentItem));
                var title = string.Format("{0}: {1}", T("New member"), contentItem.As<MemberPart>().FirstLastName);

                // add to known formats
                var description = string.Format("<p>{0} {1} {2}<br /><em>{3}</em></p>", T("Please take the time to welcome"), contentItem.As<MemberPart>().FirstName, T("to our system."), T("Click the link for more details and contact details"));
                if (context.Format == "rss")
                {
                    var link = new XElement("link");
                    context.Response.Contextualize(requestContext =>
                    {
                        var urlHelper = new UrlHelper(requestContext);
                        var uriBuilder = new UriBuilder(urlHelper.RequestContext.HttpContext.Request.ToRootUrlString()) { Path = urlHelper.RouteUrl(inspector.Link) };
                        link.Add(uriBuilder.Uri.OriginalString);
                    });

                    feedItem.Element.SetElementValue("title", title);
                    feedItem.Element.Add(link);
                    feedItem.Element.Add(new XElement("description", new XCData(description)));
                    feedItem.Element.SetElementValue("pubDate", contentItem.As<MemberAdminPart>().JoinDate.Value.ToString("r"));
                }
                else
                {
                    var feedItem1 = feedItem;
                    context.Response.Contextualize(requestContext =>
                    {
                        var urlHelper = new UrlHelper(requestContext);
                        context.Builder.AddProperty(context, feedItem1, "link", urlHelper.RouteUrl(inspector.Link));
                    });
                    context.Builder.AddProperty(context, feedItem, "title", title);
                    context.Builder.AddProperty(context, feedItem, "description", description);

                    context.Builder.AddProperty(context, feedItem, "published-date", Convert.ToString(contentItem.As<CommonPart>().PublishedUtc)); // format? cvt to generic T?
                }
            }
        }
    }
}
