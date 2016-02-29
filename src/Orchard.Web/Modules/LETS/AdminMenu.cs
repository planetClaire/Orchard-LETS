using System.Web.Routing;
using LETS.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment;
using Orchard.Localization;
using Orchard.UI.Navigation;

namespace LETS
{
    public class AdminMenu : INavigationProvider
    {
        private readonly Work<RequestContext> _requestContextAccessor;
        private readonly IOrchardServices _orchardServices;

        public string MenuName
        {
            get { return "admin"; }
        }

        public AdminMenu(Work<RequestContext> requestContextAccessor, IOrchardServices orchardServices)
        {
            _requestContextAccessor = requestContextAccessor;
            _orchardServices = orchardServices;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void GetNavigation(NavigationBuilder builder)
        {

            builder.AddImageSet("LETS").Add(T("LETS"), "1.4", BuildMenu);
        }

        private void BuildMenu(NavigationItemBuilder builder)
        {
            var requestContext = _requestContextAccessor.Value;
            var idValue = (string)requestContext.RouteData.Values["id"];
            var idMember = 0;

            if (!string.IsNullOrEmpty(idValue))
            {
                int.TryParse(idValue, out idMember);
            }

            builder.Add(T("Members"), "1.0", menu =>
                    menu.LinkToFirstChild(false)
                    .Action("Index", "MemberAdmin", new { area = "LETS" }).Permission(Permissions.AdminMemberContent)
                    .Add(T("Transactions"), "2.1", item => item.Action("List", "TransactionsAdmin", new {area = "LETS", id = idMember }).LocalNav().Permission(Permissions.AdminMemberContent))
                    .Add(T("Notices"), "2.2", item => item.Action("List", "NoticesAdmin", new { area = "LETS", id = idMember }).LocalNav().Permission(Permissions.AdminMemberContent))
                    );

            builder.Add(T("Mailchimp"), "1.1", menu =>
                     menu.Action("Mailchimp", "MemberAdmin", new { area = "LETS" }).Permission(Permissions.AdminMemberContent)
                     );

            builder.Add(T("Alerts"), "1.2", menu =>
                     menu.Action("Alerts", "MemberAdmin", new { area = "LETS" }).Permission(Permissions.AdminMemberContent)
                     );
            if (_orchardServices.WorkContext.CurrentSite.As<LETSSettingsPart>().UseDemurrage) {
             
                builder.Add(T("Demurrage"), "1.3", menu =>
                    menu.Action("Demurrage", "TransactionsAdmin", new {area = "LETS"}).Permission(Permissions.AdminMemberContent)
                    );

                builder.Add(T("Demurrage Forecast"), "1.3", menu =>
                    menu.Action("DemurrageForecast", "TransactionsAdmin", new {area = "LETS"}).Permission(Permissions.AdminMemberContent)
                    );
            }

        }

    }
}
