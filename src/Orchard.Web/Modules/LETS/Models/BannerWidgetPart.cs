using Orchard.ContentManagement;

namespace LETS.Models
{
    public class BannerWidgetPart : ContentPart
    {
        public string PrimaryButtonText
        {
            get { return this.Retrieve(x => x.PrimaryButtonText); }
            set { this.Store(x => x.PrimaryButtonText, value); }
        }

        public string PrimaryButtonUrl
        {
            get { return this.Retrieve(x => x.PrimaryButtonUrl); }
            set { this.Store(x => x.PrimaryButtonUrl, value); }
        }

        public string SecondaryButtonText
        {
            get { return this.Retrieve(x => x.SecondaryButtonText); }
            set { this.Store(x => x.SecondaryButtonText, value); }
        }

        public string SecondaryButtonUrl
        {
            get { return this.Retrieve(x => x.SecondaryButtonUrl); }
            set { this.Store(x => x.SecondaryButtonUrl, value); }
        }

    }
}