using Orchard.Localization;
using Orchard.Workflows.Activities;

namespace LETS.Workflow
{
    public class ContentUnpublishedActivity : ContentActivity
    {
        public override string Name {
            get { return "ContentUnpublished"; }
        }

        public override LocalizedString Description {
            get { return T("Content is unpublished."); }
        }
    }
}