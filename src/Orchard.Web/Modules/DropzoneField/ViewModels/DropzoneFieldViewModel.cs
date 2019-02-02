using DropzoneField.Settings;

namespace DropzoneField.ViewModels
{
    public class DropzoneFieldViewModel
    {
        public string DropzoneMediaFolder { get; set; }
        public DropzoneFieldSettings Settings { get; set; }
        public Fields.DropzoneField Field { get; set; }
    }
}
