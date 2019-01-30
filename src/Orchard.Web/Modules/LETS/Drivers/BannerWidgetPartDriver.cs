using LETS.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace LETS.Drivers
{
    public class BannerWidgetPartDriver : ContentPartDriver<BannerWidgetPart>
    {
        protected override DriverResult Display(BannerWidgetPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_BannerWidget", () => shapeHelper.Parts_BannerWidget(
                Part: part
                ));
        }

        protected override DriverResult Editor(BannerWidgetPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_BannerWidget_Edit",
                                        () => shapeHelper.EditorTemplate(
                                            TemplateName: "Parts/BannerWidget",
                                            Model: part,
                                            Prefix: Prefix));
        }

        protected override DriverResult Editor(BannerWidgetPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }

    }
}