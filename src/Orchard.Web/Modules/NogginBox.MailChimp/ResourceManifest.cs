using Orchard.UI.Resources;

namespace NogginBox.MailChimp {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineStyle("MailChimp").SetUrl("MailChimp.css");
        }
    }
}
