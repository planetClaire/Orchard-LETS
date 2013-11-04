using System.Web.Routing;
using JetBrains.Annotations;
using LETS.Models;
using LETS.Services;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Feeds;
using Orchard.Localization;
using Orchard.Settings;

namespace LETS.Drivers
{
    [UsedImplicitly]
    public class MemberPartDriver : ContentPartDriver<MemberPart>
    {
        private readonly IMemberService _memberService;
        private readonly IFeedManager _feedManager;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly ISiteService _siteService;
        private readonly ISignals _signals;
        private readonly INoticeService _noticeService;
        public Localizer T { get; set; }

        public MemberPartDriver(IMemberService memberService, IFeedManager feedManager, IWorkContextAccessor workContextAccessor, ISiteService siteService, ISignals signals, INoticeService noticeService)
        {
            _memberService = memberService;
            _feedManager = feedManager;
            _workContextAccessor = workContextAccessor;
            _siteService = siteService;
            _signals = signals;
            _noticeService = noticeService;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(MemberPart part, string displayType, dynamic shapeHelper) {
            if (displayType.Equals("Detail")) {
                _feedManager.Register(T("Members").ToString(), "rss", new RouteValueDictionary {{"members", "all"}});

                var letsSettings = _siteService.GetSiteSettings().As<LETSSettingsPart>();
                var profileNotices =
                    shapeHelper.Profile_Notices(Member: part,
                                                Notices:
                                                    _noticeService.GetMemberNoticeShapes(part.User.Id, "Summary"));
                _workContextAccessor.GetContext()
                    .Layout.Zones[letsSettings.MemberNoticesZone]
                    .Add(profileNotices, letsSettings.MemberNoticesPosition);
                return ContentShape("Parts_Member",
                                    () =>
                                    shapeHelper.Parts_Member(Member: _memberService.GetMemberViewModel(part)));

            }
            return ContentShape("Parts_Member_Summary",
                                () =>
                                shapeHelper.Parts_Member_Summary(MemberPart: part));
        }

        protected override DriverResult Editor(MemberPart part, dynamic shapeHelper)
        {
            var result = ContentShape("Parts_Member_Edit", 
                                        () => shapeHelper.EditorTemplate(
                                            TemplateName: "Parts/Member", 
                                            Model: part, 
                                            Prefix: Prefix));
            return result;
        }

        protected override DriverResult Editor(MemberPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);
            _signals.Trigger(string.Format("letsMemberPartsType{0}Changed", part.As<MemberAdminPart>().MemberType));
            _signals.Trigger("letsMemberListChanged");
            return Editor(part, shapeHelper);
        }

        protected override void Importing(MemberPart part, Orchard.ContentManagement.Handlers.ImportContentContext context)
        {
            part.FirstName = context.Attribute(part.PartDefinition.Name, "FirstName");
            part.LastName = context.Attribute(part.PartDefinition.Name, "LastName");
            part.Telephone = context.Attribute(part.PartDefinition.Name, "Telephone");
        }

        protected override void Exporting(MemberPart part, Orchard.ContentManagement.Handlers.ExportContentContext context)
        {
            context.Element(part.PartDefinition.Name).SetAttributeValue("FirstName", part.FirstName);
            context.Element(part.PartDefinition.Name).SetAttributeValue("LastName", part.LastName);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Telephone", part.Telephone);
        }
    }
}