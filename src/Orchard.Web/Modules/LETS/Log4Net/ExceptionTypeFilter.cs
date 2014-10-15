using log4net.Core;
using log4net.Filter;
using Orchard.Security;

namespace LETS.Log4Net
{
    public class ExceptionTypeFilter : FilterSkeleton
    {
        override public FilterDecision Decide(LoggingEvent loggingEvent) {
            if (loggingEvent.ExceptionObject is OrchardSecurityException) {
                return FilterDecision.Deny;
            }
            return FilterDecision.Accept;
        }
    }
}