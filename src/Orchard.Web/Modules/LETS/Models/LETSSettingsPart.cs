using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Orchard.ContentManagement;

namespace LETS.Models
{
    public class LETSSettingsPart : ContentPart<LETSSettingsPartRecord>
    {
        [Required]
        public string CurrencyUnit
        {
            get { return Record.CurrencyUnit; }
            set { Record.CurrencyUnit = value; }
        }

        [Required]
        public int IdRoleMember
        {
            get { return Record.IdRoleMember; }
            set { Record.IdRoleMember = value; }
        }

        [Required]
        public int IdTaxonomyNotices
        {
            get { return Record.IdTaxonomyNotices; }
            set { Record.IdTaxonomyNotices = value; }
        }

        [Required, Range(1, 999999, ErrorMessage = "Please enter a number between 1 and 999999")]
        public int MaximumNoticeAgeDays
        {
            get { return Record.MaximumNoticeAgeDays; }
            set { Record.MaximumNoticeAgeDays = value; }
        }

        [Required, Range(1, 90000, ErrorMessage = "Please enter a number between 1 and 90000")]
        public int OldestRecordableTransactionDays 
        {
            get { return Record.OldestRecordableTransactionDays; }
            set { Record.OldestRecordableTransactionDays = value; }
        }

        [Required, Range(1, 365, ErrorMessage = "Please enter a number between 1 and 365")]
        public int DefaultTurnoverDays
        {
            get { return Record.DefaultTurnoverDays; }
            set { Record.DefaultTurnoverDays = value; }
        }

        public bool UseDemurrage
        {
            get { return Record.UseDemurrage; }
            set { Record.UseDemurrage = value; }
        }

        public DateTime? DemurrageStartDate
        {
            get { return Record.DemurrageStartDate; }
            set { if (value != null) Record.DemurrageStartDate = value; }
        }

        [Range(0, 365, ErrorMessage = "Please enter a number between 1 and 365")]
        public int DemurrageTimeIntervalDays
        {
            get { return Record.DemurrageTimeIntervalDays; }
            set { Record.DemurrageTimeIntervalDays = value; }
        }

        public string DemurrageSteps
        {
            get { return Record.DemurrageSteps; }
            set { Record.DemurrageSteps = value; }
        }

        [Required]
        public int IdDemurrageRecipient
        {
            get { return Record.IdDemurrageRecipient; }
            set { Record.IdDemurrageRecipient = value; }
        }

        public string IdMailChimpList
        {
            get { return Record.IdMailChimpList; }
            set { Record.IdMailChimpList = value; }
        }

        public string MemberLinksZone
        {
            get { return Record.MemberLinksZone; }
            set { Record.MemberLinksZone = value; }
        }

        public string MemberLinksPosition
        {
            get { return Record.MemberLinksPosition; }
            set { Record.MemberLinksPosition = value; }
        }

        [Required]
        public string MemberNoticesZone
        {
            get { return Record.MemberNoticesZone; }
            set { Record.MemberNoticesZone = value; }
        }

        [Required]
        public string MemberNoticesPosition
        {
            get { return Record.MemberNoticesPosition; }
            set { Record.MemberNoticesPosition = value; }
        }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Record.CurrencyUnit) && Record.IdRoleMember != 0 &&
                   Record.IdTaxonomyNotices != 0 && Record.MaximumNoticeAgeDays != 0 &&
                   Record.OldestRecordableTransactionDays != 0 && Record.DefaultTurnoverDays != 0 &&
                   !string.IsNullOrEmpty(Record.MemberNoticesZone) && !string.IsNullOrEmpty(Record.MemberNoticesPosition) &&
                   DemurrageSettingsValid();
        }

        private bool DemurrageSettingsValid()
        {
            return !UseDemurrage || DemurrageTimeIntervalDays > 0 && DemurrageStepsValid(DemurrageStepsStringArray) && IdDemurrageRecipient != 0 && DemurrageStartDate != null;
        }

        private bool DemurrageStepsValid(IEnumerable<string> stepsValue)
        {
            var isValid = true;
            if (stepsValue != null)
            {
                foreach (var step in stepsValue)
                {
                    int percentStep;
                    if (!int.TryParse(step, out percentStep) || percentStep <= 0 || percentStep > 99)
                    {
                        isValid = false;
                    }
                }
            }
            return isValid;
        }

        private string[] DemurrageStepsStringArray
        {
            get
            {
                return !string.IsNullOrEmpty(DemurrageSteps) ? Record.DemurrageSteps.Split(',') : new string[0];
            }
        }

        public IEnumerable<int>  DemurrageStepsList
        {
            get
            {
                var demurrageStepsStringArray = DemurrageStepsStringArray;
                var demurrageStepsList = new List<int>();
                if (DemurrageStepsValid(demurrageStepsStringArray))
                {
                    demurrageStepsList.AddRange(demurrageStepsStringArray.Select(int.Parse));
                }
                return demurrageStepsList;
            }
        }
    }
}