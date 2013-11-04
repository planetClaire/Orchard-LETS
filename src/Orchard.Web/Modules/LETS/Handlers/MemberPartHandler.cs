using JetBrains.Annotations;
using LETS.Models;
using LETS.Services;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Roles.Models;

namespace LETS.Handlers
{
    [UsedImplicitly]
    public class MemberPartHandler : ContentHandler
    {
        private readonly IRepository<UserRolesPartRecord> _userRolesRespository;
        private readonly IMemberService _memberService;

        public MemberPartHandler(IRepository<MemberPartRecord> repository, IRepository<UserRolesPartRecord> userRolesRespository, IMemberService memberService)
        {
            Filters.Add(StorageFilter.For(repository));

            _userRolesRespository = userRolesRespository;
            _memberService = memberService;
            OnCreated<MemberPart>((context, part) =>
                                  _userRolesRespository.Create(new UserRolesPartRecord
                                                                   {
                                                                       UserId = part.User.Id,
                                                                       Role = _memberService.GetMemberRole()
                                                                   }));
        }
    }
}