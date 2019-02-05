using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using Orchard.ContentManagement.Utilities;

namespace LETS.Models
{
    public class NoticePart : ContentPart<NoticePartRecord>
    {
        [Required(ErrorMessage="The notice type is required")]
        public NoticeTypePartRecord NoticeType
        {
            get { return Record.NoticeTypePartRecord; }
            set { Record.NoticeTypePartRecord = value; }
        }

        public int? Price
        {
            get { return Record.Price; }
            set { Record.Price = value; }
        }

        public string Title
        {
            get { return this.As<TitlePart>().Title; }
            set { this.As<TitlePart>().Title = value; }
        }

        public MemberPart Member
        {
            get { return this.As<CommonPart>().Owner.As<MemberPart>(); } 
        }

        private readonly LazyField<string> _strNoticeType = new LazyField<string>();
        internal LazyField<string> StrNoticeTypeField { get { return _strNoticeType; } }
        public string StrNoticeType { get { return _strNoticeType.Value; } }


    }
}
