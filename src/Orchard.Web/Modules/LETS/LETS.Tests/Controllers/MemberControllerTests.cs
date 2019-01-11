using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Autofac;
using LETS.Controllers;
using Moq;
using NUnit.Framework;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.Records;
using Orchard.Core.Common.Models;
using Orchard.Core.Settings.Metadata;
using Orchard.Data;
using Orchard.Environment;
using Orchard.Tests.Modules;
using Orchard.Tests.Stubs;
using Orchard.Tests.Utility;

namespace LETS.Tests.Controllers
{
    [TestFixture]
    public class MemberControllerTests : DatabaseEnabledTestsBase
    {
        private MemberController _controller;
        private Mock<IOrchardServices> _orchardServicesMock;

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

            builder.RegisterType<OrchardServices>().As<IOrchardServices>();
            builder.RegisterType<MemberController>().SingleInstance();
            _orchardServicesMock = new Mock<IOrchardServices>();
            builder.RegisterInstance(_orchardServicesMock);
        }

        public override void Init()
        {
            base.Init();
            _controller = _container.Resolve<MemberController>();
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
                };
            }
        }

        [Test]
        public void Index_MustBeAuthorizedToAccessMemberContent() {
            _orchardServicesMock.Setup(o => o.Authorizer.Authorize(Permissions.AccessMemberContent)).Returns(false);

            var result = _controller.Index();

            Assert.IsInstanceOf<HttpUnauthorizedResult>(result);

            ClearSession();
        }

    }
}
