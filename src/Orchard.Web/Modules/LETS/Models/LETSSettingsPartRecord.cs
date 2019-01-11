using System;
using Orchard.ContentManagement.Records;

namespace LETS.Models
{
    public class LETSSettingsPartRecord : ContentPartRecord
    {
        public virtual string CurrencyUnit { get; set; }
        public virtual int IdRoleMember { get; set; }
        public virtual int IdTaxonomyNotices { get; set; }
        public virtual int MaximumNoticeAgeDays { get; set; }
        public virtual int OldestRecordableTransactionDays { get; set; }
        public virtual int DefaultTurnoverDays { get; set; }
        public virtual bool UseDemurrage { get; set; }
        public virtual DateTime? DemurrageStartDate { get; set; }
        public virtual int DemurrageTimeIntervalDays { get; set; }
        public virtual string DemurrageSteps { get; set; }
        public virtual int? IdDemurrageRecipient { get; set; }
        public virtual string IdMailChimpList { get; set; }
        public virtual string MemberLinksZone { get; set; }
        public virtual string MemberLinksPosition { get; set; }
        public virtual string MemberNoticesZone { get; set; }
        public virtual string MemberNoticesPosition { get; set; }
    }
}