using Orchard.ContentManagement.Records;

namespace LETS.Models
{
    public class NoticePartRecord : ContentPartRecord
    {
        public virtual NoticeTypePartRecord NoticeTypePartRecord { get; set; }
        public virtual int? Price { get; set; }
    }
}
