using System;
using System.Linq;
using System.Web.Mvc;
using System.Xml.Linq;
using LETS.Models;
using LETS.Services;
using Orchard;
using Orchard.ArchiveLater.Models;
using Orchard.ContentManagement;
using Orchard.Core.Common.Fields;
using Orchard.Core.Common.Models;
using Orchard.Core.Feeds;
using Orchard.Core.Feeds.Models;
using Orchard.Core.Feeds.StandardBuilders;
using Orchard.Core.Settings.Models;
using Orchard.Fields.Fields;
using Orchard.Localization;
using Orchard.Utility.Extensions;

namespace LETS.Feeds
{
    public class NoticeFeedItemBuilder : IFeedItemBuilder
    {
        private readonly INoticeService _noticeService;
        private readonly IContentManager _contentManager;
        private readonly IOrchardServices _orchardServices;
        public Localizer T { get; set; }

        public NoticeFeedItemBuilder(INoticeService noticeService, IContentManager contentManager, IOrchardServices orchardServices)
        {
            _noticeService = noticeService;
            _contentManager = contentManager;
            _orchardServices = orchardServices;
            T = NullLocalizer.Instance;
        }

        public void Populate(FeedContext context)
        {
            foreach(var feedItem in context.Response.Items.Where(i => i.Item is NoticePart)) 
            {
                var notice = (NoticePart)feedItem.Item;
                var contentItem = notice.ContentItem;

                var inspector = new ItemInspector(contentItem, _contentManager.GetItemMetadata(contentItem));

                var title = string.Format("{0} - {1}", _noticeService.GetNoticeTypeTitle(notice.NoticeType.Id), notice.Title);


                // add to known formats
                var description = string.Format("<strong>{0} {1}</strong>", T("From"), notice.Member.FirstLastName);
                var currencyUnit = _orchardServices.WorkContext.CurrentSite.As<LETSSettingsPart>().CurrencyUnit;
                if (notice.Price > 0) {
                    var per = ((dynamic) notice).Per;
                    if (per != null) {
                        var selectedValues = ((EnumerationField) per).SelectedValues;
                        if (selectedValues != null)
                        {
                            var valueFormat = T("{0}").ToString();
                            var translatedValues = selectedValues.Select(v => string.Format(valueFormat, T(v).Text)).ToArray();
                            var separator = T(", ").ToString();
                            per = string.Join(separator, translatedValues);
                        }
                    }
                    description += string.Format("<br /><em>{0}{1} {2}</em>", currencyUnit, notice.Price, per);
                }
                var paymentTerms = ((dynamic) notice).PaymentTerms;
                if (paymentTerms != null) {
                    var selectedValues = ((EnumerationField) paymentTerms).SelectedValues;
                    if (selectedValues != null)
                    {
                        var valueFormat = T("{0}").ToString();
                        var translatedValues = selectedValues.Select(v => string.Format(valueFormat, T(v).Text)).ToArray();
                        var separator = T(", ").ToString();
                        paymentTerms = string.Join(separator, translatedValues);
                    }
                    description += string.Format("<br /><em>{0}</em>", paymentTerms);
                }
                var noticeDescription = ((dynamic) notice).Description;
                if (noticeDescription != null && !string.IsNullOrEmpty(((TextField)noticeDescription).Value)) {
                    description += string.Format("<p>{0}</p>", Helpers.Helpers.Linkify(((TextField)noticeDescription).Value.ReplaceNewLinesWith("<br />")));
                }
                var photos = ((dynamic)notice).Photos;
                if (photos != null)
                {
                    var fileNames = ((DropzoneField.Fields.DropzoneField)photos).FileNames;
                    if (!string.IsNullOrEmpty(fileNames))
                    {
                        var urlHelper = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
                        var uriBuilder = new UriBuilder(urlHelper.RequestContext.HttpContext.Request.ToRootUrlString()) { Path = urlHelper.RouteUrl(inspector.Link) };
                        description += "<p>";
                        foreach (var fileName in fileNames.Split(';'))
                        {
                            description += string.Format("<a href='{2}'><img alt='Photo' src='{1}/{0}' /></a>", fileName, _orchardServices.WorkContext.CurrentSite.As<SiteSettingsPart>().BaseUrl, uriBuilder.Uri.OriginalString);
                        }
                        description += "</p>";
                    }
                }
                var expiryDate = notice.As<ArchiveLaterPart>();
                var currentTimeZone = _orchardServices.WorkContext.CurrentTimeZone;
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
                    var publishedUtc = notice.As<CommonPart>().PublishedUtc;
                    if (publishedUtc != null) {
                        feedItem.Element.SetElementValue("pubDate", publishedUtc.Value.ToString("r"));
                    }
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

                    context.Builder.AddProperty(context, feedItem, "published-date", Convert.ToString(notice.As<CommonPart>().PublishedUtc)); // format? cvt to generic T?
                }
            }
        }
    }
}
