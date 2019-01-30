using System.Linq;
using LETSBootstrap.Models;
using Orchard.Data;

namespace LETSBootstrap.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly IRepository<LETSBootstrapSettingsPart> _repository;

        public SettingsService(IRepository<LETSBootstrapSettingsPart> repository)
        {
            _repository = repository;
        }

        public LETSBootstrapSettingsPart GetSettings()
        {
            var settings = _repository.Table.SingleOrDefault();
            if (settings == null)
            {
                _repository.Create(settings = new LETSBootstrapSettingsPart());
            }

            return settings;
        }
    }
}