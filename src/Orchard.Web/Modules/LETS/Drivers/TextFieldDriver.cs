using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Fields;
using Orchard.Core.Common.Settings;
using Orchard.Services;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LETS.Drivers
{
    public class TextFieldDriver : ContentFieldDriver<TextField>
    {
        private readonly IEnumerable<IHtmlFilter> _htmlFilters;

        public TextFieldDriver(IEnumerable<IHtmlFilter> htmlFilters)
        {
            _htmlFilters = htmlFilters;
        }

        private string GetDifferentiator(TextField field, ContentPart part)
        {
            return field.Name;
        }

        protected override DriverResult Display(ContentPart part, TextField field, string displayType, dynamic shapeHelper)
        {
            var settings = field.PartFieldDefinition.Settings.GetModel<TextFieldSettings>();
            object fieldValue = new HtmlString(_htmlFilters.Aggregate(field.Value, (text, filter) => filter.ProcessContent(text, settings.Flavor)));
            return ContentShape("Fields_Common_Text_Summary", GetDifferentiator(field, part),
                () => shapeHelper.Fields_Common_Text_Summary(Name: field.Name, Value: fieldValue));
        }
    }
}