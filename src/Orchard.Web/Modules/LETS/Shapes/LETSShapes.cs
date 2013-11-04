using System;
using System.Web;
using Orchard;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Localization;

namespace LETS.Shapes
{
    public class LETSShapes : IShapeTableProvider
    {
        private readonly IWorkContextAccessor _workContextAccessor;
        public Localizer T;

        public LETSShapes(IWorkContextAccessor workContextAccessor)
        {
            _workContextAccessor = workContextAccessor;
            T = NullLocalizer.Instance;
        }

        public void Discover(ShapeTableBuilder builder)
        {
            builder.Describe("EditorTemplate").OnDisplaying(displaying =>
            {
                if (displaying.ShapeMetadata.Prefix != null && (displaying.ShapeMetadata.Prefix.Equals("ArchiveLater") && displaying.Shape.ContentItem.ContentType.Equals("Notice")))
                {
                    displaying.ShapeMetadata.Alternates.Add("EditorTemplate__ArchiveNotice");
                }
            });
            builder.Describe("Parts_Common_Metadata_Summary").OnDisplaying(displaying =>
                {
                    if (displaying.Shape.ContentItem.ContentType.Equals("Notice") && !displaying.Shape.ContentItem.VersionRecord.Published)
                    {
                        displaying.ShapeMetadata.Wrappers.Add("MetadataSummary_DraftWrapper");
                    }
                });
            builder.Describe("Content").OnDisplaying(displaying =>
                {
                    if (displaying.ShapeMetadata.DisplayType.Equals("DetailedSummary"))
                    {
                        displaying.ShapeMetadata.Wrappers.Add("Content_DetailedSummaryWrapper");
                    }
                });
        }

        [Shape]
        public IHtmlString ArchiveState(dynamic Display, DateTime? archiveDateTimeUtc)
        {
            if (!archiveDateTimeUtc.HasValue)
            {
                return T("never");
            }

            var days = (archiveDateTimeUtc - DateTime.UtcNow).Value.Days + 1;
            var strDays = "day";
            if (days != 1) {
                strDays += "s";
            }
            var htmlString = string.Format("Expires in {0} {1}", days, strDays);
            if (days < 8) {
                htmlString = string.Format("<strong>{0}</strong>", htmlString);
            }
            return T(htmlString);
        }
    }
}