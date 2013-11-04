using System;
using System.Linq;
using System.Web.Mvc;
using System.Xml.Linq;
using LETS.Models;
using Orchard.Comments.Models;
using Orchard.ContentManagement;
using Orchard.Core.Feeds;
using Orchard.Core.Feeds.Models;
using Orchard.Core.Feeds.StandardBuilders;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Utility.Extensions;

namespace LETS.Feeds {
    public class CommentFeedItemBuilder : IFeedItemBuilder {
        private readonly IContentManager _contentManager;
        private readonly IMembershipService _membershipService;
        public Localizer T { get; set; }

        public CommentFeedItemBuilder(IContentManager contentManager, IMembershipService membershipService)
        {
            _contentManager = contentManager;
            _membershipService = membershipService;
            T = NullLocalizer.Instance;
        }

        public void Populate(FeedContext context) {
            foreach (var feedItem in context.Response.Items.Where(i => i.Item is CommentPart))
            {
                var comment = (CommentPart)feedItem.Item;
                var commentedOn = _contentManager.Get(comment.Record.CommentedOn);
                var commentedOnInspector = new ItemInspector(
                    commentedOn,
                    _contentManager.GetItemMetadata(commentedOn)
                    );

                var author = comment.Record.Author;
                var user = _membershipService.GetUser(comment.Record.UserName);
                if (user != null && user.ContentItem.Has<MemberPart>()) {
                    author = user.As<MemberPart>().FirstLastName;
                }
                var title = string.Format("{0} {1} {2}", author, T("commented on"), commentedOnInspector.Title);


                // add to known formats
                if (context.Format == "rss") {
                    var link = new XElement("link");
                    var guid = new XElement("guid", new XAttribute("isPermaLink", "false"));
                    context.Response.Contextualize(requestContext => {
                                                       var urlHelper = new UrlHelper(requestContext);
                                                       var uriBuilder = new UriBuilder(urlHelper.RequestContext.HttpContext.Request.ToRootUrlString()) {Path = urlHelper.RouteUrl(commentedOnInspector.Link)};
                                                       link.Add(string.Format("{0}#comments", uriBuilder.Uri.OriginalString));
                                                       guid.Add(uriBuilder.Uri.OriginalString);
                                                   });

                    feedItem.Element.SetElementValue("title", title);
                    feedItem.Element.Add(link);
                    var commentText = Helpers.Helpers.Linkify(comment.Record.CommentText.ReplaceNewLinesWith("<br />"));
                    feedItem.Element.SetElementValue("description", commentText);
                    feedItem.Element.SetElementValue("pubDate", comment.Record.CommentDateUtc); 
                    feedItem.Element.Add(guid);
                }
                else {
                    var feedItem1 = feedItem;
                    context.Response.Contextualize(requestContext => {
                                                       var urlHelper = new UrlHelper(requestContext);
                                                       context.Builder.AddProperty(context, feedItem1, "link", urlHelper.RouteUrl(commentedOnInspector.Link));
                                                   });
                    context.Builder.AddProperty(context, feedItem, "title", title);
                    context.Builder.AddProperty(context, feedItem, "description", comment.Record.CommentText);

                    context.Builder.AddProperty(context, feedItem, "published-date", Convert.ToString(comment.Record.CommentDateUtc)); // format? cvt to generic T?
                }
            }
        }
    }
}