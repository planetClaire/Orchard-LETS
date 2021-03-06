﻿using LETS.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Title.Models;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Security;

namespace LETS.Handlers
{
    public class NoticePartHandler : ContentHandler
    {
        private readonly IOrchardServices _orchardServices;
        public Localizer T { get; set; }

        public NoticePartHandler(IRepository<NoticePartRecord> repository, IOrchardServices orchardServices)
        {
            Filters.Add(StorageFilter.For(repository));

            _orchardServices = orchardServices;
            T = NullLocalizer.Instance;
            OnGetDisplayShape<NoticePart>((context, part) =>
            {
                if (context.DisplayType.StartsWith("Detail") && !_orchardServices.Authorizer.Authorize(Permissions.AccessMemberContent))
                {
                    throw new OrchardSecurityException(T("attempt to access member content"));
                }
            });
            OnLoaded<NoticePart>(LazyLoadHandlers);
        }

        void LazyLoadHandlers(LoadContentContext context, NoticePart part)
        {
            part.StrNoticeTypeField.Loader(() =>
            {
                return _orchardServices.ContentManager.Get(part.NoticeType.Id).As<TitlePart>().Title;
            });
        }
    }
}
