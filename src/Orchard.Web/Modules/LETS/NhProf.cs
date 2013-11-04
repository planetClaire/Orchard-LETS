using Orchard.Environment;

namespace LETS
{
    public class NhProf : IOrchardShellEvents
    {
        public void Activated()
        {
            HibernatingRhinos.Profiler.Appender.NHibernate.NHibernateProfiler.Initialize(); 
        }

        public void Terminating()
        {
        }
    }
}