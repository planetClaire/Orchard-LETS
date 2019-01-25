using System.Collections.Generic;
using System.Linq;
using Orchard.Environment.Extensions;
using Orchard.UI.Notify;

namespace LETS.Notifications
{
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