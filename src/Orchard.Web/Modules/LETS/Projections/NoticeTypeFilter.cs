using System;
using System.Linq;
using LETS.Models;
using LETS.Services;
using Orchard.ContentManagement;
using Orchard.Events;
using Orchard.Localization;

namespace LETS.Projections
{
    public interface IFilterProvider : IEventHandler
    {
        void Describe(dynamic describe);
    }

    public class NoticeTypeFilter : IFilterProvider
    {
        private readonly INoticeService _noticeService;

        public NoticeTypeFilter(INoticeService noticeService)
        {
            _noticeService = noticeService;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(dynamic describe)
        {
            describe.For("NoticeType", T("Notice Type"), T("Notice Type"))
                .Element("IsNoticeType", T("Is Notice Type"), T("Notices"),
                    (Action<dynamic>)ApplyFilter,
                    (Func<dynamic, LocalizedString>)DisplayFilter,
                    "SelectNoticeTypes"
                );
        }

        public void ApplyFilter(dynamic context)
        {
            string noticeTypeIds = Convert.ToString(context.State.NoticeTypeIds);
            if (!String.IsNullOrEmpty(noticeTypeIds))
            {
                var ids = noticeTypeIds.Split(new[] { ',' }).Select(Int32.Parse).ToArray();
                if (ids.Length == 0)
                {
                    return;
                }
                var query = (IHqlQuery)context.Query;
                context.Query = query.Where(x => x.ContentPartRecord<NoticePartRecord>().Property("NoticeTypePartRecord", "noticeTypePartRecord"), x => x.In("Id", ids));
            }
        }

        public LocalizedString DisplayFilter(dynamic context)
        {
            string noticeTypeIds = Convert.ToString(context.State.NoticeTypeIds);

            if (String.IsNullOrEmpty(noticeTypeIds))
            {
                return T("Any Notice Type");
            }

            var noticeTypeNames = noticeTypeIds.Split(new[] { ',' }).Select(x => _noticeService.GetNoticeTypeTitle(Int32.Parse(x)));

            return T("Is {0}", String.Join(", ", noticeTypeNames));
        }
    }
}