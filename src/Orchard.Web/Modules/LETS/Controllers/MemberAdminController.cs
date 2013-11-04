using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using LETS.Models;
using LETS.Services;
using LETS.ViewModels;
using NogginBox.MailChimp.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using Orchard.Users.Models;

namespace LETS.Controllers
{
    [Admin]
    public class MemberAdminController : Controller
    {
        private readonly IMemberService _memberService;
        private readonly IMailChimpService _mailchimpService;
        private readonly IOrchardServices _orchardServices;
        public readonly Localizer T;
        private readonly INoticeService _noticeService;
        private readonly ITransactionService _transactionService;

        public MemberAdminController(IMemberService memberService, IMailChimpService mailchimpService, IOrchardServices orchardServices, INoticeService noticeService, ITransactionService transactionService)
        {
            _memberService = memberService;
            _mailchimpService = mailchimpService;
            _orchardServices = orchardServices;
            _noticeService = noticeService;
            _transactionService = transactionService;
            T = NullLocalizer.Instance;
        }
 
        public ActionResult Index() {
            var memberList = new MemberListViewModel { Members = _memberService.GetMemberList(MemberType.Member).OrderBy(m => m.LastName).ThenBy(m => m.FirstName), MemberType = @T("Members").ToString() };
            var adminMemberList = new MemberListViewModel { Members = _memberService.GetMemberList(MemberType.Admin).OrderBy(m => m.LastName).ThenBy(m => m.FirstName), MemberType = @T("Admin Members").ToString() };
            var letsSytemList = new MemberListViewModel { Members = _memberService.GetMemberList(MemberType.LETSystem).OrderBy(m => m.LastName).ThenBy(m => m.FirstName), MemberType = @T("Other LETS Systems").ToString() };
            var sinkingFundList = new MemberListViewModel { Members = _memberService.GetMemberList(MemberType.SinkingFund).OrderBy(m => m.LastName).ThenBy(m => m.FirstName), MemberType = @T("Sinking Fund").ToString() };
            var archivedList = new MemberListViewModel { Members = _memberService.GetMemberList(MemberType.Archived).OrderBy(m => m.LastName).ThenBy(m => m.FirstName), MemberType = @T("Archived").ToString() };
            var model = new MemberListsViewModel();
            model.MemberLists.Add(memberList);
            model.MemberLists.Add(adminMemberList);
            model.MemberLists.Add(letsSytemList);
            model.MemberLists.Add(sinkingFundList);
            model.MemberLists.Add(archivedList);
            return View(model);
        }

        public ActionResult Alerts()
        {
            // verify total credit value is equal to any credit balance for all members
            var members = _memberService.GetMemberParts();
            var memberParts = members as IList<MemberPart> ?? members.ToList();
            var alerts = (from member in memberParts
                          let memberBalance = _memberService.GetMemberBalance(member.Id, true)
                          let memberCreditValueTotal = _memberService.GetCreditValueTotal(member.Id)
                          where memberBalance > 0 && !memberBalance.Equals(memberCreditValueTotal) || memberBalance <= 0 && memberCreditValueTotal != 0
                          select new AdminAlertViewModel
                                     {
                                         Message = T("Total credit value ({0}) not equal to the credit balance ({1}) of the member", memberCreditValueTotal, memberBalance), 
                                         ReferenceLinkText = new LocalizedString(member.FirstLastName), 
                                         ReferenceLinkHref = Url.Action("List", "TransactionsAdmin", new {area = "LETS", id = member.Id}), 
                                         ActionLinkText = T("Fix it (bring the credit value in line with the member balance)"), 
                                         ActionLinkHref = Url.Action("CorrectCreditValues", "TransactionsAdmin", new {area = "LETS", id = member.Id})
                                     }).ToList();

            // find any demurrage credit usages that don't have corresponding transactions
            // perhaps the transactions have been deleted without removing the corresponding credit usage record
            // this will mean that demurrage will be incorrectly charged on that transaction in future
            var orphanedDemurrageCreditUsages = _transactionService.FindOrphanedDemurrageCreditUsages();
            alerts.AddRange(orphanedDemurrageCreditUsages.Select(orphan => new AdminAlertViewModel
                {
                    Message =
                        T(
                            "Orphaned demurrage credit usage record (demurrage transaction has probably been deleted without proper cleanup), idTransactionEarnt is {0}",
                            orphan.IdTransactionEarnt),
                    ActionLinkHref = Url.Action("DeleteCreditUsage", "MemberAdmin", new {area = "LETS", id = orphan.Id}),
                    ActionLinkText = T("Delete the orphan")
                }));

            // find members who don't meet the minimum notice requirements
            var noticeTypes = _noticeService.GetRequiredNoticeTypes();
            foreach (var noticeType in noticeTypes)
            {
                alerts.AddRange(from member in memberParts where member.As<MemberAdminPart>().MemberType.Equals(MemberType.Member)
                                let memberNoticesCount =
                                    _noticeService.GetMemberNotices(member.Id, noticeType.Id).Count()
                                where memberNoticesCount < noticeType.RequiredCount
                                select new AdminAlertViewModel
                                    {
                                        Message =
                                            T("Member only has {0} {1} but should have atleast {2} - email <a href='mailto:{3}'>{3}</a>. ",
                                              memberNoticesCount, noticeType.Title, noticeType.RequiredCount, member.As<UserPart>().Email),
                                        ReferenceLinkText = new LocalizedString(member.FirstLastName),
                                        ReferenceLinkHref =
                                            Url.Action("List", "NoticesAdmin", new {area = "LETS", id = member.Id})
                                    });
            }


            return View(alerts);
        }

        public ActionResult Mailchimp()
        {
            var idList = _orchardServices.WorkContext.CurrentSite.As<LETSSettingsPart>().IdMailChimpList;
            if (string.IsNullOrEmpty(_mailchimpService.MailChimpSettings.ApiKey) || string.IsNullOrEmpty(idList))
                return View();
            var listSubscribers = _mailchimpService.getListSubscribers(idList, "Subscribed").ToList();
            var orchardMembers = _memberService.GetMemberParts().ToList();

            var missingListMembers = (from member in orchardMembers
                                      let email = member.As<IUser>().Email
                                      where !listSubscribers.Contains(email)
                                      select
                                          new MailchimpMemberViewModel
                                              {Id = member.Id, Email = email, Name = member.FirstLastName}).ToList();

            var missingOrchardMembers = (from email in listSubscribers
                                         where
                                             orchardMembers.FirstOrDefault(m => m.As<IUser>().Email.Equals(email)) ==
                                             null
                                         select new MailchimpMemberViewModel {Email = email}).ToList();

            var mailchimpSyncViewModel = new MailchimpSyncViewModel
                                             {
                                                 MissingListMembers = missingListMembers,
                                                 MissingOrchardMembers = missingOrchardMembers
                                             };
            return View(mailchimpSyncViewModel);
        }

        public ActionResult SubscribeToMailchimp(int id)
        {
            var idList = _orchardServices.WorkContext.CurrentSite.As<LETSSettingsPart>().IdMailChimpList;
            var member = _memberService.GetMember(id);
            if (_mailchimpService.Subscribe(idList, member.As<IUser>().Email, _memberService.GetMergeVarsForMailChimp(member, idList), "html", false, true, true, false))
            {
                _orchardServices.Notifier.Information(T("Subscribed successfully"));
            }
            else
            {
                _orchardServices.Notifier.Error(T("That failed for some reason"));
            }
            return RedirectToAction("Mailchimp");
        }

        public ActionResult DeleteCreditUsage(int id)
        {
            _transactionService.DeleteCreditUsage(id);
            return RedirectToAction("Alerts");
        }
    }
}