using LETS.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace LETS.Handlers
{
    public class MembersMapPartHandler : ContentHandler
    {
        public MembersMapPartHandler(IRepository<MembersMapPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}