using Orchard.ContentManagement;

namespace LETS.Models
{
    public class MemberLinksPart : ContentPart<MemberLinksPartRecord>
    {
        public string Website
        {
            get { return Record.Website; }
            set { Record.Website = value; }
        }

        public string Facebook
        {
            get { return Record.Facebook; }
            set { Record.Facebook = value; }
        }

        public string Twitter
        {
            get { return Record.Twitter; }
            set { Record.Twitter = value; }
        }

        public string LinkedIn
        {
            get { return Record.LinkedIn; }
            set { Record.LinkedIn = value; }
        }

        public string Tumblr
        {
            get { return Record.Tumblr; }
            set { Record.Tumblr = value; }
        }

        public string Flickr
        {
            get { return Record.Flickr; }
            set { Record.Flickr = value; }
        }

        public string Pinterest
        {
            get { return Record.Pinterest; }
            set { Record.Pinterest = value; }
        }

        public string GooglePlus
        {
            get { return Record.GooglePlus; }
            set { Record.GooglePlus = value; }
        }

        public string Goodreads
        {
            get { return Record.Goodreads; }
            set { Record.Goodreads = value; }
        }

        public string Skype
        {
            get { return Record.Skype; }
            set { Record.Skype = value; }
        }

        public bool NoLinks
        {
            get
            {
                return string.IsNullOrEmpty(Website)
                       && string.IsNullOrEmpty(GooglePlus)
                       && string.IsNullOrEmpty(Goodreads)
                       && string.IsNullOrEmpty(Pinterest)
                       && string.IsNullOrEmpty(Flickr)
                       && string.IsNullOrEmpty(Tumblr)
                       && string.IsNullOrEmpty(LinkedIn)
                       && string.IsNullOrEmpty(Twitter)
                       && string.IsNullOrEmpty(Skype)
                       && string.IsNullOrEmpty(Facebook);
            }
        }
    }
}