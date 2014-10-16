using LETS.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace LETS.Handlers
{
    public class MemberAdminPartHandler : ContentHandler
    {
        public MemberAdminPartHandler(IRepository<MemberAdminPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
            Filters.Add(new ActivatingFilter<MemberAdminPart>("User"));
        }
    }
}