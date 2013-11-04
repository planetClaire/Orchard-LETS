using Orchard.ContentManagement.Records;

namespace LETS.Models
{
    public class MembersMapPartRecord : ContentPartRecord
    {
        public virtual string ApiKey { get; set; }
        public virtual double Latitude { get; set; }
        public virtual double Longitude { get; set; }
        public virtual int MapWidth { get; set; }
        public virtual int MapHeight { get; set; }
        public virtual int ZoomLevel { get; set; }
    }
}