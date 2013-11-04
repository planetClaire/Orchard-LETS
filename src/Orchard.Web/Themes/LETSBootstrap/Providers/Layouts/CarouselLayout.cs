using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Projections.Descriptors.Layout;
using Orchard.Projections.Models;
using Orchard.Projections.Services;

namespace LETSBootstrap.Providers.Layouts {
    public class CarouselLayout : ILayoutProvider {
        private readonly IContentManager _contentManager;
        protected dynamic Shape { get; set; }

        public CarouselLayout(IShapeFactory shapeFactory, IContentManager contentManager)
        {
            _contentManager = contentManager;
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeLayoutContext describe) {
            describe.For("Html", T("Html"),T("Html Layouts"))
                .Element("Carousel", T("Carousel"), T("Organizes content items in a Carousel."),
                    DisplayLayout,
                    RenderLayout,
                    "CarouselLayout"
                );
        }

        public LocalizedString DisplayLayout(LayoutContext context) {
            return T("Carousel layout");
        }

        public dynamic RenderLayout(LayoutContext context, IEnumerable<LayoutComponentResult> layoutComponentResults) {
            string outerDivClass = context.State.OuterDivClass;
            string outerDivId = context.State.OuterDivId;
            string innerDivClass = context.State.InnerDivClass;
            string firstItemClass = context.State.FirstItemClass;
            string itemClass = context.State.ItemClass;

            IEnumerable<dynamic> shapes =
               context.LayoutRecord.Display == (int)LayoutRecord.Displays.Content
                   ? layoutComponentResults.Select(x => _contentManager.BuildDisplay(x.ContentItem, context.LayoutRecord.DisplayType))
                   : layoutComponentResults.Select(x => x.Properties);

            return Shape.Carousel(Id: outerDivId, Items: shapes, OuterClasses: new[] {outerDivClass}, InnerClasses: new[] {innerDivClass}, FirstItemClasses: new[] {firstItemClass}, ItemClasses: new[] {itemClass});
        }
    }
}