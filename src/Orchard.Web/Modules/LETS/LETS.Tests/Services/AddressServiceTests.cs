using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using LETS.Handlers;
using LETS.Models;
using LETS.Services;
using Moq;
using NUnit.Framework;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.Records;
using Orchard.Core.Settings.Metadata;
using Orchard.Core.Title.Handlers;
using Orchard.Core.Title.Models;
using Orchard.Data;
using Orchard.Environment;
using Orchard.Tests.Modules;
using Orchard.Tests.Stubs;
using Orchard.Tests.Utility;

namespace LETS.Tests.Services
{
    public class AddressServiceTests : DatabaseEnabledTestsBase
    {
        private Mock<WorkContext> _workContextMock;
        private IContentManager _contentManager;

        private IAddressService _addressService;

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

            _workContextMock = new Mock<WorkContext>();
            var workContextAccessor = new Mock<IWorkContextAccessor>();
            workContextAccessor.Setup(w => w.GetContext()).Returns(_workContextMock.Object);
            builder.RegisterInstance(workContextAccessor.Object).As<IWorkContextAccessor>();

            builder.RegisterType<LocalityPartHandler>().As<IContentHandler>();
            builder.RegisterType<TitlePartHandler>().As<IContentHandler>();
            builder.RegisterType<AddressService>().As<IAddressService>();
            builder.RegisterType<LETSHandler>().As<IContentHandler>();
        }

        public class LETSHandler : ContentHandler {
            public LETSHandler() {
                Filters.Add(new ActivatingFilter<LocalityPart>("Locality"));
                Filters.Add(new ActivatingFilter<TitlePart>("Locality"));                
            }
        }

        public override void Init()
        {
            base.Init();
            _contentManager = _container.Resolve<IContentManager>();
            _addressService= _container.Resolve<IAddressService>();
        }

        protected override IEnumerable<Type> DatabaseTypes
        {
            get
            {
                return new[] {
                    typeof (ContentItemRecord),
                    typeof (ContentItemVersionRecord),
                    typeof (ContentTypeRecord),
                    typeof(LocalityPartRecord),
                    typeof(TitlePartRecord)
                };
            }
        }

        [Test]
        public void GetLocalityViewsReturnsThemInAlphaOrder() {
            _contentManager.Create<TitlePart>("Locality", l => l.Title = "Kalamunda");
            _contentManager.Create<TitlePart>("Locality", l => l.Title = "Armadale");
            _contentManager.Create<TitlePart>("Locality", l => l.Title = "Fremantle");

            var localityViews = _addressService.GetLocalityViews().ToList();

            Assert.AreEqual("Armadale", localityViews.ElementAt(0).Name);
            Assert.AreEqual("Fremantle", localityViews.ElementAt(1).Name);
            Assert.AreEqual("Kalamunda", localityViews.ElementAt(2).Name);

            ClearSession();
        }

    }
}
