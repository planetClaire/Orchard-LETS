using System.Collections.Generic;
using Orchard;
using Orchard.Environment.Extensions;
using Orchard.UI.Notify;

namespace LETS.Notifications
{
    public interface INotificationManager : IDependency
    {
        /// <summary>
        /// Returns all notifications to display per zone
        /// </summary>
        IEnumerable<NotifyEntry> GetNotifications();
    }
}