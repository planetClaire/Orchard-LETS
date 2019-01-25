using System.Linq;
using System.Web.Mvc;
using Orchard.Taxonomies.Services;
using LETS.Models;
using LETS.Services;
using LETS.ViewModels;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Roles.Services;

namespace LETS.Drivers
{
    public class LETSSettingsPartDriver: ContentPartDriver<LETSSettingsPart> {

        private const string TemplateName = "Parts/LETSSettings";
        
        private readonly IRoleService _roleService;
        private readonly ITaxonomyService _taxonomyService;
        private readonly IMemberService _memberService;
        private readonly ISignals _signals;

        protected override string Prefix { get { return "LETSSettings"; } }


        public LETSSettingsPartDriver(IOrchardServices services, IRoleService roleService, ITaxonomyService taxonomyService, IMemberService memberService, ISignals signals)
        {
            _roleService = roleService;
            _taxonomyService = taxonomyService;
            _memberService = memberService;
            _signals = signals;
        }

        protected override DriverResult Editor(LETSSettingsPart part, dynamic shapeHelper)
        {
            var idRoleMember = part.IdRoleMember;
            var roles = _roleService.GetRoles().Where(role => !role.Name.Equals("Anonymous") && !role.Name.Equals("Administrator")).ToList();
            var foundRole = _roleService.GetRole(idRoleMember);
            if (foundRole == null)
            {
                part.IdRoleMember = 0;
            }
            var idTaxonomyNotices = part.IdTaxonomyNotices;
            var foundTaxonomy = _taxonomyService.GetTaxonomy(idTaxonomyNotices);
            if (foundTaxonomy == null)
            {
                part.IdTaxonomyNotices = 0;
            }
            var model = new LETSSettingsPartViewModel {
                Roles = roles,
                Taxonomies = _taxonomyService.GetTaxonomies(),
                AdminMembers = _memberService.GetMemberParts(MemberType.Admin),
                LETSSettings = part
            };

            return
                ContentShape("Parts_LETSSettings_Edit",
                             () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix))
                    .OnGroup("LETS");
        }

        [HttpPost]
        protected override DriverResult Editor(LETSSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            var roles = _roleService.GetRoles().Where(role => !role.Name.Equals("Anonymous") && !role.Name.Equals("Administrator")).ToList();
            var model = new LETSSettingsPartViewModel
            {
                Roles = roles,
                Taxonomies = _taxonomyService.GetTaxonomies(),
                AdminMembers = _memberService.GetMemberParts(MemberType.Admin),
                LETSSettings = part
            };

            updater.TryUpdateModel(model, Prefix, null, null);
            _signals.Trigger("letsMemberListChanged");
            _signals.Trigger("letsMemberTurnoversChanged");
            _signals.Trigger("letsTotalTurnoverChanged");

            return
                ContentShape("Parts_LETSSettings_Edit",
                             () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix))
                    .OnGroup("LETS");
        }
    }
}