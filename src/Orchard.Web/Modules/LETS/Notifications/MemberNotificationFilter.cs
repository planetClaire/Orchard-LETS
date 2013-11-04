using System.Linq;
using System.Web.Mvc;
using JetBrains.Annotations;
using Orchard;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.Mvc.Filters;

namespace LETS.Notifications
{
    [UsedImplicitly]
    public class MemberNotificationFilter : FilterProvider, IResultFilter
    {
        private readonly INotificationManager _memberNotificationManager;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly dynamic _shapeFactory;

        public MemberNotificationFilter(INotificationManager memberNotificationManager, IWorkContextAccessor workContextAccessor, IShapeFactory shapeFactory)
        {
            _memberNotificationManager = memberNotificationManager;
            _workContextAccessor = workContextAccessor;
            _shapeFactory = shapeFactory;
        }

        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
            // if it's not a view result, a redirect for example
            if (!(filterContext.Result is ViewResultBase))
                return;

            //var messageEntries = _memberNotificationManager.GetNotifications().ToList();
            var messageEntries = _memberNotificationManager.GetNotifications();
            if (!messageEntries.Any())
                return;

            var messagesZone = _workContextAccessor.GetContext(filterContext).Layout.Zones["Messages"];
            foreach (var messageEntry in messageEntries)
                messagesZone = messagesZone.Add(_shapeFactory.Message(messageEntry));
        }

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
        }
    }
}