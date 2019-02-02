using System.ComponentModel.DataAnnotations;

namespace DropzoneField.Settings
{
    public class DropzoneFieldSettings
    {
        public const int DefaultMaxWidth = 1024;
        public const int DefaultFileLimit = 5;

        private int _maxWidth;
        private int _fileLimit;

        public string Hint { get; set; }

        public string MediaFolder { get; set; }

        [Range(0, 2048)]
        public int MaxWidth
        {
            get { return _maxWidth != 0 ? _maxWidth : DefaultMaxWidth; }
            set { _maxWidth = value; }
        }

        [Range(0, 50)]
        public int FileLimit
        {
            get { return _fileLimit != 0 ? _fileLimit : DefaultFileLimit; }
            set { _fileLimit = value; }
        }

    }
}
