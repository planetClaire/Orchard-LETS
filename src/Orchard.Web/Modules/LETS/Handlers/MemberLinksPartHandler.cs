using LETS.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace LETS.Handlers
{
    public class MemberLinksPartHandler: ContentHandler
    {
        public MemberLinksPartHandler(IRepository<MemberLinksPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}