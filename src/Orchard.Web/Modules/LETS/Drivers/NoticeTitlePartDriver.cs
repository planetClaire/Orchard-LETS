using LETS.Models;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Title.Models;

namespace LETS.Drivers
{
    public class NoticeTitlePartDriver : ContentPartDriver<TitlePart>
    {
        protected override DriverResult Display(TitlePart part, string displayType, dynamic shapeHelper) 
        {
            if (part.ContentItem.Has(typeof(NoticePart))) {
                return Combined(
                    ContentShape("Parts_Title_DetailedSummary_Notice",
                        () => shapeHelper.Parts_Title_DetailedSummary_Notice(
                            ContentPart: part
                            )),
                    ContentShape("Parts_Title_DetailedSummaryArchived_Notice",
                        () => shapeHelper.Parts_Title_DetailedSummaryArchived_Notice(
                            ContentPart: part
                            ))
                    );
            }
            return null;
        }
    }
}