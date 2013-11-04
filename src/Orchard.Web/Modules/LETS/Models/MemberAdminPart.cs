using System;
using Orchard.ContentManagement;

namespace LETS.Models
{
    public class MemberAdminPart : ContentPart<MemberAdminPartRecord>
    {
        public int OpeningBalance
        {
            get { return Record.OpeningBalance; }
            set { Record.OpeningBalance = value; }
        }

        public DateTime? JoinDate
        {
            get { return Record.JoinDate; }
            set { if (value != null) Record.JoinDate = value; }
        }

        public MemberType MemberType
        {
            get { return Record.MemberType; }
            set { Record.MemberType = value; }
        }

    }
}