using System.Web.Script.Serialization;
using LETS.Models;
using LETS.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace LETS.Drivers
{
    public class MembersMapPartDriver : ContentPartDriver<MembersMapPart>
    {
        private readonly IMembersMapService _membersMapService;

        public MembersMapPartDriver(IMembersMapService membersMapService) {
            _membersMapService = membersMapService;
        }

        protected override DriverResult Display(MembersMapPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_MembersMap", () => shapeHelper.Parts_MembersMap(
                MembersMap: part,
                MemberMarkers: (new JavaScriptSerializer()).Serialize(_membersMapService.MemberMarkers())
                ));
        }

        protected override DriverResult Editor(MembersMapPart part, dynamic shapeHelper)
        {
            var result = ContentShape("Parts_MembersMap_Edit",
                                        () => shapeHelper.EditorTemplate(
                                            TemplateName: "Parts/MembersMap",
                                            Model: part,
                                            Prefix: Prefix));
            return result;
        }

        protected override DriverResult Editor(MembersMapPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }

    }
}