using LETS.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace LETS.Handlers
{
    public class MemberAccessOnlyPartHandler: ContentHandler
    {
        public MemberAccessOnlyPartHandler(IRepository<MemberAccessOnlyPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));

        }
    }
}