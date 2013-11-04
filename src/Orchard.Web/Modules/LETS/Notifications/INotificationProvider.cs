using System.Collections.Generic;
using Orchard;
using Orchard.UI.Notify;

namespace LETS.Notifications
{
    public interface INotificationProvider : IDependency
    {
        /// <summary>
        /// Returns all notifications to display per zone
        /// </summary>
        IEnumerable<NotifyEntry> GetNotifications();
    }
}
