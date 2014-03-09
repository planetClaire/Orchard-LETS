using System.Linq;
using System.Web.Routing;
using JetBrains.Annotations;
using LETS.Models;
using LETS.Services;
using LETS.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Feeds;

namespace LETS.Drivers
{
    [UsedImplicitly]
    public class NoticePartDriver : ContentPartDriver<NoticePart>
    {
        private readonly INoticeService _noticeService;
        private readonly IWorkContextAccessor _workContext;
        private readonly IFeedManager _feedManager;
        private readonly IContentManager _contentManager;

        protected override string Prefix { get { return "Notice"; } }

        public NoticePartDriver(INoticeService noticeService, IWorkContextAccessor workContext, IFeedManager feedManager, IContentManager contentManager)
        {
            _noticeService = noticeService;
            _workContext = workContext;
            _feedManager = feedManager;
            _contentManager = contentManager;
        }

        protected override DriverResult Display(NoticePart part, string displayType, dynamic shapeHelper)
        {
            if (displayType.Equals("Detail"))
            {
                _feedManager.Register(_noticeService.GetNoticeTypeTitle(part.NoticeType.Id), "rss",
                                      new RouteValueDictionary { { "idnoticetype", part.NoticeType.Id } });
            }
            return Combined(
                ContentShape("Parts_Notice",
                             () => shapeHelper.Parts_Notice(
                                 ContentPart: part,
                                 NoticeTypeTitle: _noticeService.GetNoticeTypeTitle(part.NoticeType.Id))),
                ContentShape("Parts_Notice_Summary",
                             () => shapeHelper.Parts_Notice_Summary(
                                 ContentPart: part,
                                 NoticeTypeTitle: _noticeService.GetNoticeTypeTitle(part.NoticeType.Id))),
                ContentShape("Parts_Notice_DetailedSummary",
                             () => shapeHelper.Parts_Notice_DetailedSummary(
                                 ContentPart: part,
                                 NoticeTypeTitle: _noticeService.GetNoticeTypeTitle(part.NoticeType.Id))),
                ContentShape("Parts_Notice_DetailedSummaryArchived",
                             () => shapeHelper.Parts_Notice_DetailedSummaryArchived(
                                 ContentPart: part,
                                 NoticeTypeTitle: _noticeService.GetNoticeTypeTitle(part.NoticeType.Id)))
                 );

        }

        protected override DriverResult Editor(NoticePart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_Notice_Edit",
                                () => shapeHelper.EditorTemplate(
                                    TemplateName: "Parts/Notice",
                                    Model: BuildEditorViewModel(part),
                                    Prefix: Prefix));
        }

        protected override DriverResult Editor(NoticePart part, IUpdateModel updater, dynamic shapeHelper)
        {
            var model = new EditNoticeViewModel();
            if (updater.TryUpdateModel(model, Prefix, null, null))
            {
                if (part.ContentItem.Id != 0)
                {
                    _noticeService.UpdateNoticeForContentItem(part.ContentItem, model);
                }
            }
            return Editor(part, shapeHelper);
        }

        private EditNoticeViewModel BuildEditorViewModel(NoticePart part)
        {
            var nvm = new EditNoticeViewModel {
                NoticeTypes = _noticeService.GetNoticeTypes(),
                Price = part.Price,
                CurrencyUnit = _workContext.GetContext().CurrentSite.As<LETSSettingsPart>().CurrencyUnit
            };
            if (part.NoticeType != null)
            {
                nvm.IdNoticeType = part.NoticeType.Id;
            }
            return nvm;
        }

        protected override void Importing(NoticePart part, Orchard.ContentManagement.Handlers.ImportContentContext context)
        {
            var price = context.Attribute(part.PartDefinition.Name, "Price");
            if (price != null)
            {
                part.Price = int.Parse(price);
            }
        }

        protected override void Imported(NoticePart part, Orchard.ContentManagement.Handlers.ImportContentContext context)
        {
            var noticeType = context.Attribute(part.PartDefinition.Name, "NoticeType");
            if (noticeType != null)
            {
                part.Record.NoticeTypePartRecord = context.GetItemFromSession(noticeType).As<NoticeTypePart>().Record;
            }
        }

        protected override void Exporting(NoticePart part, Orchard.ContentManagement.Handlers.ExportContentContext context)
        {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Price", part.Price);
            var noticeTypePart = _contentManager.Query<NoticeTypePart, NoticeTypePartRecord>("NoticeType").Where(x => x.Id == part.Record.NoticeTypePartRecord.Id).List().FirstOrDefault();
            if (noticeTypePart != null)
            {
                var noticeTypeIdentity = _contentManager.GetItemMetadata(noticeTypePart).Identity;
                context.Element(part.PartDefinition.Name).SetAttributeValue("NoticeType", noticeTypeIdentity.ToString());
            }
        }
    }
}