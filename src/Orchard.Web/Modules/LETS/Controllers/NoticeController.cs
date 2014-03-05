using System;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using LETS.Models;
using LETS.Services;
using LETS.ViewModels;
using Orchard;
using Orchard.ArchiveLater.Models;
using Orchard.ArchiveLater.Services;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Security;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using Orchard.Themes;
using Orchard.UI.Notify;

namespace LETS.Controllers
{
    [Themed]
    [ValidateInput(false)]
    public class NoticeController : Controller, IUpdateModel
    {
        private readonly IContentManager _contentManager;
        private readonly IOrchardServices _orchardServices;
        private readonly ITaxonomyService _taxonomyService;
        private readonly INoticeService _noticeService;
        public Localizer T { get; set; }

        private readonly IAuthorizationService _authorizationService;
        private readonly IMemberService _memberService;
        private readonly ISignals _signals;
        private readonly IArchiveLaterService _archiveLaterService;

        public NoticeController(IContentManager contentManager, IOrchardServices orchardServices, ITaxonomyService taxonomyService, INoticeService noticeService, IAuthorizationService authorizationService, IMemberService memberService, ISignals signals, IArchiveLaterService archiveLaterService)
        {
            _contentManager = contentManager;
            _orchardServices = orchardServices;
            _taxonomyService = taxonomyService;
            _noticeService = noticeService;
            T = NullLocalizer.Instance;
            _authorizationService = authorizationService;
            _memberService = memberService;
            _signals = signals;
            _archiveLaterService = archiveLaterService;
        }

        public ActionResult Browse()
        {
            var viewModel = new BrowseNoticeCategoriesViewModel();
            var rootCategories = _noticeService.GetTopLevelCategories();
            foreach (var rootCategory in rootCategories)
            {
                var category = new BrowseNoticeCategoryViewModel { Term = rootCategory };
                var subCategories = _taxonomyService.GetChildren(rootCategory);
                foreach (var subCategory in subCategories)
                {
                    var browseNoticeSubCategoryViewModel = new BrowseNoticeSubCategoryViewModel { Term = subCategory, NoticeCount = subCategory.Count };
                    category.SubCategories.Add(browseNoticeSubCategoryViewModel);
                }
                viewModel.Categories.Add(category);
            }
            return View(viewModel);
        }

        public ActionResult Create()
        {
            if (!_orchardServices.Authorizer.Authorize(Permissions.AccessMemberContent))
            {
                return new HttpUnauthorizedResult();
            }
            var notice = _contentManager.New("Notice");
            var noticeModel = _contentManager.BuildEditor(notice);
            noticeModel.NoticeCategoryViewModel = new NoticeCategoryViewModel
            {
                Categories = _noticeService.GetCategoryTerms()
            };
            return View((object)noticeModel);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePOST()
        {
            if (!_orchardServices.Authorizer.Authorize(Permissions.AccessMemberContent))
            {
                return new HttpUnauthorizedResult();
            }
            var notice = _contentManager.Create("Notice", VersionOptions.Draft);
            var noticeModel = _contentManager.UpdateEditor(notice, this);
            if (!ModelState.IsValid)
            {
                noticeModel.NoticeCategoryViewModel = GetNoticeCategoryViewModel(notice.Id);
                return View((object)noticeModel);
            }
            var term = _taxonomyService.GetTerm(int.Parse(Request.Form["noticeCategoryVM.IdCategory"]));
            if (notice.Has<TermsPart>())
            {
                _taxonomyService.UpdateTerms(notice, new [] { term }, "Category");
                _contentManager.Publish(notice);
            }
            _orchardServices.Notifier.Information(T("Your Notice has been created."));
            return Redirect(string.Format("~/{0}", term.Slug));
        }

        public ActionResult Edit(int id)
        {
            var notice = _contentManager.Get(id, VersionOptions.Latest);
            if (!_authorizationService.TryCheckAccess(Orchard.Core.Contents.Permissions.EditContent, _orchardServices.WorkContext.CurrentUser, notice)) {
                return new HttpUnauthorizedResult();
            }
            var noticeModel = _contentManager.BuildEditor(notice);
            noticeModel.NoticeCategoryViewModel = GetNoticeCategoryViewModel(id);
            noticeModel.IsPublished = notice.VersionRecord.Published;
            return View((object)noticeModel);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("submit.Save")]
        public ActionResult EditPOST(int id)
        {
            var notice = _contentManager.Get(id, VersionOptions.Latest);
            if (!_authorizationService.TryCheckAccess(Orchard.Core.Contents.Permissions.EditContent, _orchardServices.WorkContext.CurrentUser, notice))
            {
                return new HttpUnauthorizedResult();
            }
            var noticeModel = _contentManager.UpdateEditor(notice, this);
            if (!ModelState.IsValid)
            {
                noticeModel.NoticeCategoryViewModel = GetNoticeCategoryViewModel(id);
                return View((object)noticeModel);
            }
            var term = _taxonomyService.GetTerm(int.Parse(Request.Form["noticeCategoryVM.IdCategory"]));
            if (notice.Has<TermsPart>())
            {
                _taxonomyService.UpdateTerms(notice, new BindingList<TermPart> {term}, "Category");
            }
            if (notice.VersionRecord.Published) {
                _orchardServices.Notifier.Information(T("Your Notice has been updated."));
            }
            else {
                _contentManager.Publish(notice);
                _orchardServices.Notifier.Information(T("Your Notice has been saved & published."));
            }
            var idOwner = notice.As<CommonPart>().Owner.Id;
            _signals.Trigger(string.Format("letsMemberNoticesChanged{0}", idOwner));
            _signals.Trigger(string.Format("letsMemberArchivedNoticesChanged{0}", idOwner));
            _signals.Trigger(string.Format("letsNoticesByLocalityChanged{0}", _memberService.GetMember(idOwner).As<AddressPart>().Locality.Id));
            return Redirect(string.Format("~/{0}", term.Slug));
        }

        private NoticeCategoryViewModel GetNoticeCategoryViewModel(int idContentItem)
        {
            var noticeCategoryViewModel = new NoticeCategoryViewModel
            {
                Categories = _noticeService.GetCategoryTerms()
            };
            var term = _taxonomyService.GetTermsForContentItem(idContentItem).FirstOrDefault();
            if (term != null)
            {
                noticeCategoryViewModel.IdCategory = term.Id;
                // help the select list bind to the id
                ViewData.Add("noticeCategoryVM.IdCategory", term.Id);
            }
            return noticeCategoryViewModel;
        }

        [HttpPost]
        public ActionResult Unpublish(int id) {
            return UnpublishPOST(id);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("submit.Unpublish")]
        public ActionResult UnpublishPOST(int id)
        {
            var notice = _contentManager.Get(id, VersionOptions.Latest);
            if (!_authorizationService.TryCheckAccess(Orchard.Core.Contents.Permissions.EditContent, _orchardServices.WorkContext.CurrentUser, notice))
            {
                return new HttpUnauthorizedResult();
            }
            _contentManager.Unpublish(notice);
            var idUser = notice.As<CommonPart>().Owner.Id;
            _signals.Trigger(string.Format("letsMemberNoticesChanged{0}", idUser));
            _signals.Trigger(string.Format("letsMemberArchivedNoticesChanged{0}", idUser));
            _signals.Trigger(string.Format("letsNoticesByLocalityChanged{0}", _memberService.GetMember(idUser).As<AddressPart>().Locality.Id));
            _orchardServices.Notifier.Information(T("Your Notice has been archived."));
            return RedirectToAction("Member", new { id = idUser });
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("submit.Delete")]
        public ActionResult DeletePOST(int id)
        {
            return Delete(id);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var notice = _contentManager.Get(id, VersionOptions.Latest);
            if (!_authorizationService.TryCheckAccess(Orchard.Core.Contents.Permissions.DeleteContent, _orchardServices.WorkContext.CurrentUser, notice))
            {
                return new HttpUnauthorizedResult();
            }
            _contentManager.Remove(notice);
            var idUser = notice.As<CommonPart>().Owner.Id;
            _signals.Trigger(string.Format("letsMemberNoticesChanged{0}", idUser));
            _signals.Trigger(string.Format("letsMemberArchivedNoticesChanged{0}", idUser));
            _signals.Trigger(string.Format("letsNoticesByLocalityChanged{0}", _memberService.GetMember(idUser).As<AddressPart>().Locality.Id));
            _orchardServices.Notifier.Information(T("Your Notice has been deleted."));
            return RedirectToAction("Member", new { id = idUser });
        }

        [HttpPost]
        public ActionResult Publish(int id)
        {
            var notice = _contentManager.Get(id, VersionOptions.Latest);
            if (!_authorizationService.TryCheckAccess(Orchard.Core.Contents.Permissions.PublishContent, _orchardServices.WorkContext.CurrentUser, notice))
            {
                return new HttpUnauthorizedResult();
            }
            // if archive date is null set archive date to max before publishing
            var archiveDate = (notice).As<ArchiveLaterPart>().ScheduledArchiveUtc.Value;
            if (!archiveDate.HasValue) {
                _archiveLaterService.ArchiveLater(notice, DateTime.Now.AddDays(
                         _orchardServices.WorkContext.CurrentSite.As<LETSSettingsPart>()
                             .MaximumNoticeAgeDays));
            }
            _contentManager.Publish(notice);
            var idUser = notice.As<CommonPart>().Owner.Id;
            _signals.Trigger(string.Format("letsMemberNoticesChanged{0}", idUser));
            _signals.Trigger(string.Format("letsMemberArchivedNoticesChanged{0}", idUser));
            _signals.Trigger(string.Format("letsNoticesByLocalityChanged{0}", _memberService.GetMember(idUser).As<AddressPart>().Locality.Id));
            _orchardServices.Notifier.Information(T("Your notice has been published"));
            return RedirectToAction("Member", new { id = idUser });
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties)
        {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage)
        {
            ModelState.AddModelError(key, errorMessage.ToString());
        }

        public ActionResult Member(int id)
        {
            if (!_orchardServices.Authorizer.Authorize(Permissions.AccessMemberContent))
                return new HttpUnauthorizedResult();

            var memberNoticesViewModel = new MemberNoticesViewModel {Notices = _noticeService.GetMemberNoticeShapes(id), ArchivedNotices = _noticeService.GetMemberArchivedNoticeShapes(id), Member = _memberService.GetMember(id), AdminIsViewing = _orchardServices.Authorizer.Authorize(Permissions.AdminMemberContent)};

            return View(memberNoticesViewModel);
        }

    }
}
