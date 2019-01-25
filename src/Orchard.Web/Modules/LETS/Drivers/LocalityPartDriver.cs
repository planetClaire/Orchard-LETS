using LETS.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace LETS.Drivers
{
    public class LocalityPartDriver : ContentPartDriver<LocalityPart>
    {
        protected override DriverResult Display(LocalityPart part, string displayType, dynamic shapeHelper)
        {
            return Combined(
                ContentShape("Parts_Locality", () => shapeHelper.Parts_Locality()),
                ContentShape("Parts_Locality_SummaryAdmin", () => shapeHelper.Parts_Locality_SummaryAdmin(Locality:part))
                );
        }

        protected override DriverResult Editor(LocalityPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_Locality_Edit", 
                                () => shapeHelper.EditorTemplate(
                                    TemplateName: "Parts/Locality", 
                                    Model: part, 
                                    Prefix: Prefix));
        }

        protected override DriverResult Editor(LocalityPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }

        protected override void Importing(LocalityPart part, Orchard.ContentManagement.Handlers.ImportContentContext context)
        {
            part.Postcode = context.Attribute(part.PartDefinition.Name, "Postcode");
        }

        protected override void Exporting(LocalityPart part, Orchard.ContentManagement.Handlers.ExportContentContext context)
        {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Postcode", part.Postcode);
        }
    }
}