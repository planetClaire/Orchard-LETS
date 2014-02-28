﻿using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LETS.Helpers;
using LETS.Models;
using LETS.ViewModels;
using NogginBox.MailChimp.Models;
using NogginBox.MailChimp.Services;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.Environment;
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
        private readonly IAddressService _addressService;
        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;
        private readonly Work<ISessionLocator> _sessionLocator;
        private readonly IRepository<TransactionPartRecord> _transactionRepository;

        public MemberService(IRoleService roleService, IOrchardServices orchardServices, IContentManager contentManager, IMailChimpService mailchimpService, IAddressService addressService, ICacheManager cacheManager, ISignals signals, Work<ISessionLocator> sessionLocator, IRepository<TransactionPartRecord> transactionRepository)
        {
            _roleService = roleService;
            _orchardServices = orchardServices;
            _contentManager = contentManager;
            _mailchimpService = mailchimpService;
            _addressService = addressService;
            _cacheManager = cacheManager;
            _signals = signals;
            _sessionLocator = sessionLocator;
            _transactionRepository = transactionRepository;
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


        //public MemberViewModel GetMemberViewModel(MemberPart memberPart, MemberAdminPart memberAdminPart, AddressPart addressPart)
        //{
        //    return new MemberViewModel
        //    {
        //        Id = memberPart.Id,
        //        UserName = memberPart.As<IUser>().UserName,
        //        JoinDate = memberAdminPart.JoinDate != null ? (DateTime)memberAdminPart.JoinDate : DateTime.MinValue,
        //        Balance = GetMemberBalance(memberPart.Id),
        //        Turnover = GetMemberTurnover(memberPart.Id),
        //        Email = memberPart.User.Email,
        //        Fields = memberPart.Fields,
        //        FirstName = memberPart.FirstName,
        //        LastName = memberPart.LastName,
        //        LastFirstName = string.Format("{0}, {1}", memberPart.LastName, memberPart.FirstName),
        //        Telephone = memberPart.Telephone,
        //        Locality = addressPart.Locality != null ? _addressService.GetLocality(addressPart.Locality.Id).Title : null
        //    };
        //}

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
                                                           var session = _sessionLocator.Value.For(null);
                                                           var turnoverDays = _orchardServices.WorkContext.CurrentSite.As<LETSSettingsPart>().DefaultTurnoverDays;
                                                           // direct query for performance sake (todo stored procedure?)
                                                           var query = session.CreateSQLQuery(string.Format(@"
                    SELECT m.Id
                          ,u.Email
                          ,ma.JoinDate
                          ,(SELECT dbo.LETS_CalculateMemberBalance(m.id)) as 'Balance'
                          ,(SELECT dbo.LETS_CalculateMemberTurnover(m.id, {0})) as 'Turnover'
                          ,m.FirstName
                          ,m.LastName
                          ,m.Telephone
                          ,l.Id as 'IdLocality'
                          ,(select top 1 Title from  Title_TitlePartRecord where ContentItemRecord_id = l.Id) as 'Locality'
                      FROM LETS_MemberPartRecord m
                      join Orchard_Users_UserPartRecord u on u.Id = m.Id
                      join LETS_MemberAdminPartRecord ma on ma.Id = m.Id
                      join LETS_AddressPartRecord a on a.Id = m.Id
                      join LETS_LocalityPartRecord l on l.Id = a.LocalityPartRecord_Id
                      where ma.MemberType = '{1}'
                      and u.EmailStatus = 'Approved'
                      and u.RegistrationStatus = 'Approved'
                ", turnoverDays, memberType));
                                                           var list = query.List<object[]>()
                                                               .Select(record => new MemberViewModel {
                                                                   Id = (int) record[0],
                                                                   UserName = (string) record[1],
                                                                   JoinDate = (DateTime) record[2],
                                                                   Balance = (int) record[3],
                                                                   Turnover = (int) record[4],
                                                                   FirstName = (string)record[5],
                                                                   LastName = (string)record[6],
                                                                   LastFirstName = string.Format("{0}, {1}", record[6], record[5]),
                                                                   Telephone = (string) record[7],
                                                                   IdLocality = (int)record[8],
                                                                   Locality = (string)record[9]
                                                               }
                                                               ).ToList();
                                                           return list;
                                                       });
        }

        public IEnumerable<GroupedSelectListItem> GetGroupedMembers(IEnumerable<MemberType> memberTypes) {
            var groupedMembers = new List<GroupedSelectListItem>();
            foreach (var memberType in memberTypes)
            {
                groupedMembers.AddRange(GetMemberParts(memberType).Select(member => new GroupedSelectListItem
                {
                    GroupName = Convert.ToString(memberType),
                    GroupKey = Convert.ToString(memberType),
                    Text = member.LastFirstName,
                    Value = Convert.ToString(member.Id)
                }));
            }
            return groupedMembers;
        }

        public IEnumerable<GroupedSelectListItem> GetGroupedMembers(IEnumerable<MemberType> memberTypes, int idMemberToExclude) {
            var groupedMembers = new List<GroupedSelectListItem>();
            foreach (var memberType in memberTypes) {
                groupedMembers.AddRange(GetMemberParts(memberType, idMemberToExclude).Select(member => new GroupedSelectListItem {
                    GroupName = Convert.ToString(memberType),
                    GroupKey = Convert.ToString(memberType),
                    Text = member.LastFirstName,
                    Value = Convert.ToString(member.Id)
                }));
            }
            return groupedMembers;
        }

        public IEnumerable<dynamic> GetMembersByLocality(int idLocality) {

            return _contentManager.Query<MemberPart>().Join<AddressPartRecord>().Where(a => a.LocalityPartRecord.Id.Equals(idLocality)).List<MemberPart>().Select(member => _contentManager.BuildDisplay(member, "Summary"));
        }
    }

}