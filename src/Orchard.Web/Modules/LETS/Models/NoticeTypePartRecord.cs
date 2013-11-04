using System.Collections.Generic;
using Orchard.ContentManagement.Records;

namespace LETS.Models
{
    public class NoticeTypePartRecord : ContentPartRecord
    {
        public virtual int RequiredCount { get; set; }
        public virtual int SortOrder { get; set; }

        public NoticeTypePartRecord()
        {
            NoticePartRecords = new List<NoticePartRecord>();
        }

        public virtual IList<NoticePartRecord> NoticePartRecords { get; set; }
    }
}   