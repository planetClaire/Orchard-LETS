using System.Web.Mvc;
using LETS.Models;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.Security;

namespace LETS.Drivers
{
    public class MemberAccessOnlyPartDriver : ContentPartDriver<MemberAccessOnlyPart>
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IAuthenticationService _authenticationService;
        public Localizer T;

        public MemberAccessOnlyPartDriver(IAuthorizationService authorizationService, IAuthenticationService authenticationService) {
            _authorizationService = authorizationService;
            _authenticationService = authenticationService;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(MemberAccessOnlyPart part, string displayType, dynamic shapeHelper)
        {
            if (!_authorizationService.TryCheckAccess(Permissions.AccessMemberContent, _authenticationService.GetAuthenticatedUser(), part))
                throw new OrchardSecurityException(T("This is a member only page"));
            return null;
        }
    }
}