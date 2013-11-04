using System.Linq;
using System.Web.Mvc;
using LETS.Models;
using LETS.Services;
using LETS.ViewModels;
using Orchard;
using Orchard.Localization;
using Orchard.Themes;

namespace LETS.Controllers
{
    public class MemberController : Controller
    {
        private readonly IOrchardServices _orchardServices;
        private readonly IMemberService _memberService;

        public Localizer T { get; set; }

        public MemberController(IOrchardServices orchardServices, IMemberService memberService)
        {
            _orchardServices = orchardServices;
            _memberService = memberService;

            T = NullLocalizer.Instance;
        }

        [Themed]
        public ActionResult Index(string orderby = "name")
        {
            if (!_orchardServices.Authorizer.Authorize(Permissions.AccessMemberContent))
                return new HttpUnauthorizedResult();

            var members = _memberService.GetMemberList(MemberType.Member);
            var adminMembers = _memberService.GetMemberList(MemberType.Admin);
            var letssystemMembers = _memberService.GetMemberList(MemberType.LETSystem);
            switch (orderby)
            {
                case "name":
                    members = members.OrderBy(m => m.LastName).ThenBy(m => m.FirstName);
                    adminMembers = adminMembers.OrderBy(m => m.LastName).ThenBy(m => m.FirstName);
                    letssystemMembers = letssystemMembers.OrderBy(m => m.LastName).ThenBy(m => m.FirstName);
                    break;
                case "nameDesc":
                    members = members.OrderByDescending(m => m.LastName).ThenBy(m => m.FirstName);
                    adminMembers = adminMembers.OrderByDescending(m => m.LastName).ThenBy(m => m.FirstName);
                    letssystemMembers = letssystemMembers.OrderByDescending(m => m.LastName).ThenBy(m => m.FirstName);
                    break;
                case "balance":
                    members = members.OrderBy(m => m.Balance);
                    adminMembers = adminMembers.OrderBy(m => m.Balance);
                    letssystemMembers = letssystemMembers.OrderBy(m => m.Balance);
                    break;
                case "balanceDesc":
                    members = members.OrderByDescending(m => m.Balance);
                    adminMembers = adminMembers.OrderByDescending(m => m.Balance);
                    letssystemMembers = letssystemMembers.OrderByDescending(m => m.Balance);
                    break;
                case "turnover":
                    members = members.OrderBy(m => m.Turnover);
                    adminMembers = adminMembers.OrderBy(m => m.Turnover);
                    letssystemMembers = letssystemMembers.OrderBy(m => m.Turnover);
                    break;
                case "turnoverDesc":
                    members = members.OrderByDescending(m => m.Turnover);
                    adminMembers = adminMembers.OrderByDescending(m => m.Turnover);
                    letssystemMembers = letssystemMembers.OrderByDescending(m => m.Turnover);
                    break;
                case "joined":
                    members = members.OrderBy(m => m.JoinDate);
                    adminMembers = adminMembers.OrderBy(m => m.JoinDate);
                    letssystemMembers = letssystemMembers.OrderBy(m => m.JoinDate);
                    break;
                case "joinedDesc":
                    members = members.OrderByDescending(m => m.JoinDate);
                    adminMembers = adminMembers.OrderByDescending(m => m.JoinDate);
                    letssystemMembers = letssystemMembers.OrderByDescending(m => m.JoinDate);
                    break;
                case "location":
                    members = members.OrderBy(m => m.Locality);
                    adminMembers = adminMembers.OrderBy(m => m.Locality);
                    letssystemMembers = letssystemMembers.OrderBy(m => m.Locality);
                    break;
                case "locationDesc":
                    members = members.OrderByDescending(m => m.Locality);
                    adminMembers = adminMembers.OrderByDescending(m => m.Locality);
                    letssystemMembers = letssystemMembers.OrderByDescending(m => m.Locality);
                    break;
            }

            var memberList = new MemberListViewModel {Members = members, MemberType = @T("Members").ToString()};
            var adminList = new MemberListViewModel { Members = adminMembers, MemberType = @T("Admin Accounts").ToString() };
            var letsSystemList = new MemberListViewModel { Members = letssystemMembers, MemberType = @T("Other LETS Systems").ToString() };

            var model = new MemberListsViewModel();
            model.MemberLists.Add(memberList);
            model.MemberLists.Add(adminList);
            model.MemberLists.Add(letsSystemList);
            return View(model);
        }

    }
}