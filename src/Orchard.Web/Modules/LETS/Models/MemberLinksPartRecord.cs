using Orchard.ContentManagement.Records;

namespace LETS.Models
{
    public class MemberLinksPartRecord : ContentPartRecord
    {
        public virtual string Website { get; set; }
        public virtual string Facebook { get; set; }
        public virtual string Instagram { get; set; }
        public virtual string Twitter { get; set; }
        public virtual string LinkedIn { get; set; }
        public virtual string Tumblr { get; set; }
        public virtual string Flickr { get; set; }
        public virtual string Pinterest { get; set; }
        public virtual string GooglePlus { get; set; }
        public virtual string Goodreads { get; set; }
        public virtual string Skype { get; set; }
    }
}