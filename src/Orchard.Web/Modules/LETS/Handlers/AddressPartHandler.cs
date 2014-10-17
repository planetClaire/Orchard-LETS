using JetBrains.Annotations;
using LETS.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace LETS.Handlers
{
    [UsedImplicitly]
    public class AddressPartHandler : ContentHandler
    {
        public AddressPartHandler(IRepository<AddressPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}