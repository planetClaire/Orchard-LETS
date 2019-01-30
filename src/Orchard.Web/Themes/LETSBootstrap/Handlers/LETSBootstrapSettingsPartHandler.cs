using LETSBootstrap.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;

namespace LETSBootstrap.Handlers
{
    public class LETSBootstrapSettingsPartHandler : ContentHandler
    {
        public LETSBootstrapSettingsPartHandler()
        {
            T = NullLocalizer.Instance;
            Filters.Add(new ActivatingFilter<LETSBootstrapSettingsPart>("Site"));
            Filters.Add(new TemplateFilterForPart<LETSBootstrapSettingsPart>("LETSBootstrapSettings", "Parts/LETSBootstrapSettings", "ThemeOptions"));
        }

        public Localizer T { get; set; }

        protected override void GetItemMetadata(GetContentItemMetadataContext context)
        {
            if (context.ContentItem.ContentType != "Site")
                return;
            base.GetItemMetadata(context);
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Theme Options")));
        }
    }
}