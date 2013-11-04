using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;

namespace LETS.Models
{
    public class MembersMapPart : ContentPart<MembersMapPartRecord> {
        [Required]
        public string ApiKey {
            get { return Record.ApiKey; }
            set { Record.ApiKey = value; }
        }

        [Required]
        public double Latitude
        {
            get { return Record.Latitude; }
            set { Record.Latitude = value; }
        }

        [Required]
        public double Longitude
        {
            get { return Record.Longitude; }
            set { Record.Longitude = value; }
        }

        [Required]
        public int MapWidth
        {
            get { return Record.MapWidth; }
            set { Record.MapWidth = value; }
        }

        [Required]
        public int MapHeight
        {
            get { return Record.MapHeight; }
            set { Record.MapHeight = value; }
        }

        [Required]
        public int ZoomLevel
        {
            get { return Record.ZoomLevel; }
            set { Record.ZoomLevel = value; }
        }

    }
}