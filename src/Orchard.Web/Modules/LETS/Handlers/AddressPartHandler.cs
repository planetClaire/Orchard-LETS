using LETS.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace LETS.Handlers
{
    public class AddressPartHandler : ContentHandler
    {
        public AddressPartHandler(IRepository<AddressPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}