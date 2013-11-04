using System;
using System.Web.Mvc;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Mvc.Extensions;
using Orchard.Security;
using Orchard.Themes;
using Orchard.UI.Notify;

namespace Contrib.Profile.Controllers
{
    [ValidateInput(false), Themed]
    public class HomeController : Controller, IUpdateModel
    {
        private readonly IMembershipService _membershipService;

        public HomeController(
            IOrchardServices orchardServices,
            IMembershipService membershipService)
        {

            Services = orchardServices;
            _membershipService = membershipService;
        }

        private IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        private ActionResult RedirectToLogOn()
        {
            return this.RedirectLocal(
                Url.Action("LogOn",
                    new
                    {
                        Controller = "Account",
                        Area = "Orchard.Users",
                        ReturnUrl = Services.WorkContext.HttpContext.Request.RawUrl
                    })
                );
        }

        public ActionResult Index(string username)
        {
            IUser user = Services.WorkContext.CurrentUser;
            IUser profile = _membershipService.GetUser(username);

            if (profile == null) return new HttpNotFoundResult();

            var allowedToViewProfiles = Services.Authorizer.Authorize(Permissions.ViewProfiles, profile, T("Not allowed to view profiles"));

            if (user == null)
            {
                if (!allowedToViewProfiles) return new HttpUnauthorizedResult();
            }
            else
            {
                if (user.UserName == profile.UserName)
                {
                    if (!Services.Authorizer.Authorize(Permissions.ViewOwnProfile, T("Not allowed to view own profile")))
                        return new HttpUnauthorizedResult();
                }
                else
                {
                    if (!allowedToViewProfiles) return new HttpUnauthorizedResult();
                }
            }

            dynamic shape = Services.ContentManager.BuildDisplay(profile.ContentItem);

            return View((object)shape);
        }

        public ActionResult Edit()
        {
            IUser user = Services.WorkContext.CurrentUser;

            if (user == null) return RedirectToLogOn();

            dynamic shape = Services.ContentManager.BuildEditor(user.ContentItem);

            return View((object)shape);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost()
        {
            IUser user = Services.WorkContext.CurrentUser;

            if (user == null) return RedirectToLogOn();

            dynamic shape = Services.ContentManager.UpdateEditor(user.ContentItem, this);
            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();
                return View("Edit", (object)shape);
            }

            Services.Notifier.Information(T("Your profile has been saved."));

            return RedirectToAction("Edit");
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties)
        {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage)
        {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}