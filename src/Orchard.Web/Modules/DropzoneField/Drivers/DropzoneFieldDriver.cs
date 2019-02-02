using System;
using DropzoneField.Settings;
using DropzoneField.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;

namespace DropzoneField.Drivers
{
    public class DropzoneFieldDriver : ContentFieldDriver<Fields.DropzoneField>
    {
        private readonly IOrchardServices _orchardServices;

        public DropzoneFieldDriver(IOrchardServices orchardServices)
        {
            _orchardServices = orchardServices;
        }

        private const string TemplateName = "Fields/Dropzone"; 
        private const string TokenContentType = "{content-type}";
        private const string TokenFieldName = "{field-name}";
        private const string TokenContentItemId = "{content-item-id}";
        private const string TokenUserId = "{user-id}";

        private static string GetPrefix(ContentField field, ContentPart part)
        {
            return part.PartDefinition.Name + "." + field.Name;
        }

        private static string GetDifferentiator(Fields.DropzoneField field, ContentPart part)
        {
            return field.Name;
        }

        protected override DriverResult Display(ContentPart part, Fields.DropzoneField field, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Fields_Dropzone", GetDifferentiator(field, part),
                () =>
                    shapeHelper.Fields_Dropzone( // this is the actual Shape which will be resolved (Fields/Dropzone.cshtml)
                        ContentPart: part, // it will allow to access the content item
                        ContentField: field
                        )
                    );
        }

        protected override DriverResult Editor(ContentPart part, Fields.DropzoneField field, dynamic shapeHelper)
        {
            var settings = field.PartFieldDefinition.Settings.GetModel<DropzoneFieldSettings>();
            var DropzoneMediaFolder = GetDropzoneMediaFolder(part, field, settings);

            var viewModel = new DropzoneFieldViewModel
            {
                Settings = settings,
                DropzoneMediaFolder = DropzoneMediaFolder,
                Field = field
            };

            return ContentShape("Fields_Dropzone_Edit", GetDifferentiator(field, part),
                () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: viewModel, Prefix: GetPrefix(field, part)));
        }

        protected override DriverResult Editor(ContentPart part, Fields.DropzoneField field, IUpdateModel updater, dynamic shapeHelper)
        {
            var viewModel = new DropzoneFieldViewModel
            {
                Field = field
            };
            updater.TryUpdateModel(viewModel, GetPrefix(field, part), null, null);
            return Editor(part, field, shapeHelper);
        }

        private string GetDropzoneMediaFolder(IContent part, ContentField field, DropzoneFieldSettings settings)
        {
            var DropzoneMediaFolder = settings.MediaFolder;
            if (String.IsNullOrWhiteSpace(DropzoneMediaFolder))
            {
                DropzoneMediaFolder = TokenContentType + "/" + TokenFieldName;
            }

            DropzoneMediaFolder = DropzoneMediaFolder
                .Replace(TokenContentType, part.ContentItem.ContentType)
                .Replace(TokenFieldName, field.Name)
                .Replace(TokenContentItemId, Convert.ToString(part.ContentItem.Id));
            if (!string.IsNullOrEmpty(TokenUserId))
            {
                var idUser = "anonymousUser";
                if (_orchardServices.WorkContext.CurrentUser != null)
                {
                    idUser = Convert.ToString(_orchardServices.WorkContext.CurrentUser.Id);
                }
                DropzoneMediaFolder = DropzoneMediaFolder.Replace(TokenUserId, idUser);
            }
            return DropzoneMediaFolder;
        }

        protected override void Exporting(ContentPart part, Fields.DropzoneField field, ExportContentContext context)
        {
            context.Element(field.FieldDefinition.Name + "." + field.Name).SetAttributeValue("FileName", field.FileNames);
        }

        protected override void Importing(ContentPart part, Fields.DropzoneField field, ImportContentContext context)
        {
            field.FileNames = context.Attribute(field.FieldDefinition.Name + "." + field.Name, "FileNames");
        }

    }
}
