using LETS.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Localization;

namespace LETS.Handlers
{
    public class LETSSettingsPartHandler: ContentHandler {
        public Localizer T { get; set; }

        public LETSSettingsPartHandler(IRepository<LETSSettingsPartRecord> repository)
        {
            T = NullLocalizer.Instance;
            Filters.Add(new ActivatingFilter<LETSSettingsPart>("Site"));
            Filters.Add(StorageFilter.For(repository));
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context)
        {
            if (context.ContentItem.ContentType != "Site")
                return;
            base.GetItemMetadata(context);
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("LETS")));
        }
    }
}