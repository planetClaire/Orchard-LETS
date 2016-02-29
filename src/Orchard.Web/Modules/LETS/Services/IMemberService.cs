using System.Collections.Generic;
using System.Web.Mvc;
using LETS.Helpers;
using LETS.Models;
using LETS.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Roles.Models;
using Orchard.Users.Models;

namespace LETS.Services
{
    public interface IMemberService : IDependency
    {
        RoleRecord GetMemberRole();
        IEnumerable<dynamic> GetMemberContents(int idUser, string contentTypeName, VersionOptions versionOptions, string displayType);
        IContentQuery<UserPart, UserPartRecord> QueryMembers();
        IEnumerable<MemberPart> GetMemberParts();
        IEnumerable<MemberPart> GetMemberParts(MemberType memberType);
        int GetMemberBalance(int idMember);
        int GetMemberBalance(int idMember, bool bypassCache);
        int GetCreditValueTotal(int idMember);
        TransactionPartRecord GetOldestCreditValueTransaction(int idMember);
        int GetMemberTurnover(int idMember);
        int GetTotalTurnover();
        MemberViewModel GetMemberViewModel(MemberPart memberPart);
        //MemberViewModel GetMemberViewModel(MemberPart memberPart, MemberAdminPart memberAdminPart,AddressPart addressPart);
        MemberPart GetMember(int idMember);
        MemberAdminPart GetMemberAdmin(int idMember);
        Dictionary<string, object> GetMergeVarsForMailChimp(MemberPart member, string idList);
        IEnumerable<MemberViewModel> GetMemberList(MemberType memberType);
        IEnumerable<MemberViewModel> GetDisabledMemberList();
        IEnumerable<SelectListItem> GetGroupedMembers(IEnumerable<MemberType> memberTypes);
        IEnumerable<SelectListItem> GetGroupedMembers(IEnumerable<MemberType> memberTypes, int idMemberToExclude);
        IEnumerable<dynamic> GetMembersByLocality(int idLocality);
    }

}