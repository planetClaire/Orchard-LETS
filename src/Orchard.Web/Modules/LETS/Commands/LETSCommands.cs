using System;
using LETS.Models;
using Orchard.Commands;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Title.Models;
using Orchard.Security;
using Orchard.Settings;

namespace LETS.Commands
{
    public class LETSCommands : DefaultOrchardCommandHandler
    {
        private readonly IContentManager _contentManager;
        private readonly IMembershipService _membershipService;
        private readonly ISiteService _siteService;
        private readonly IAuthenticationService _authenticationService;

        public LETSCommands(IContentManager contentManager, IMembershipService membershipService, ISiteService siteService, IAuthenticationService authenticationService)
        {
            _contentManager = contentManager;
            _membershipService = membershipService;
            _siteService = siteService;
            _authenticationService = authenticationService;
        }

        [OrchardSwitch]
        public string Title { get; set; }

        [OrchardSwitch]
        public int SortOrder { get; set; }

        [OrchardSwitch]
        public int RequiredCount { get; set; }

        [CommandName("noticeType create")]
        [CommandHelp("noticeType create /Title:<title> /SortOrder:<sortOrder> /RequiredCount:<requiredCount> Creates a new Notice Type")]
        [OrchardSwitches("Title,SortOrder,RequiredCount")]
        public void Create()
        {
            var owner = _membershipService.GetUser(_siteService.GetSiteSettings().SuperUser);
            _authenticationService.SetAuthenticatedUserForRequest(owner);

            var noticeType = _contentManager.Create("NoticeType", VersionOptions.Draft);
            if (string.IsNullOrEmpty(Title))
            {
                Context.Output.WriteLine(T("Title is required"));
                return;
            }
            noticeType.As<TitlePart>().Title = Title;
            noticeType.As<ICommonPart>().Owner = owner;
            noticeType.As<NoticeTypePart>().SortOrder = SortOrder;
            noticeType.As<NoticeTypePart>().RequiredCount = RequiredCount;

            _contentManager.Publish(noticeType);

            Context.Output.WriteLine(T("Notice Type created successfully.").Text);
        }

        [CommandName("setupLETSAdmin")]
        public void SetupLETSAdmin()
        {
            var superUser = _membershipService.GetUser(_siteService.GetSiteSettings().SuperUser);
            superUser.As<MemberPart>().FirstName = "LETS";
            superUser.As<MemberPart>().LastName= "Admin";
            superUser.As<MemberAdminPart>().JoinDate = DateTime.Now;
            superUser.As<MemberAdminPart>().MemberType = MemberType.Admin;
            _siteService.GetSiteSettings().As<LETSSettingsPart>().IdDemurrageRecipient = superUser.Id;
            Context.Output.Write(T("Done"));
        }
    }
}