using JetBrains.Annotations;
using LETS.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment;
using Orchard.Environment.Extensions.Models;
using Orchard.Roles.Services;

namespace LETS
{
    [UsedImplicitly]
    public class FeatureEvents : IFeatureEventHandler
    {
        private readonly IOrchardServices _orchardServices;
        private readonly IRoleService _roleService;

        public FeatureEvents(IOrchardServices orchardServices, IRoleService roleService)
        {
            _orchardServices = orchardServices;
            _roleService = roleService;
        }

        public void Installing(Feature feature)
        {
        }

        public void Installed(Feature feature)
        {
        }

        public void Enabling(Feature feature)
        {
            if (feature.Descriptor.Id.Equals("LETS"))
            {
                // set member role
                var letsSettings = _orchardServices.WorkContext.CurrentSite.As<LETSSettingsPart>();
                var roleMember = _roleService.GetRoleByName("Member");
                if (roleMember != null)
                {
                    letsSettings.IdRoleMember = roleMember.Id;
                }
            }
        }

        public void Enabled(Feature feature)
        {
        }

        public void Disabling(Feature feature)
        {
        }

        public void Disabled(Feature feature)
        {
        }

        public void Uninstalling(Feature feature)
        {
        }

        public void Uninstalled(Feature feature)
        {
        }
    }
}