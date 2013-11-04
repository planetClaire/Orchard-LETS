using JetBrains.Annotations;
using LETS.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace LETS.Drivers
{
    [UsedImplicitly]
    public class NoticeTypePartDriver : ContentPartDriver<NoticeTypePart>
    {
        protected override DriverResult Display(NoticeTypePart part, string displayType, dynamic shapeHelper)
        {
            return Combined(
                ContentShape("Parts_NoticeType", () => shapeHelper.Parts_NoticeType()),
                ContentShape("Parts_NoticeType_SummaryAdmin", () => shapeHelper.Parts_NoticeType_SummaryAdmin(NoticeType: part))
                );
        }

        protected override DriverResult Editor(NoticeTypePart part, dynamic shapeHelper)
        {
            var result = ContentShape("Parts_NoticeType_Edit",
                                        () => shapeHelper.EditorTemplate(
                                            TemplateName: "Parts/NoticeType",
                                            Model: part,
                                            Prefix: Prefix));
            return result;
        }

        protected override DriverResult Editor(NoticeTypePart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }

        protected override void Importing(NoticeTypePart part, Orchard.ContentManagement.Handlers.ImportContentContext context)
        {
            var requiredCount = context.Attribute(part.PartDefinition.Name, "RequiredCount");
            if (requiredCount != null)
            {
                part.RequiredCount = int.Parse(requiredCount);
            }
            var sortOrder = context.Attribute(part.PartDefinition.Name, "SortOrder");
            if (sortOrder != null)
            {
                part.SortOrder = int.Parse(sortOrder);
            }
        }

        protected override void Exporting(NoticeTypePart part, Orchard.ContentManagement.Handlers.ExportContentContext context)
        {
            context.Element(part.PartDefinition.Name).SetAttributeValue("RequiredCount", part.RequiredCount);
            context.Element(part.PartDefinition.Name).SetAttributeValue("SortOrder", part.SortOrder);
        }
    }
}