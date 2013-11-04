using Orchard.UI.Resources;

namespace LETS
{
    public class ResourceManifest : IResourceManifestProvider
    {
        public void BuildManifests(ResourceManifestBuilder builder)
        {
            var manifest = builder.Add();
            manifest.DefineStyle("LETS.Common").SetUrl("common.css");
        }
    }
}