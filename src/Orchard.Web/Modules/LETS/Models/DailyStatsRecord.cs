using System;

namespace LETS.Models
{
    public class DailyStatsRecord
    {
        public virtual int Id { get; set; }
        public virtual DateTime DateCollected { get; set; }
        public virtual int TotalTurnover { get; set; }
        public virtual int MemberCount { get; set; }
    }
}