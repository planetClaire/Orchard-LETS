using Orchard;

namespace LETS.Services
{
    public interface IStatsService : IDependency
    {
        void SaveStats();
    }
}