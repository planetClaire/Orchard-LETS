using System;
using System.Collections.Generic;
using Autofac;
using LETS.Events;
using LETS.Handlers;
using LETS.Models;
using LETS.Services;
using Moq;
using NogginBox.MailChimp.Handlers;
using NogginBox.MailChimp.Models;
using NogginBox.MailChimp.Services;
using NUnit.Framework;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.Records;
using Orchard.Core.Settings.Handlers;
using Orchard.Core.Settings.Metadata;
using Orchard.Core.Settings.Services;
using Orchard.Core.Title.Models;
using Orchard.Data;
using Orchard.Environment;
using Orchard.Security;
using Orchard.Settings;
using Orchard.Tests.Modules;
using Orchard.Tests.Stubs;
using Orchard.Tests.Utility;
using Orchard.Users.Handlers;
using Orchard.Users.Models;
using Orchard.Users.Services;

namespace LETS.Tests.Events
{
    [TestFixture]
    public class UserEventsTests : DatabaseEnabledTestsBase
    {
        private IMembershipService _membershipService;
        private UserEvents _userEvents;
        private Mock<IAuthenticationService> _authenticationServiceMock;
        private Mock<INoticeService> _noticeServiceMock;
        private Mock<IMailChimpService> _mailChimpServiceMock;
        private Mock<IOrchardServices> _orchardServicesMock;
        private Mock<WorkContext> _workContext;
        private Mock<IMemberService> _memberServiceMock;

        public override void Register(ContainerBuilder builder)
        {
            builder.RegisterAutoMocking(MockBehavior.Loose);
            builder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>));
            builder.RegisterType<ContentDefinitionManager>().As<IContentDefinitionManager>();
            builder.RegisterType<DefaultContentManager>().As<IContentManager>();
            builder.RegisterType<DefaultContentQuery>().As<IContentQuery>().InstancePerDependency();
            builder.RegisterType<OrchardServices>().As<IOrchardServices>();
            builder.RegisterType<Signals>().As<ISignals>();
            builder.RegisterType<StubCacheManager>().As<ICacheManager>();
            builder.RegisterType<StubWorkContextAccessor>().As<IWorkContextAccessor>();
            builder.RegisterType<InfosetHandler>().As<IContentHandler>();
            builder.RegisterType<SiteSettingsPartHandler>().As<IContentHandler>();

            builder.RegisterType<SiteService>().As<ISiteService>();
            _workContext = new Mock<WorkContext>();
            _workContext.Setup(w => w.GetState<ISite>(It.Is<string>(s => s == "CurrentSite"))).Returns(() => { return _container.Resolve<ISiteService>().GetSiteSettings(); });

            var _workContextAccessor = new Mock<IWorkContextAccessor>();
            _workContextAccessor.Setup(w => w.GetContext()).Returns(_workContext.Object);
            builder.RegisterInstance(_workContextAccessor.Object).As<IWorkContextAccessor>();

            builder.RegisterType<MembershipService>().As<IMembershipService>();
            builder.RegisterType<UserEvents>();
            _authenticationServiceMock = new Mock<IAuthenticationService>();
            builder.RegisterInstance(_authenticationServiceMock);
            builder.RegisterType<UserPartHandler>().As<IContentHandler>();
            builder.RegisterType<LETSSettingsPartHandler>().As<IContentHandler>();
            _noticeServiceMock = new Mock<INoticeService>();
            builder.RegisterInstance(_noticeServiceMock);
            _mailChimpServiceMock = new Mock<IMailChimpService>();
            builder.RegisterInstance(_mailChimpServiceMock);
            _orchardServicesMock= new Mock<IOrchardServices>();
            builder.RegisterInstance(_orchardServicesMock);
            builder.RegisterType<MailChimpSettingsHandler>().As<IContentHandler>();
            _memberServiceMock = new Mock<IMemberService>();
            builder.RegisterInstance(_memberServiceMock);
            builder.RegisterType<LETSHandler>().As<IContentHandler>();
        }

        public class LETSHandler : ContentHandler
        {
            public LETSHandler()
            {
                Filters.Add(new ActivatingFilter<MailChimpSettingsPart>("Site"));
            }
        }

        public override void Init()
        {
            base.Init();
            _membershipService = _container.Resolve<IMembershipService>();
            _userEvents = _container.Resolve<UserEvents>();
        }

        protected override IEnumerable<Type> DatabaseTypes
        {
            get
            {
                return new[] {
                    typeof (ContentItemRecord),
                    typeof (ContentItemVersionRecord),
                    typeof (ContentTypeRecord),
                    typeof (UserPartRecord),
                    typeof(LETSSettingsPartRecord),
                    typeof(SettingsRecord)
                };
            }
        }

        [Test]
        public void ConfirmedEmailRegistrationApproved_SignsInUser()
        {
            var user = _membershipService.CreateUser(new CreateUserParams("12345", "password", "me@me.com", "", "", true));
            _session.Flush();

            _userEvents.ConfirmedEmail(user);

            _authenticationServiceMock.Verify(a => a.SignIn(user, false));

            ClearSession();
        }

        [Test]
        public void ConfirmedEmailRegistrationNotApproved_DoesNotSignInUser()
        {
            var user = _membershipService.CreateUser(new CreateUserParams("12345", "password", "me@me.com", "", "", false));
            _session.Flush();

            _userEvents.ConfirmedEmail(user);

            _authenticationServiceMock.Verify(a => a.SignIn(user, false), Times.Never());

            ClearSession();
        }

        [Test]
        public void ConfirmedEmailRegistrationApproved_PublishesNotices()
        {
            var user = _membershipService.CreateUser(new CreateUserParams("12345", "password", "me@me.com", "", "", true));
            _session.Flush();

            _userEvents.ConfirmedEmail(user);

            _noticeServiceMock.Verify(n => n.PublishMemberNotices(user.Id));

            ClearSession();
        }

        [Test]
        public void ConfirmedEmailRegistrationNotApproved_DoesNotPublishNotices()
        {
            var user = _membershipService.CreateUser(new CreateUserParams("12345", "password", "me@me.com", "", "", false));
            _session.Flush();

            _userEvents.ConfirmedEmail(user);

            _noticeServiceMock.Verify(n => n.PublishMemberNotices(user.Id), Times.Never());

            ClearSession();
        }

        [Test]
        public void ConfirmedEmailRegistrationApproved_SubscribesToMailchimp()
        {
            var user = _membershipService.CreateUser(new CreateUserParams("12345", "password", "me@me.com", "", "", true));
            _session.Flush();
            var letsSettings = _container.Resolve<IWorkContextAccessor>().GetContext().CurrentSite.As<LETSSettingsPart>();
            letsSettings.IdMailChimpList = "idList";
            var mailchimpSettings = _container.Resolve<IWorkContextAccessor>().GetContext().CurrentSite.As<MailChimpSettingsPart>();
            mailchimpSettings.ApiKey = "apiKey";
            var mergeVars = new Dictionary<string, object>();
            _memberServiceMock.Setup(m => m.GetMergeVarsForMailChimp(user.As<MemberPart>(), "idList")).Returns(mergeVars);

            _userEvents.ConfirmedEmail(user);

            _mailChimpServiceMock.Verify(n => n.Subscribe("idList", user.Email, mergeVars, "html", false, true, true, false, null));

            ClearSession();
        }

        [Test]
        public void ConfirmedEmailRegistrationNotApproved_DoesNotSubscribeToMailchimp()
        {
            var user = _membershipService.CreateUser(new CreateUserParams("12345", "password", "me@me.com", "", "", false));
            _session.Flush();
            var letsSettings = _container.Resolve<IWorkContextAccessor>().GetContext().CurrentSite.As<LETSSettingsPart>();
            letsSettings.IdMailChimpList = "idList";
            var mailchimpSettings = _container.Resolve<IWorkContextAccessor>().GetContext().CurrentSite.As<MailChimpSettingsPart>();
            mailchimpSettings.ApiKey = "apiKey";
            var mergeVars = new Dictionary<string, object>();
            _memberServiceMock.Setup(m => m.GetMergeVarsForMailChimp(user.As<MemberPart>(), "idList")).Returns(mergeVars);

            _userEvents.ConfirmedEmail(user);

            _mailChimpServiceMock.Verify(n => n.Subscribe("idList", user.Email, mergeVars, "html", false, true, true, false, null), Times.Never());

            ClearSession();
        }

        [Test]
        public void ApprovedEmailStatusApproved_PublishesNotices() {
            var user = _membershipService.CreateUser(new CreateUserParams("12345", "password", "me@me.com", "", "", false));
            _session.Flush();
            user.As<UserPart>().EmailStatus = UserStatus.Approved;

            _userEvents.Approved(user);

            _noticeServiceMock.Verify(n => n.PublishMemberNotices(user.Id));

            ClearSession();
        }

        [Test]
        public void ApprovedEmailStatusNotApproved_DoesNotPublishNotices()
        {
            var user = _membershipService.CreateUser(new CreateUserParams("12345", "password", "me@me.com", "", "", false));
            _session.Flush();

            _userEvents.Approved(user);
            
            _noticeServiceMock.Verify(n => n.PublishMemberNotices(user.Id), Times.Never());

            ClearSession();
        }


        [Test]
        public void Moderated_DeletesMemberNotices() {
            var user = _membershipService.CreateUser(new CreateUserParams("12345", "password", "me@me.com", "", "", true));

            _userEvents.Moderated(user);

            _noticeServiceMock.Verify(n => n.DeleteMemberNotices(user.Id));

            ClearSession();
        }

        [Test]
        public void Moderated_UnsubscribesFromMailchimp() {
            var user = _membershipService.CreateUser(new CreateUserParams("12345", "password", "me@me.com", "", "", true));
            var letsSettings = _container.Resolve<IWorkContextAccessor>().GetContext().CurrentSite.As<LETSSettingsPart>();
            letsSettings.IdMailChimpList = "idList";
            var mailchimpSettings = _container.Resolve<IWorkContextAccessor>().GetContext().CurrentSite.As<MailChimpSettingsPart>();
            mailchimpSettings.ApiKey = "apiKey";
            var mergeVars = new Dictionary<string, object>();
            _memberServiceMock.Setup(m => m.GetMergeVarsForMailChimp(user.As<MemberPart>(), "idList")).Returns(mergeVars);

            _userEvents.Moderated(user);

            _mailChimpServiceMock.Verify(m => m.Unsubscribe("idList", user.Email, false, false, false));

            ClearSession();
        }

        [Test]
        public void Deleting_DeletesMemberNotices()
        {
            var user = _membershipService.CreateUser(new CreateUserParams("12345", "password", "me@me.com", "", "", true));

            _userEvents.Deleting(user);

            _noticeServiceMock.Verify(n => n.DeleteMemberNotices(user.Id));

            ClearSession();
        }

        [Test]
        public void Deleting_UnsubscribesFromMailchimp()
        {
            var user = _membershipService.CreateUser(new CreateUserParams("12345", "password", "me@me.com", "", "", true));
            var letsSettings = _container.Resolve<IWorkContextAccessor>().GetContext().CurrentSite.As<LETSSettingsPart>();
            letsSettings.IdMailChimpList = "idList";
            var mailchimpSettings = _container.Resolve<IWorkContextAccessor>().GetContext().CurrentSite.As<MailChimpSettingsPart>();
            mailchimpSettings.ApiKey = "apiKey";
            var mergeVars = new Dictionary<string, object>();
            _memberServiceMock.Setup(m => m.GetMergeVarsForMailChimp(user.As<MemberPart>(), "idList")).Returns(mergeVars);

            _userEvents.Deleting(user);

            _mailChimpServiceMock.Verify(m => m.Unsubscribe("idList", user.Email, false, false, false));

            ClearSession();
        }
    }
}
