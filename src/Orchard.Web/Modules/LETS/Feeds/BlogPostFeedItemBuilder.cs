using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml.Linq;
using Orchard;
using Orchard.Blogs.Models;
using Orchard.ContentManagement;
using Orchard.Core.Feeds;
using Orchard.Core.Feeds.Models;
using Orchard.Core.Feeds.StandardBuilders;
using Orchard.Fields.Fields;
//using Orchard.MediaPicker.Fields;
using Orchard.Services;
using Orchard.Utility.Extensions;

namespace LETS.Feeds
{
    public class BlogPostFeedItemBuilder : IFeedItemBuilder
    {
        private readonly RouteCollection _routes;
        private readonly IContentManager _contentManager;
        private readonly IEnumerable<IHtmlFilter> _htmlFilters;
        private readonly IOrchardServices _orchardServices;

        public BlogPostFeedItemBuilder(RouteCollection routes, IContentManager contentManager, IEnumerable<IHtmlFilter> htmlFilters, IOrchardServices orchardServices)
        {
            _routes = routes;
            _contentManager = contentManager;
            _htmlFilters = htmlFilters;
            _orchardServices = orchardServices;
        }

        public void Populate(FeedContext context)
        {
            foreach (var feedItem in context.Response.Items.OfType<FeedItem<ContentItem>>())
            {
                if (feedItem.Item.Has<BlogPostPart>())
                {
                    var inspector = new ItemInspector(feedItem.Item, _contentManager.GetItemMetadata(feedItem.Item),
                                                      _htmlFilters);
                    var description = inspector.Description;
                    var mainImage = ((dynamic)feedItem.Item).BlogPost.MainPicture;
                    //var mediaPickerField = mainImage as MediaPickerField;
                    //if (mediaPickerField != null && !string.IsNullOrEmpty(mediaPickerField.Url))
                    //{
                    //    description = string.Format("<p><img src='{0}{1}?width=560' alt='{2}' /></p>{3}", _orchardServices.WorkContext.CurrentSite.BaseUrl, mediaPickerField.Url.Substring(1), mediaPickerField.AlternateText, description);
                    //}
                    // add to known formats
                    if (context.Format == "rss")
                    {
                        var link = new XElement("link");

                        context.Response.Contextualize(requestContext =>
                        {
                            var urlHelper = new UrlHelper(requestContext, _routes);
                            var uriBuilder =
                                new UriBuilder(urlHelper.RequestContext.HttpContext.Request.ToRootUrlString())
                                {
                                    Path = urlHelper.RouteUrl(inspector.Link)
                                };
                            link.Add(uriBuilder.Uri.OriginalString);
                        });

                        feedItem.Element.SetElementValue("title", inspector.Title);
                        feedItem.Element.Add(link);

                        feedItem.Element.SetElementValue("description", description);

                        if (true)
                        {
                            // RFC833 
                            // The "R" or "r" standard format specifier represents a custom date and time format string that is defined by 
                            // the DateTimeFormatInfo.RFC1123Pattern property. The pattern reflects a defined standard, and the property  
                            // is read-only. Therefore, it is always the same, regardless of the culture used or the format provider supplied.  
                            // The custom format string is "ddd, dd MMM yyyy HH':'mm':'ss 'GMT'". When this standard format specifier is used,  
                            // the formatting or parsing operation always uses the invariant culture. 
                            feedItem.Element.SetElementValue("pubDate", inspector.PublishedUtc.Value.ToString("r"));
                        }

                    }
                    else
                    {
                        var feedItem1 = feedItem;
                        context.Response.Contextualize(requestContext =>
                        {
                            var urlHelper = new UrlHelper(requestContext, _routes);
                            context.Builder.AddProperty(context, feedItem1, "link", urlHelper.RouteUrl(inspector.Link));
                        });
                        context.Builder.AddProperty(context, feedItem, "title", inspector.Title);
                        context.Builder.AddProperty(context, feedItem, "description", description);

                        if (inspector.PublishedUtc != null)
                            context.Builder.AddProperty(context, feedItem, "published-date", Convert.ToString(inspector.PublishedUtc)); // format? cvt to generic T?
                    }
                }
            }
        }
    }
}