using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Orchard.Taxonomies.Helpers;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using JetBrains.Annotations;
using LETS.Helpers;
using LETS.Models;
using LETS.ViewModels;
using Orchard;
using Orchard.ArchiveLater.Services;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.Environment;
using Orchard.Localization;
using Orchard.UI.Notify;

namespace LETS.Services
{
    [UsedImplicitly]
    public class NoticeService : INoticeService
    {
        private readonly IRepository<NoticeTypePartRecord> _noticeTypeRepository;
        private readonly IContentManager _contentManager;
        private readonly INotifier _notifier;
        public Localizer T;
        private readonly ITaxonomyService _taxonomyService;
        private readonly IOrchardServices _orchardServices;
        private readonly IArchiveLaterService _archiveLaterService;
        private readonly Work<ISessionLocator> _sessionLocator;
        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;
        private readonly IMemberService _memberService;

        public NoticeService(IRepository<NoticeTypePartRecord> noticeTypeRepository, IContentManager contentManager, INotifier notifier, ITaxonomyService taxonomyService, IOrchardServices orchardServices, IArchiveLaterService archiveLaterService, Work<ISessionLocator> sessionLocator, ICacheManager cacheManager, ISignals signals, IMemberService memberService)
        {
            _noticeTypeRepository = noticeTypeRepository;
            _contentManager = contentManager;
            _notifier = notifier;
            _taxonomyService = taxonomyService;
            _orchardServices = orchardServices;
            _archiveLaterService = archiveLaterService;
            _sessionLocator = sessionLocator;
            _cacheManager = cacheManager;
            _signals = signals;
            _memberService = memberService;
            T = NullLocalizer.Instance;
        }

        public NoticePart GetNotice(int idNotice)
        {
            return _contentManager.Get<NoticePart>(idNotice);
        }

        public IEnumerable<NoticeTypePart> GetNoticeTypes()
        {
            return _contentManager.Query<NoticeTypePart, NoticeTypePartRecord>(VersionOptions.Published).OrderBy(nt => nt.SortOrder).List();
        }

        public void UpdateNoticeForContentItem(ContentItem contentItem, EditNoticeViewModel model)
        {
            var noticePart = contentItem.As<NoticePart>();
            noticePart.Price = model.Price;
            noticePart.NoticeType = _noticeTypeRepository.Get(nt => nt.Id == model.IdNoticeType);
            var idUser = contentItem.As<CommonPart>().Owner.Id;
            _signals.Trigger(string.Format("letsMemberNoticesChanged{0}", idUser));
            _signals.Trigger(string.Format("letsMemberArchivedNoticesChanged{0}", idUser));
            _signals.Trigger(string.Format("letsNoticesByLocalityChanged{0}", _memberService.GetMember(idUser).As<AddressPart>().Locality.Id));
        }

        public string GetNoticeTypeTitle(int idNoticeType)
        {
            var contentItem = _contentManager.Get(idNoticeType, VersionOptions.Latest);
            if (contentItem.Has<NoticeTypePart>())
            {
                var noticeTypeTitle = contentItem.As<NoticeTypePart>().Title;
                if (noticeTypeTitle != null) return noticeTypeTitle;
            }
            if (contentItem.Has<NoticePart>())
            {
                var noticeTypePartRecord = contentItem.As<NoticePart>().NoticeType;
                if (noticeTypePartRecord != null)
                    return GetNoticeTypeTitle(noticeTypePartRecord.Id);
            }
            return string.Empty;
        }

        public IEnumerable<NoticeTypePart> GetRequiredNoticeTypes()
        {
            return _contentManager.Query<NoticeTypePart, NoticeTypePartRecord>(VersionOptions.Published).Where(nt => nt.RequiredCount > 0).List();
        }

        public IContentQuery<NoticePart, NoticePartRecord> GetNotices() {
            return _contentManager.Query<NoticePart, NoticePartRecord>(VersionOptions.Published);
        }

        public IContentQuery<NoticePart> GetNotices(int idNoticeType)
        {
            return
                _contentManager.Query<NoticePart, NoticePartRecord>(VersionOptions.Published).Where(n => n.NoticeTypePartRecord.Id.Equals(idNoticeType));
        }

        public IEnumerable<dynamic> GetNoticesByLocality(int idLocality) {
            var list = _cacheManager.Get(string.Format("letsNoticesByLocality{0}", idLocality), ctx => {
                ctx.Monitor(_signals.When(string.Format("letsNoticesByLocalityChanged{0}", idLocality)));
                var session = _sessionLocator.Value.For(null);
                var query = session.CreateSQLQuery(string.Format(@"
                SELECT n.Id
                  FROM LETS_NoticePartRecord n
                join Common_CommonPartRecord c on c.Id = n.Id
                join LETS_AddressPartRecord a on a.Id = c.OwnerId
                and LocalityPartRecord_Id = {0}
                order by c.PublishedUtc desc
                ", idLocality));
                return query.List<int>();
            });
            return _contentManager.GetMany<NoticePart>(list, VersionOptions.Published, QueryHints.Empty).Select(contentItem => _contentManager.BuildDisplay(contentItem, "DetailedSummary")); 
        }

        public IEnumerable<GroupedSelectListItem> GetCategoryTerms()
        {
            var terms = _taxonomyService.GetTerms(_orchardServices.WorkContext.CurrentSite.As<LETSSettingsPart>().IdTaxonomyNotices);
            var topLevelTerms = terms.Where(termPart => termPart.GetLevels().Equals(0));
            var groupedTerms = new List<GroupedSelectListItem>();
            foreach (var topLevelTerm in topLevelTerms)
            {
                groupedTerms.AddRange(_taxonomyService.GetChildren(topLevelTerm).Select(term => new GroupedSelectListItem
                {
                    GroupName = topLevelTerm.Name,
                    GroupKey = topLevelTerm.Id.ToString(CultureInfo.InvariantCulture),
                    Text = term.Name,
                    Value = term.Id.ToString(CultureInfo.InvariantCulture)
                }));
            }
            return groupedTerms;
        }

        public IEnumerable<TermPart> GetTopLevelCategories()
        {
            return _taxonomyService.GetTerms(
                    _orchardServices.WorkContext.CurrentSite.As<LETSSettingsPart>().IdTaxonomyNotices).Where(t => t.GetLevels().Equals(0));
        }

        public int GetNoticeCount(int idNoticeType, int idCategory)
        {
            var term = _taxonomyService.GetTerm(idCategory);
            return _taxonomyService.GetContentItems(term).Count(c => c.Is<NoticePart>() && c.As<NoticePart>().NoticeType.Id.Equals(idNoticeType));
        }

        private IEnumerable<NoticePart> GetMemberNotices(int idUser)
        {
            return _cacheManager.Get(string.Format("letsMemberNotices{0}", idUser), ctx =>
            {
                ctx.Monitor(_signals.When(string.Format("letsMemberNoticesChanged{0}", idUser)));
                return _contentManager.Query<NoticePart>(VersionOptions.Published).Where<CommonPartRecord>(c => c.OwnerId.Equals(idUser)).OrderByDescending(c => c.PublishedUtc).List();
            });
        }

        private IEnumerable<NoticePart> GetMemberArchivedNotices(int idUser)
        {
            return _cacheManager.Get(string.Format("letsMemberArchivedNotices{0}", idUser), ctx =>
            {
                ctx.Monitor(_signals.When(string.Format("letsMemberArchivedNoticesChanged{0}", idUser)));
                return _contentManager.Query<NoticePart>(VersionOptions.Draft).Where<CommonPartRecord>(c => c.OwnerId.Equals(idUser)).OrderByDescending(c => c.ModifiedUtc).List();
            });
        }

        public IEnumerable<dynamic> GetMemberNoticeShapes(int idUser, string displayType = "DetailedSummary")
        {
            return GetMemberNotices(idUser).Select(contentItem => _contentManager.BuildDisplay(contentItem, displayType));
        }

        public IEnumerable<dynamic> GetMemberArchivedNoticeShapes(int idUser, string displayType = "DetailedSummary")
        {
            return GetMemberArchivedNotices(idUser).Select(contentItem => _contentManager.BuildDisplay(contentItem, displayType));
        }

        public IEnumerable<dynamic> GetNoticeShapesByType(int id, int page, int pageSize)
        {
            return _contentManager.Query<NoticePart, NoticePartRecord>(VersionOptions.Published).Where(n => n.NoticeTypePartRecord.Id.Equals(id)).OrderByDescending<CommonPartRecord>(c => c.PublishedUtc).Slice(pageSize * (page - 1), pageSize).Select(n => _contentManager.BuildDisplay(n.ContentItem, "Summary"));
        }

        public int GetNoticeCountByType(int idNoticeType) {
            return _contentManager.Query<NoticePart, NoticePartRecord>(VersionOptions.Published).Where(n => n.NoticeTypePartRecord.Id.Equals(idNoticeType)).OrderByDescending<CommonPartRecord>(c => c.PublishedUtc).Count();
        }

        public void DeleteMemberNotices(int idUser)
        {
            var memberNotices = GetMemberNotices(idUser);
            foreach (var notice in memberNotices)
            {
                _contentManager.Remove(notice.ContentItem);
                _notifier.Information(T("Notice \"{0}\" deleted", notice.Title));
            }
            _signals.Trigger(string.Format("letsMemberNoticesChanged{0}", idUser));
            _signals.Trigger(string.Format("letsMemberArchivedNoticesChanged{0}", idUser));
            _signals.Trigger(string.Format("letsNoticesByLocalityChanged{0}", _memberService.GetMember(idUser).As<AddressPart>().Locality.Id));
        }

        public void PublishMemberNotices(int idUser)
        {
            var memberNotices = GetMemberNotices(idUser, VersionOptions.Draft);
            foreach (var notice in memberNotices)
            {
                _archiveLaterService.ArchiveLater(notice.ContentItem,DateTime.Now.AddDays(
                                         _orchardServices.WorkContext.CurrentSite.As<LETSSettingsPart>()
                                             .MaximumNoticeAgeDays));
                _contentManager.Publish(notice.ContentItem);
            }
            _signals.Trigger(string.Format("letsMemberNoticesChanged{0}", idUser));
            _signals.Trigger(string.Format("letsMemberArchivedNoticesChanged{0}", idUser));
            _signals.Trigger(string.Format("letsNoticesByLocalityChanged{0}", _memberService.GetMember(idUser).As<AddressPart>().Locality.Id));
        }

        public IEnumerable<NoticePart> GetMemberNotices(int idUser, int idNoticeType)
        {
            return _contentManager.Query<NoticePart>(VersionOptions.Published).
                Where<NoticePartRecord>(n => n.NoticeTypePartRecord.Id.Equals(idNoticeType)).
                Join<CommonPartRecord>().Where(c => c.OwnerId.Equals(idUser)).
                List<NoticePart>();
        }

        public NoticeTypePartRecord GetNoticeType(int? id)
        {
            return id != null ? _noticeTypeRepository.Get((int) id) : null;
        }

        private IEnumerable<NoticePart> GetMemberNotices(int idUser, VersionOptions versionOptions)
        {
            return
               _contentManager.Query<NoticePart>(versionOptions).Where<CommonPartRecord>(c => c.OwnerId.Equals(idUser)).List();
        }
    }
}