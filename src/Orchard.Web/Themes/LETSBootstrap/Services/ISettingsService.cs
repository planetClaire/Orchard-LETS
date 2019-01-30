using LETSBootstrap.Models;
using Orchard;

namespace LETSBootstrap.Services
{
    public interface ISettingsService : IDependency
    {
        LETSBootstrapSettingsPart GetSettings();
    }
}