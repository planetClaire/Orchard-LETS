using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using JetBrains.Annotations;
using LETS.Models;
using LETS.ViewModels;
using NogginBox.MailChimp.Models;
using NogginBox.MailChimp.Services;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using Orchard.Data;
using Orchard.Environment;
using Orchard.Logging;
using Orchard.Roles.Models;
using Orchard.Roles.Services;
using Orchard.Security;
using Orchard.Users.Models;

namespace LETS.Services
{
    [UsedImplicitly]
    public class MemberService : IMemberService
    {
        private readonly IRoleService _roleService;
        private readonly IOrchardServices _orchardServices;
        private readonly IContentManager _contentManager;
        private readonly IMailChimpService _mailchimpService;
        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;
        private readonly Work<ISessionLocator> _sessionLocator;
        private readonly IRepository<TransactionPartRecord> _transactionRepository;

        public ILogger Logger { get; set; }

        public MemberService(IRoleService roleService, IOrchardServices orchardServices, IContentManager contentManager, IMailChimpService mailchimpService, ICacheManager cacheManager, ISignals signals, Work<ISessionLocator> sessionLocator, IRepository<TransactionPartRecord> transactionRepository)
        {
            _roleService = roleService;
            _orchardServices = orchardServices;
            _contentManager = contentManager;
            _mailchimpService = mailchimpService;
            _cacheManager = cacheManager;
            _signals = signals;
            _sessionLocator = sessionLocator;
            _transactionRepository = transactionRepository;
            Logger = NullLogger.Instance;
        }

        public RoleRecord GetMemberRole()
        {
            return _roleService.GetRole(_orchardServices.WorkContext.CurrentSite.As<LETSSettingsPart>().IdRoleMember);
        }


        public IContentQuery<UserPart, UserPartRecord> QueryMembers() {
            return _orchardServices.ContentManager.Query<UserPart, UserPartRecord>().Where(u => u.RegistrationStatus == UserStatus.Approved && u.EmailStatus == UserStatus.Approved);
        }

        public IEnumerable<MemberPart> GetMemberParts()
        {
            return QueryMembers().Join<MemberPartRecord>().OrderBy(m => m.LastName).List<MemberPart>();
        }

        public IEnumerable<MemberPart> GetMemberParts(MemberType memberType, int idMemberToExclude) {
            return _cacheManager.Get(string.Format("letsMemberPartsType{0}Excluding{1}", memberType, idMemberToExclude), ctx => {
                ctx.Monitor(_signals.When(string.Format("letsMemberPartsType{0}Changed", memberType)));
                return _orchardServices.ContentManager.Query<UserPart, UserPartRecord>().
                    Where(u => u.RegistrationStatus == UserStatus.Approved && u.EmailStatus == UserStatus.Approved).
                    Where<MemberAdminPartRecord>(m => !m.Id.Equals(idMemberToExclude) && m.MemberType == memberType).
                    Join<MemberPartRecord>().
                    OrderBy(m => m.LastName).
                    List<MemberPart>();
            });
        }

        public IEnumerable<MemberPart> GetMemberParts(MemberType memberType) {
            return
                _orchardServices.ContentManager.Query<UserPart, UserPartRecord>().Where(
                    m => m.RegistrationStatus == UserStatus.Approved && m.EmailStatus == UserStatus.Approved).
                    Join<MemberAdminPartRecord>().Where(m => m.MemberType == memberType).Join<MemberPartRecord>().OrderBy(m => m.LastName).List<MemberPart>();
        }

        public int GetMemberBalance(int idMember, bool bypassCache)
        {
            return bypassCache ? QueryMemberBalance(idMember) : GetMemberBalance(idMember);
        }

        public int GetMemberBalance(int idMember)
        {
            return _cacheManager.Get(string.Format("letsMemberBal{0}", idMember), ctx =>
                {
                    ctx.Monitor(_signals.When(string.Format("letsMemberBal{0}Changed", idMember)));
                    return QueryMemberBalance(idMember);
                });

        }

        private int QueryMemberBalance(int idMember)
        {
            var session = _sessionLocator.Value.For(null);
            // direct query for performance sake
            var query = session.CreateSQLQuery(string.Format(@"SELECT dbo.LETS_CalculateMemberBalance({0})", idMember));
            var result = query.List<int>();
            return result[0];
        }

        public int GetCreditValueTotal(int idMember)
        {
            return
                _orchardServices.ContentManager.Query<TransactionPart, TransactionPartRecord>().Where(
                    t => t.CreditValue > 0 && t.SellerMemberPartRecord.Id.Equals(idMember)).List().Sum(t => t.CreditValue);
        }

        public TransactionPartRecord GetOldestCreditValueTransaction(int idMember) {
                var session = _sessionLocator.Value.For(null);
                // direct query for performance sake
                var query = session.CreateSQLQuery(string.Format(@"SELECT dbo.LETS_GetOldestCreditValueTransaction({0})", idMember));
                var result = query.List();
                return result.Count > 0 && result[0] != null ? _transactionRepository.Get((int)result[0]) : null;
        }

        public int GetMemberTurnover(int idMember)
        {
            return _cacheManager.Get(string.Format("letsMemberTurnover{0}", idMember), ctx =>
            {
                ctx.Monitor(_signals.When(string.Format("letsMemberTurnover{0}Changed", idMember)));
                ctx.Monitor(_signals.When(string.Format("letsMemberTurnoversChanged")));
                var session = _sessionLocator.Value.For(null);
                // direct query for performance sake
                var turnoverDays = _orchardServices.WorkContext.CurrentSite.As<LETSSettingsPart>().DefaultTurnoverDays;
                var query = session.CreateSQLQuery(string.Format(@"SELECT dbo.LETS_CalculateMemberTurnover({0}, {1})", idMember, turnoverDays));
                var result = query.List<int>();
                return result[0];
            });
        }

        public int GetTotalTurnover()
        {
            return _cacheManager.Get(string.Format("letsTotalTurnover"), ctx =>
            {
                ctx.Monitor(_signals.When("letsTotalTurnoverChanged"));
                var session = _sessionLocator.Value.For(null);
                // direct query for performance sake
                var turnoverDays = _orchardServices.WorkContext.CurrentSite.As<LETSSettingsPart>().DefaultTurnoverDays;
                var query = session.CreateSQLQuery(string.Format(@"SELECT dbo.LETS_CalculateTotalTurnover({0})", turnoverDays));
                var result = query.List<int>();
                return result[0];
            });
        }

        public MemberViewModel GetMemberViewModel(MemberPart memberPart)
        {
            return new MemberViewModel
            {
                Id = memberPart.Id,
                UserName = memberPart.As<IUser>().UserName,
                Balance = GetMemberBalance(memberPart.Id),
                Turnover = GetMemberTurnover(memberPart.Id),
                Email = memberPart.User.Email,
                Fields = memberPart.Fields,
                FirstName = memberPart.FirstName,
                LastName = memberPart.LastName,
                LastFirstName = string.Format("{0}, {1}", memberPart.LastName, memberPart.FirstName),
                Telephone = memberPart.Telephone
            };
        }


        public MemberPart GetMember(int idMember)
        {
            return _contentManager.Get<MemberPart>(idMember);
        }

        public MemberAdminPart GetMemberAdmin(int idMember)
        {
            return _contentManager.Get<MemberAdminPart>(idMember);
        }

        public IEnumerable<dynamic> GetMemberContents(int idUser, string contentTypeName, VersionOptions versionOptions, string displayType)
        {
            var contentQuery = _contentManager.Query(versionOptions, new[] { contentTypeName }).
                Where<CommonPartRecord>(c => c.OwnerId.Equals(idUser)).
                OrderByDescending<CommonPartRecord>(c => c.PublishedUtc).List();

            return contentQuery.Select(contentItem => _contentManager.BuildDisplay(contentItem, displayType));
        }

        public Dictionary<string, object> GetMergeVarsForMailChimp(MemberPart member, string idList)
        {
            var mergeVars =
                (List<MergeVariableRecord>)
                _mailchimpService.getMergeVariablesFromApi(idList);
            var mergeVarsForMailChimp = new Dictionary<String, object>();
            var firstNameMergeVar = mergeVars.Find(m => m.Tag.Equals("FNAME"));
            if (firstNameMergeVar != null)
            {
                mergeVarsForMailChimp.Add(firstNameMergeVar.Tag, member.FirstName);
            }
            var lastNameMergeVar = mergeVars.Find(m => m.Tag.Equals("LNAME"));
            if (lastNameMergeVar != null)
            {
                mergeVarsForMailChimp.Add(lastNameMergeVar.Tag, member.LastName);
            }
            return mergeVarsForMailChimp;
        }

        public IEnumerable<MemberViewModel> GetMemberList(MemberType memberType) {
            return _cacheManager.Get(string.Format("letsMemberList{0}", memberType), ctx => {
                ctx.Monitor(_signals.When("letsMemberListChanged"));
                var activeMembers = _contentManager
                    .Query<UserPart, UserPartRecord>("User").Where(u => u.EmailStatus == UserStatus.Approved && u.RegistrationStatus == UserStatus.Approved)
                    .Join<MemberAdminPartRecord>().Where(m => m.MemberType == memberType)
                    .List();
                var memberViewModels = new List<MemberViewModel>();
                // do not convert this to a LINQ expression, it will cause lifetime exceptions in Orchard (because of the cache lambda)
                foreach (var user in activeMembers) {
                    var memberPart = user.As<MemberPart>();
                    var memberAdminPart = user.As<MemberAdminPart>();
                    var addressPart = user.As<AddressPart>();
                    ContentItem locality = null;
                    if (addressPart.Locality != null)
                    {
                        locality = _contentManager.Get(addressPart.Locality.Id);
                    }
                    else
                    {
                        Logger.Error("A member {0} has no locality", memberPart.Id);
                    }
                    AddMemberViewModel(memberViewModels, user, memberAdminPart, memberPart, addressPart, locality);
                }
                return memberViewModels;
            });
        }

        public IEnumerable<MemberViewModel> GetDisabledMemberList() {
            return _cacheManager.Get(string.Format("letsDisabledMemberList"), ctx => {
                ctx.Monitor(_signals.When("letsDisabledMemberListChanged"));
                var disabledMembers = _contentManager
                    .Query<UserPart, UserPartRecord>("User").Where(u => u.RegistrationStatus == UserStatus.Pending)
                    //.Join<MemberAdminPartRecord>().Where(m => m.MemberType == memberType)
                    .List();
                var memberViewModels = new List<MemberViewModel>();
                // do not convert this to a LINQ expression, it will cause lifetime exceptions in Orchard (because of the cache lambda)
                foreach (var user in disabledMembers)
                {
                    var memberPart = user.As<MemberPart>();
                    var memberAdminPart = user.As<MemberAdminPart>();
                    var addressPart = user.As<AddressPart>();
                    var locality = _contentManager.Get(addressPart.Locality.Id);
                    AddMemberViewModel(memberViewModels, user, memberAdminPart, memberPart, addressPart, locality);
                }
                return memberViewModels;
            });
        }

        private void AddMemberViewModel(List<MemberViewModel> memberViewModels, UserPart user, MemberAdminPart memberAdminPart, MemberPart memberPart, AddressPart addressPart, ContentItem locality) {
            memberViewModels.Add(new MemberViewModel {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                EmailStatus = user.EmailStatus,
                JoinDate = memberAdminPart.JoinDate.HasValue ? memberAdminPart.JoinDate.Value : DateTime.MinValue,
                Balance = GetMemberBalance(user.Id),
                Turnover = GetMemberTurnover(user.Id),
                FirstName = memberPart.FirstName,
                LastName = memberPart.LastName,
                LastFirstName = memberPart.LastFirstName,
                Telephone = memberPart.Telephone,
                IdLocality = locality != null ? addressPart.Locality.Id : 0,
                Locality = locality != null ? locality.As<TitlePart>().Title : "na"
            });
        }

        IEnumerable<SelectListItem> IMemberService.GetGroupedMembers(IEnumerable<MemberType> memberTypes) {
            var groupedMembers = new List<SelectListItem>();
            foreach (var memberType in memberTypes) {
                PopulateGroupedMembers(memberType, groupedMembers, GetMemberParts(memberType));
            }
            return groupedMembers;
        }

        IEnumerable<SelectListItem> IMemberService.GetGroupedMembers(IEnumerable<MemberType> memberTypes, int idMemberToExclude) {
            var groupedMembers = new List<SelectListItem>();
            foreach (var memberType in memberTypes)
            {
                PopulateGroupedMembers(memberType, groupedMembers, GetMemberParts(memberType, idMemberToExclude));
            }
            return groupedMembers;
        }

        private static void PopulateGroupedMembers(MemberType memberType, List<SelectListItem> groupedMembers, IEnumerable<MemberPart> memberParts)
        {
            var group = new SelectListGroup { Name = Convert.ToString(memberType) };
            groupedMembers.AddRange(memberParts.Select(member => new SelectListItem
            {
                Text = member.LastFirstName,
                Value = Convert.ToString(member.Id),
                Group = @group
            }));
        }

        public IEnumerable<dynamic> GetMembersByLocality(int idLocality)
        {

            return _contentManager.Query<MemberPart>().Join<AddressPartRecord>().Where(a => a.LocalityPartRecord.Id.Equals(idLocality)).List<MemberPart>().Select(member => _contentManager.BuildDisplay(member, "Summary"));
        }
    }

}