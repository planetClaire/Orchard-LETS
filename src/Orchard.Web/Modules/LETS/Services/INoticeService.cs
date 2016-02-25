using System.Collections.Generic;
using System.Web.Mvc;
using Orchard.Taxonomies.Models;
using LETS.Models;
using LETS.ViewModels;
using Orchard;
using Orchard.ContentManagement;

namespace LETS.Services
{
    public interface INoticeService : IDependency
    {
        NoticePart GetNotice(int idNotice);
        IEnumerable<NoticeTypePart> GetNoticeTypes();
        IEnumerable<NoticeTypePart> GetRequiredNoticeTypes();
        IEnumerable<NoticePart> GetMemberNotices(int idUser, int idNoticeType);
        IContentQuery<NoticePart, NoticePartRecord> GetNotices();
        IContentQuery<NoticePart> GetNotices(int idNoticeType);
        IEnumerable<dynamic> GetNoticesByLocality(int idLocality);
        IEnumerable<SelectListItem> GetCategoryTerms();
        IEnumerable<TermPart> GetTopLevelCategories();
        NoticeTypePartRecord GetNoticeType(int? id);
        string GetNoticeTypeTitle(int idNoticeType);
        int GetNoticeCount(int idNoticeType, int idCategory);

        void UpdateNoticeForContentItem(ContentItem contentItem, EditNoticeViewModel model);
        void PublishMemberNotices(int idUser);

        void DeleteMemberNotices(int idUser);

        IEnumerable<dynamic> GetMemberNoticeShapes(int idUser, string displayType = "DetailedSummary");
        IEnumerable<dynamic> GetMemberArchivedNoticeShapes(int id, string displayType = "DetailedSummaryArchived");
        IEnumerable<dynamic> GetNoticeShapesByType(int id, int page, int pageSize);
        int GetNoticeCountByType(int idNoticeType);

    }
}