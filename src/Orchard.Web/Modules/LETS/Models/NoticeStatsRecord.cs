using System;

namespace LETS.Models
{
    public class NoticeStatsRecord
    {
        public virtual int Id { get; set; }
        public virtual DateTime DateCollected { get; set; }
        public virtual int IdNoticeType { get; set; }
        public virtual int NoticeCount { get; set; }
    }
}