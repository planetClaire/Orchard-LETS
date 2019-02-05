using LETS.Models;
using LETS.Services;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Roles.Models;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;

namespace LETS.Handlers
{
    public class MemberPartHandler : ContentHandler
    {
        private readonly IRepository<UserRolesPartRecord> _userRolesRespository;
        private readonly IMemberService _memberService;
        private readonly IContentManager _contentManager;

        public MemberPartHandler(IRepository<MemberPartRecord> repository, IRepository<UserRolesPartRecord> userRolesRespository, IMemberService memberService, IContentManager contentManager)
        {
            Filters.Add(StorageFilter.For(repository));

            _userRolesRespository = userRolesRespository;
            _memberService = memberService;
            _contentManager = contentManager;
            OnCreated<MemberPart>((context, part) =>
                                  _userRolesRespository.Create(new UserRolesPartRecord
                                  {
                                      UserId = part.User.Id,
                                      Role = _memberService.GetMemberRole()
                                  }));
            OnLoaded<MemberPart>(LazyLoadHandlers);
        }

        void LazyLoadHandlers(LoadContentContext context, MemberPart part)
        {
            part.LocalityField.Loader(() =>
            {
                return _contentManager.Get(part.As<AddressPart>().Locality.Id).As<TitlePart>().Title;
            });
        }

    }
}
