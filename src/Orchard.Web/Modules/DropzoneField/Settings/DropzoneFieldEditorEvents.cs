using System;
using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;

namespace DropzoneField.Settings
{
    public class DropzoneFieldEditorEvents : ContentDefinitionEditorEventsBase 
    {

        public override IEnumerable<TemplateViewModel> PartFieldEditor(ContentPartFieldDefinition definition)
        {
            if (definition.FieldDefinition.Name == "DropzoneField")
            {
                var model = definition.Settings.GetModel<DropzoneFieldSettings>();
                yield return DefinitionTemplate(model);
            }
        }
 
        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(ContentPartFieldDefinitionBuilder builder, IUpdateModel updateModel) {
            var model = new DropzoneFieldSettings();
            if (builder.FieldType != "DropzoneField")
            {
                yield break;
            } 
            if (updateModel.TryUpdateModel(model, "DropzoneFieldSettings", null, null))
            {
                builder.WithSetting("DropzoneFieldSettings.Hint", model.Hint);
                builder.WithSetting("DropzoneFieldSettings.MaxWidth", Convert.ToString(model.MaxWidth));
                builder.WithSetting("DropzoneFieldSettings.MediaFolder", model.MediaFolder);
                builder.WithSetting("DropzoneFieldSettings.FileLimit", Convert.ToString(model.FileLimit));
            }

            yield return DefinitionTemplate(model);
        }
    }
}