using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
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
    public class LocalityControllerTests : DatabaseEnabledTestsBase
    {
        private IContentManager _contentManager;

        private LocalityController _controller;
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
            builder.RegisterType<LocalityController>().SingleInstance();
            _orchardServicesMock = new Mock<IOrchardServices>();
            builder.RegisterInstance(_orchardServicesMock);
        }

        public override void Init()
        {
            base.Init();
            _controller = _container.Resolve<LocalityController>();
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
        public void Notices_MustBeAuthorizedToAccessMemberContent() {
            _orchardServicesMock.Setup(o => o.Authorizer.Authorize(Permissions.AccessMemberContent)).Returns(false);

            var result = _controller.Notices(1);

            Assert.IsInstanceOf<HttpUnauthorizedResult>(result);

            ClearSession();
        }

    }
}
