using System.Linq;
using JetBrains.Annotations;
using LETS.Models;
using LETS.Services;
using LETS.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace LETS.Drivers
{
    [UsedImplicitly]
    public class AddressPartDriver : ContentPartDriver<AddressPart>
    {
        private readonly IAddressService _addressService;
        private readonly IContentManager _contentManager;

        public AddressPartDriver(IAddressService addressService, IContentManager contentManager)
        {
            _addressService = addressService;
            _contentManager = contentManager;
        }

        protected override DriverResult Display(AddressPart part, string displayType, dynamic shapeHelper) {
            return ContentShape("Parts_Address",
                                () => {
                                    var locality = _addressService.GetLocality(part.Locality.Id);
                                    return shapeHelper.Parts_Address(
                                        Address: part,
                                        Locality: locality.Title,
                                        Postcode: locality.Postcode
                                        );
                                });
        }

        protected override DriverResult Editor(AddressPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_Address_Edit",
            () => shapeHelper.EditorTemplate(
                TemplateName: "Parts/Address",
                Model: BuildEditorViewModel(part),
                Prefix: Prefix));
        }

        protected override DriverResult Editor(AddressPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            var model = new EditAddressViewModel();
            updater.TryUpdateModel(model, Prefix, null, null);

            if (part.ContentItem.Id != 0)
            {
                _addressService.UpdateAddressForContentItem(part.ContentItem, model);
            }
            return Editor(part, shapeHelper);
        }

        private EditAddressViewModel BuildEditorViewModel(AddressPart part)
        {
            var evm = new EditAddressViewModel
            {
                StreetAddress = part.StreetAddress,
                Localities= _addressService.GetLocalityViews()
            };
            if (part.Locality != null)
            {
                evm.IdLocality = part.Locality.Id;
            }
            return evm;
        }

        protected override void Importing(AddressPart part, Orchard.ContentManagement.Handlers.ImportContentContext context)
        {
            part.StreetAddress = context.Attribute(part.PartDefinition.Name, "StreetAddress");
        }

        protected override void Imported(AddressPart part, Orchard.ContentManagement.Handlers.ImportContentContext context)
        {
            var locality = context.Attribute(part.PartDefinition.Name, "Locality");
            if (locality != null)
            {
                part.Record.LocalityPartRecord = context.GetItemFromSession(locality).As<LocalityPart>().Record;
            }
        }

        protected override void Exporting(AddressPart part, Orchard.ContentManagement.Handlers.ExportContentContext context)
        {
            context.Element(part.PartDefinition.Name).SetAttributeValue("StreetAddress", part.StreetAddress);
            var localityPart = _contentManager.Query<LocalityPart, LocalityPartRecord>("Locality").Where(x => x.Id == part.Record.LocalityPartRecord.Id).List().FirstOrDefault();
            if (localityPart != null)
            {
                var localityIdentity = _contentManager.GetItemMetadata(localityPart).Identity;
                context.Element(part.PartDefinition.Name).SetAttributeValue("Locality", localityIdentity.ToString());
            }
        }
    }
}