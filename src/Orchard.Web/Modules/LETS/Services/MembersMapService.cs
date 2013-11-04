using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;
using LETS.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Users.Models;

namespace LETS.Services
{
    public class MembersMapService : IMembersMapService
    {
        private readonly IOrchardServices _orchardServices;
        private readonly IMemberService _memberService;
        public Localizer T { get; set; }

        public MembersMapService(IOrchardServices orchardServices, IMemberService memberService) {
            _orchardServices = orchardServices;
            _memberService = memberService;
            T = NullLocalizer.Instance;
        }

        private IContentQuery<UserPart, UserPartRecord> Users() {
            return _orchardServices.ContentManager.Query<UserPart, UserPartRecord>().Where(u => u.RegistrationStatus == UserStatus.Approved && u.EmailStatus == UserStatus.Approved);
        }

        public IEnumerable<MemberMapMarker> MemberMarkers() {
            var members = _memberService.GetMemberParts(MemberType.Member);
            var userMarkers = new List<MemberMapMarker>();
            var urlHelper = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
            var loggedIn = _orchardServices.WorkContext.CurrentUser != null;
            var html = new StringBuilder("<div class='infowindow'>Please login for details of this member</div>");
            foreach (var member in members) {
                var latLong = (member.As<AddressPart>()).LatLong;
                if (!string.IsNullOrEmpty(latLong)) {
                    if (loggedIn) {
                        html = new StringBuilder();
                        html.AppendFormat("<div class='infowindow'><h4>{0}</h4>", member.FirstLastName);
                        html.AppendFormat("<a href={0}>{1}</a></div>", urlHelper.Action("Index", new {area = "Contrib.Profile", Controller = "Home", username = member.User.UserName}), T("Visit profile"));
                    }
                    userMarkers.Add(new MemberMapMarker {InfoHtml = html.ToString(), LatLong = latLong});
                }
            }
            return userMarkers;
        }
    }
}