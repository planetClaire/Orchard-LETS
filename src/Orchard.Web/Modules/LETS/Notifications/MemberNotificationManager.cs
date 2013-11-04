using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.Environment.Extensions;
using Orchard.UI.Notify;

namespace LETS.Notifications
{
    [UsedImplicitly]
    public class MemberNotificationManager : INotificationManager
    {
        private readonly IEnumerable<INotificationProvider> _memberNotificationProviders;

        public MemberNotificationManager(IEnumerable<INotificationProvider> memberNotificationProviders)
        {
            _memberNotificationProviders = memberNotificationProviders;
        }

        public IEnumerable<NotifyEntry> GetNotifications()
        {
            return _memberNotificationProviders
                .SelectMany(n => n.GetNotifications());
        }
    }
}