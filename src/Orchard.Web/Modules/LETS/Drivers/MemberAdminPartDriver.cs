using System;
using System.Globalization;
using JetBrains.Annotations;
using LETS.Models;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Security;

namespace LETS.Drivers
{
    [UsedImplicitly]
    public class MemberAdminPartDriver : ContentPartDriver<MemberAdminPart>
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IAuthenticationService _authenticationService;
        private readonly ISignals _signals;

        public MemberAdminPartDriver(IAuthorizationService authorizationService, IAuthenticationService authenticationService, ISignals signals)
        {
            _authorizationService = authorizationService;
            _authenticationService = authenticationService;
            _signals = signals;
        }

        protected override DriverResult Display(MemberAdminPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_MemberAdmin", () => shapeHelper.Parts_MemberAdmin(MemberAdmin: part));
        }

        protected override DriverResult Editor(MemberAdminPart part, dynamic shapeHelper)
        {
            if (!_authorizationService.TryCheckAccess(Permissions.AdminMemberContent, _authenticationService.GetAuthenticatedUser(), part))
                return null;

            return ContentShape("Parts_MemberAdmin_Edit",
                                () => shapeHelper.EditorTemplate(
                                    TemplateName: "Parts/MemberAdmin",
                                    Model: part,
                                    Prefix: Prefix));
        }

        protected override DriverResult Editor(MemberAdminPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (!_authorizationService.TryCheckAccess(Permissions.AdminMemberContent, _authenticationService.GetAuthenticatedUser(), part))
                return null;

            updater.TryUpdateModel(part, Prefix, null, null);
            _signals.Trigger(string.Format("letsMemberPartsType{0}Changed", part.As<MemberAdminPart>().MemberType));
            _signals.Trigger("letsMemberListChanged");
            return Editor(part, shapeHelper);
        }

        protected override void Importing(MemberAdminPart part, Orchard.ContentManagement.Handlers.ImportContentContext context)
        {
            var openingBalance = context.Attribute(part.PartDefinition.Name, "OpeningBalance");
            if (openingBalance != null)
            {
                part.OpeningBalance = int.Parse(context.Attribute(part.PartDefinition.Name, "OpeningBalance"));
            }
            part.JoinDate = DateTime.Parse(context.Attribute(part.PartDefinition.Name, "JoinDate"), CultureInfo.InvariantCulture);
            part.MemberType = (MemberType) Enum.Parse(typeof(MemberType), context.Attribute(part.PartDefinition.Name, "MemberType"));
        }

        protected override void Exporting(MemberAdminPart part, Orchard.ContentManagement.Handlers.ExportContentContext context)
        {
            context.Element(part.PartDefinition.Name).SetAttributeValue("OpeningBalance", part.OpeningBalance);
            context.Element(part.PartDefinition.Name).SetAttributeValue("JoinDate", part.JoinDate);
            context.Element(part.PartDefinition.Name).SetAttributeValue("MemberType", part.MemberType);
        }
    }
}