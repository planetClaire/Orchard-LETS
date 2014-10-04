using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Workflows.Services;

namespace LETS.Workflow
{
    public class UnpublishHandler : ContentHandler
    {
        public UnpublishHandler(IWorkflowManager workflowManager)
        {
            OnUnpublished<ContentPart>(
                (context, part) =>
                    workflowManager.TriggerEvent("ContentUnpublished",
                    context.ContentItem,
                    () => new Dictionary<string, object> { { "Content", context.ContentItem } }));
        }
    }
}