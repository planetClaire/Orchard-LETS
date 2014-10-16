using JetBrains.Annotations;
using LETS.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace LETS.Handlers
{
    [UsedImplicitly]
    public class NoticeTypePartHandler : ContentHandler
    {
        public NoticeTypePartHandler(IRepository<NoticeTypePartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
            Filters.Add(new ActivatingFilter<NoticeTypePart>("NoticeType"));
        }
    }
}