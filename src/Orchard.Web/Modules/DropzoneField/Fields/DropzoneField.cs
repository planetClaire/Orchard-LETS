using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage;

namespace DropzoneField.Fields
{
    public class DropzoneField: ContentField
    {
        public string FileNames
        {
            get { return Storage.Get<string>(); }
            set { Storage.Set(value); }
        }

    }

}