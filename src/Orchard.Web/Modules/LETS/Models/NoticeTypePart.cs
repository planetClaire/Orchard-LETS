using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;

namespace LETS.Models
{
    public class NoticeTypePart : ContentPart<NoticeTypePartRecord>
    {
        public string Title
        {
            get { return this.As<TitlePart>().Title; }    
        }

        public int RequiredCount
        {
            get { return Record.RequiredCount; }
            set { Record.RequiredCount = value; }
        }

        public int SortOrder
        {
            get { return Record.SortOrder; }
            set { Record.SortOrder = value; }
        }
    }
}