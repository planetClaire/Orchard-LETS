using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.Models;
using Orchard.Rules.Services;

namespace LETS.Handlers
{
    [UsedImplicitly]
    public class RulesHandler
    {
        [UsedImplicitly]
        public class RulePartHandler : ContentHandler
        {
            private readonly ISignals _signals;

            public RulePartHandler(IRulesManager rulesManager, ISignals signals) {
                _signals = signals;
                {
                    OnUnpublished<ContentPart>(
                        (context, part) =>
                            {
                                var stackTrace = new System.Diagnostics.StackTrace();
                                if (stackTrace.GetFrames().FirstOrDefault(f => f.GetMethod().Module.Name.Contains("ArchiveLater")) != null)
                                {
                                    rulesManager.TriggerEvent("Content", "Archived",
                                                              () =>
                                                              new Dictionary<string, object>
                                                                  {{"Content", context.ContentItem}}
                                        );
                                    var idUser = context.ContentItem.As<CommonPart>().Owner.Id;
                                    _signals.Trigger(string.Format("letsMemberNoticesChanged{0}", idUser));
                                    _signals.Trigger(string.Format("letsMemberArchivedNoticesChanged{0}", idUser));
                                }
                            });
                }
            }
        }
    }
}
