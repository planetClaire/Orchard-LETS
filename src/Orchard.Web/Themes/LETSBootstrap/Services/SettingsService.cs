using System.Linq;
using LETSBootstrap.Models;
using Orchard.Data;

namespace LETSBootstrap.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly IRepository<SettingsRecord> _repository;

        public SettingsService(IRepository<SettingsRecord> repository)
        {
            _repository = repository;
        }

        public SettingsRecord GetSettings()
        {
            var settings = _repository.Table.SingleOrDefault();
            if (settings == null)
            {
                _repository.Create(settings = new SettingsRecord());
            }

            return settings;
        }
    }
}