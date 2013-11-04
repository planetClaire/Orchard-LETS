using System;
using Orchard.ContentManagement.Records;

namespace LETS.Models
{
    public class MemberAdminPartRecord : ContentPartRecord
    {
        public virtual int OpeningBalance { get; set; }
        public virtual DateTime? JoinDate { get; set; }
        public virtual MemberType MemberType { get; set; }
    }
}