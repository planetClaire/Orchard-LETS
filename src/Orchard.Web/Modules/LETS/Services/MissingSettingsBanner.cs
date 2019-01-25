using System.Collections.Generic;
using System.Web.Mvc;
using Orchard.Taxonomies.Services;
using LETS.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Roles.Services;
using Orchard.UI.Admin.Notification;
using Orchard.UI.Notify;

namespace LETS.Services
{
    public class MissingSettingsBanner : INotificationProvider
    {
        private readonly IOrchardServices _orchardServices;
        private readonly WorkContext _workContext;
        private readonly IRoleService _roleService;
        private readonly ITaxonomyService _taxonomyService;

        public Localizer T { get; set; }

        public MissingSettingsBanner(IOrchardServices orchardServices, IWorkContextAccessor workContextAccessor, IRoleService roleService, ITaxonomyService taxonomyService)
        {
            _orchardServices = orchardServices;
            _roleService = roleService;
            _taxonomyService = taxonomyService;
            _workContext = workContextAccessor.GetContext();
            T = NullLocalizer.Instance;
        }

        public IEnumerable<NotifyEntry> GetNotifications()
        {
            var letsSettings = _orchardServices.WorkContext.CurrentSite.As<LETSSettingsPart>();
            var foundRole = _roleService.GetRole(letsSettings.IdRoleMember);
            var foundTaxonomyNotices = _taxonomyService.GetTaxonomy(letsSettings.IdTaxonomyNotices);
            if (!letsSettings.IsValid() || foundRole == null || foundTaxonomyNotices == null)
            {
                var urlHelper = new UrlHelper(_workContext.HttpContext.Request.RequestContext);
// ReSharper disable Mvc.AreaNotResolved
// ReSharper disable Mvc.ActionNotResolved
                var url = urlHelper.Action("LETS", "Admin", new { area = "Settings" });
// ReSharper restore Mvc.ActionNotResolved
// ReSharper restore Mvc.AreaNotResolved
                yield return new NotifyEntry { Message = T("The <a href=\"{0}\">LETS settings</a> need to be configured.", url), Type = NotifyType.Warning };
            }
        }
    }
}