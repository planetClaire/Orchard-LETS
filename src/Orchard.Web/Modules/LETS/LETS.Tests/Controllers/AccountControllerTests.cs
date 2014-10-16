using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using LETS.Controllers;
using LETS.Handlers;
using LETS.Models;
using LETS.Services;
using LETS.ViewModels;
using Moq;
using NUnit.Framework;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.Records;
using Orchard.Core.Common.Handlers;
using Orchard.Core.Common.Models;
using Orchard.Core.Settings.Handlers;
using Orchard.Core.Settings.Metadata;
using Orchard.Core.Settings.Services;
using Orchard.Core.Title.Handlers;
using Orchard.Core.Title.Models;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Implementation;
using Orchard.Environment;
using Orchard.Mvc;
using Orchard.Security;
using Orchard.Settings;
using Orchard.Taxonomies.Services;
using Orchard.Tests.Modules;
using Orchard.Tests.Modules.Indexing;
using Orchard.Tests.Stubs;
using Orchard.Tests.Utility;
using Orchard.Users.Handlers;
using Orchard.Users.Models;
using Orchard.Users.Services;

namespace LETS.Tests.Controllers
{
    [TestFixture]
    public class AccountControllerTests : DatabaseEnabledTestsBase
    {
        private IContentManager _contentManager;

        private AccountController _controller;
        private IMembershipService _membershipService;
        private Mock<WorkContext> _workContext;
        private Mock<IUserService> _userServiceMock;
        private IndexingTaskExecutorTests.StubLogger _logger;
        private Mock<INoticeService> _noticeServiceMock;

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

            builder.RegisterType<AccountController>().SingleInstance();
            builder.RegisterType<UserPartHandler>().As<IContentHandler>();
            builder.RegisterType<MembershipService>().As<IMembershipService>();
            builder.RegisterType<CommonPartHandler>().As<IContentHandler>();

            builder.RegisterType<MemberAdminPartHandler>().As<IContentHandler>();
            builder.RegisterType<DefaultShapeTableManager>().As<IShapeTableManager>();
            builder.RegisterType<DefaultShapeFactory>().As<IShapeFactory>();
            builder.RegisterType<ShapeTableLocator>().As<IShapeTableLocator>();
            builder.RegisterType<SiteSettingsPartHandler>().As<IContentHandler>();
            builder.RegisterType<RegistrationSettingsPartHandler>().As<IContentHandler>();

            builder.RegisterType<SiteService>().As<ISiteService>();
            _workContext = new Mock<WorkContext>();
            _workContext.Setup(w => w.GetState<ISite>(It.Is<string>(s => s == "CurrentSite"))).Returns(() => { return _container.Resolve<ISiteService>().GetSiteSettings(); });

            var _workContextAccessor = new Mock<IWorkContextAccessor>();
            _workContextAccessor.Setup(w => w.GetContext()).Returns(_workContext.Object);
            builder.RegisterInstance(_workContextAccessor.Object).As<IWorkContextAccessor>();

            _userServiceMock = new Mock<IUserService>();
            builder.RegisterInstance(_userServiceMock);

            _noticeServiceMock = new Mock<INoticeService>();
            builder.RegisterInstance(_noticeServiceMock);
            builder.RegisterType<NoticeTypePartHandler>().As<IContentHandler>();
            builder.RegisterType<NoticePartHandler>().As<IContentHandler>();
            builder.RegisterType<TitlePartHandler>().As<IContentHandler>();
        }

        public override void Init()
        {
            base.Init();
            _contentManager = _container.Resolve<IContentManager>();
            _controller = _container.Resolve<AccountController>();
            _membershipService = _container.Resolve<IMembershipService>();
            _controller.Logger = _logger = new IndexingTaskExecutorTests.StubLogger();
        }

        protected override IEnumerable<Type> DatabaseTypes
        {
            get
            {
                return new[] {
                    typeof (ContentItemRecord),
                    typeof (ContentItemVersionRecord),
                    typeof (ContentTypeRecord),
                    typeof(CommonPartRecord),

                    typeof (UserPartRecord),
                    typeof(MemberAdminPartRecord),
                    typeof(NoticeTypePartRecord),
                    typeof(NoticePartRecord),
                    typeof(TitlePartRecord)
                };
            }
        }

        [Test]
        public void RegisterMember_AsksForMinimumOfRequiredNoticeTypes() {
            var registrationSettings = _container.Resolve<IWorkContextAccessor>().GetContext().CurrentSite.As<RegistrationSettingsPart>();
            registrationSettings.UsersCanRegister = true;
            _noticeServiceMock.Setup(n => n.GetRequiredNoticeTypes()).Returns(new List<NoticeTypePart> {_contentManager.Create<NoticeTypePart>("NoticeType", n => n.RequiredCount = 2)});
            var shapeResult = (ShapeResult)_controller.RegisterMember();

            Assert.IsInstanceOf(typeof(RegisterNoticeTypesViewModel), ((dynamic)shapeResult.Model).RequiredNoticeTypes);
            Assert.AreEqual(2, ((RegisterNoticeTypesViewModel) ((dynamic) shapeResult.Model).RequiredNoticeTypes).NoticeTypes.FirstOrDefault().RequiredCount);

            ClearSession();
        }


        [Test]
        public void RegisterMember_AddsJoinDate() {
            var email = "me@email.com";
            var registerMemberViewModel = new RegisterMemberViewModel {
                Email = email,
                Password = "Password1",
                ConfirmPassword = "Password1"
            };
            var registerNoticeTypesViewModel = new RegisterNoticeTypesViewModel {
                NoticeTypes = new List<RegisterNoticesViewModel> { new RegisterNoticesViewModel { Notices = new List<RegisterNoticeViewModel> { new RegisterNoticeViewModel { Title = "my notice", IdCategoryTerm = 1 } } } }
            };
            var registrationSettings = _container.Resolve<IWorkContextAccessor>().GetContext().CurrentSite.As<RegistrationSettingsPart>();
            registrationSettings.UsersCanRegister = true;
            _userServiceMock.Setup(m => m.VerifyUserUnicity(email, email)).Returns(true);
            _controller.RegisterMember(registerMemberViewModel, registerNoticeTypesViewModel);

            var member = _membershipService.GetUser(email);

            Assert.NotNull(member);
            Assert.AreEqual(DateTime.UtcNow.DayOfYear, member.As<MemberAdminPart>().JoinDate.Value.DayOfYear);

            ClearSession();
        }

        [Test]
        public void RegisterMember_ValidationFails_UserNotSaved() {
            var email = "me@email.com";
            var registerMemberViewModel = new RegisterMemberViewModel
            {
                Email = email,
                Password = "Password1",
                ConfirmPassword = "Password1"
            };
            var registerNoticeTypesViewModel = new RegisterNoticeTypesViewModel
            {
                // missing IdCategory (this doesn't trigger !ModelState.IsValid, why not?
                NoticeTypes = new List<RegisterNoticesViewModel> { new RegisterNoticesViewModel { Notices = new List<RegisterNoticeViewModel> { new RegisterNoticeViewModel { Title = "my notice" } } } }
            };
            _controller.ModelState.AddModelError("key", "value");
            _controller.RegisterMember(registerMemberViewModel, registerNoticeTypesViewModel);

            var member = _membershipService.GetUser(email);

            Assert.IsNull(member);

            ClearSession();
        }

        [Test]
        public void RegisterMember_SavesNoticeDrafts() {
            var email = "me@email.com";
            var registerMemberViewModel = new RegisterMemberViewModel
            {
                Email = email,
                Password = "Password1",
                ConfirmPassword = "Password1"
            };
            var noticeType = _contentManager.Create<NoticeTypePart>("NoticeType");
            var idNoticeType = noticeType.Id;
            var registerNoticeTypesViewModel = new RegisterNoticeTypesViewModel
            {
                NoticeTypes = new List<RegisterNoticesViewModel> { new RegisterNoticesViewModel { IdNoticeType = idNoticeType, NoticeTypeName = "Offer", Notices = new List<RegisterNoticeViewModel> { new RegisterNoticeViewModel { Title = "my notice", IdCategoryTerm = 1 } } } }
            };
            var registrationSettings = _container.Resolve<IWorkContextAccessor>().GetContext().CurrentSite.As<RegistrationSettingsPart>();
            registrationSettings.UsersCanRegister = true;
            _userServiceMock.Setup(m => m.VerifyUserUnicity(email, email)).Returns(true);
            _noticeServiceMock.Setup(n => n.GetNoticeType(idNoticeType)).Returns(noticeType.Record);
            _controller.RegisterMember(registerMemberViewModel, registerNoticeTypesViewModel);

            var member = _membershipService.GetUser(email);

            var notices = _contentManager.Query<NoticePart>(VersionOptions.Draft, "Notice").List();

            Assert.AreEqual(1, notices.Count());
            var notice = notices.FirstOrDefault();
            Assert.AreEqual("my notice", notice.Title);
            Assert.AreEqual(member.Id, notice.As<CommonPart>().Owner.Id);
            Assert.AreEqual(idNoticeType, notice.NoticeType.Id);

            ClearSession();
        }

        [Test]
        public void RegisterMember_SaveNoticeDraftsThrowsException_LogsException() {
            var email = "me@email.com";
            var registerMemberViewModel = new RegisterMemberViewModel
            {
                Email = email,
                Password = "Password1",
                ConfirmPassword = "Password1"
            };
            var idNoticeType = 6;
            var registerNoticeTypesViewModel = new RegisterNoticeTypesViewModel
            {
                NoticeTypes = new List<RegisterNoticesViewModel> { new RegisterNoticesViewModel {IdNoticeType = idNoticeType, Notices = new List<RegisterNoticeViewModel> { new RegisterNoticeViewModel { Title = "my notice", IdCategoryTerm = 1 } } } }
            };
            var registrationSettings = _container.Resolve<IWorkContextAccessor>().GetContext().CurrentSite.As<RegistrationSettingsPart>();
            registrationSettings.UsersCanRegister = true;
            _userServiceMock.Setup(m => m.VerifyUserUnicity(email, email)).Returns(true);
            _noticeServiceMock.Setup(n => n.GetNoticeType(idNoticeType)).Throws(new ApplicationException());
            _controller.RegisterMember(registerMemberViewModel, registerNoticeTypesViewModel);

            Assert.That(_logger.LogEntries.Count, Is.EqualTo(1));
            Assert.That(_logger.LogEntries, Has.Some.Matches<IndexingTaskExecutorTests.LogEntry>(entry => entry.LogFormat.StartsWith("Something went wrong drafting new member notices for user ")));

            ClearSession();
        }

        [Test]
        public void RegisterMember_SaveNoticeDraftsThrowsException_StillSavesMember() {
            var email = "me@email.com";
            var registerMemberViewModel = new RegisterMemberViewModel
            {
                Email = email,
                Password = "Password1",
                ConfirmPassword = "Password1"
            };
            var registerNoticeTypesViewModel = new RegisterNoticeTypesViewModel
            {
                NoticeTypes = new List<RegisterNoticesViewModel> { new RegisterNoticesViewModel { Notices = new List<RegisterNoticeViewModel> { new RegisterNoticeViewModel { Title = "my notice", IdCategoryTerm = 1 } } } }
            };
            var registrationSettings = _container.Resolve<IWorkContextAccessor>().GetContext().CurrentSite.As<RegistrationSettingsPart>();
            registrationSettings.UsersCanRegister = true;
            _userServiceMock.Setup(m => m.VerifyUserUnicity(email, email)).Returns(true);
            _noticeServiceMock.Setup(n => n.GetNoticeType(1)).Throws(new ApplicationException());
            _controller.RegisterMember(registerMemberViewModel, registerNoticeTypesViewModel);

            var member = _membershipService.GetUser(email);

            Assert.NotNull(member);

            ClearSession();
        }
    }
}
