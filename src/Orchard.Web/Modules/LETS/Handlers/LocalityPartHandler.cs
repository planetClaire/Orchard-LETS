using JetBrains.Annotations;
using LETS.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Title.Models;
using Orchard.Data;

namespace LETS.Handlers
{
    [UsedImplicitly]
    public class LocalityPartHandler : ContentHandler
    {
        public LocalityPartHandler(IRepository<LocalityPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}