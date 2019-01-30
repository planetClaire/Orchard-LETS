using Orchard.ContentManagement;

namespace LETSBootstrap.Models
{
    public class LETSBootstrapSettingsPart : ContentPart
    {
        public virtual string Tagline
        {
            get { return this.Retrieve(x => x.Tagline); }
            set { this.Store(x => x.Tagline, value); }
        }
    }
}