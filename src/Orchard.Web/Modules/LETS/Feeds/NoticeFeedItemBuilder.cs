using System;
using System.Linq;
using System.Web.Mvc;
using System.Xml.Linq;
using JetBrains.Annotations;
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
//using So.ImageResizer.Helpers;

namespace LETS.Feeds
{
    [UsedImplicitly]
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
                var published = notice.As<CommonPart>().PublishedUtc.Value;
                var created = notice.As<CommonPart>().CreatedUtc.Value;
                if ((published - created).Days > 1) {
                    description += string.Format("<br />This notice has been re-posted. It was first posted on {0}", notice.As<CommonPart>().CreatedUtc.Value.ToLongDateString());
                }
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
                var photos = ((dynamic) notice).Photos;
                if (photos != null) {
                    var fileNames = ((AgileUploaderField.Fields.AgileUploaderField) photos).FileNames;
                    if (!string.IsNullOrEmpty(fileNames)) {
                        var urlHelper = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
                        var uriBuilder = new UriBuilder(urlHelper.RequestContext.HttpContext.Request.ToRootUrlString()) { Path = urlHelper.RouteUrl(inspector.Link) };
                        description += "<p>";
                        //foreach (var fileName in fileNames.Split(';')) {
                        //    description += string.Format("<a href='{5}'><img alt='Photo' src='{4}/resizedImage?url={0}&width=100&height=80&maxWidth=100&maxheight=80&cropMode={1}&scale={2}&stretchMode={3}' /></a>", fileName, ResizeSettingType.CropMode.Auto, ResizeSettingType.ScaleMode.DownscaleOnly, ResizeSettingType.StretchMode.Proportionally, _orchardServices.WorkContext.CurrentSite.As<SiteSettings2Part>().BaseUrl, uriBuilder.Uri.OriginalString);
                        //}
                        description += "</p>";
                    }
                }
                var expiryDate = notice.As<ArchiveLaterPart>();
                var currentTimeZone = _orchardServices.WorkContext.CurrentTimeZone;
                if (expiryDate.ScheduledArchiveUtc.Value != null) {
                    var dayLabel = "days";
                    
                    var days = Math.Ceiling(((DateTime)expiryDate.ScheduledArchiveUtc.Value - DateTime.UtcNow).TotalDays); 
                    if (days.Equals(1)) {
                        dayLabel = "day";
                    }
                    description += string.Format("<em>{0} {1} {2}</em>", T("This notice expires in"), days , T(dayLabel));
                }
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
