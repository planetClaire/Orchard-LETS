using System;
using System.Collections.Generic;
using LETS.Models;
using LETS.Services;
using NogginBox.MailChimp.Models;
using NogginBox.MailChimp.Services;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Security;
using Orchard.Users.Events;
using Orchard.Users.Models;

namespace LETS.Events
{
    public class UserEvents : IUserEventHandler
    {
        private readonly INoticeService _noticeService;
        private readonly IMailChimpService _mailchimpService;
        private readonly IOrchardServices _orchardServices;
        private readonly IAuthenticationService _authenticationService;
        private readonly IEnumerable<IUserEventHandler> _userEventHandlers;
        private readonly IMemberService _memberService;
        private readonly ISignals _signals;

        public UserEvents(INoticeService noticeService, IMailChimpService mailchimpService, IOrchardServices orchardServices, IAuthenticationService authenticationService, IEnumerable<IUserEventHandler> userEventHandlers, IMemberService memberService, ISignals signals)
        {
            _noticeService = noticeService;
            _mailchimpService = mailchimpService;
            _orchardServices = orchardServices;
            _authenticationService = authenticationService;
            _userEventHandlers = userEventHandlers;
            _memberService = memberService;
            _signals = signals;
        }

        public void Creating(UserContext context)
        {
        }

        public void Created(UserContext context)
        {
        }

        public void LoggingIn(string userNameOrEmail, string password) {
        }

        public void LoggedIn(IUser user)
        {
        }

        public void LogInFailed(string userNameOrEmail, string password) {
        }

        public void LoggedOut(IUser user)
        {
        }

        public void AccessDenied(IUser user)
        {
        }

        public void ChangedPassword(IUser user)
        {
        }

        public void SentChallengeEmail(IUser user)
        {
        }

        public void ConfirmedEmail(IUser user)
        {
            if (user.As<UserPart>().RegistrationStatus.Equals(UserStatus.Approved))
            {
                _authenticationService.SignIn(user, false);
                foreach (var userEventHandler in _userEventHandlers)
                {
                    userEventHandler.LoggedIn(user);
                }
                _noticeService.PublishMemberNotices(user.Id);
                SubscribeToMailChimp(user);
                _signals.Trigger("letsMemberListChanged");
                _signals.Trigger("letsDisabledMemberListChanged");
            }
        }

        public void Approved(IUser user)
        {
            if (user.As<UserPart>().EmailStatus.Equals(UserStatus.Approved))
            {
                _noticeService.PublishMemberNotices(user.Id);
                SubscribeToMailChimp(user);
                _signals.Trigger("letsMemberListChanged");
                _signals.Trigger("letsDisabledMemberListChanged");
            }
        }

        private void SubscribeToMailChimp(IUser user)
        {
            var idList = _orchardServices.WorkContext.CurrentSite.As<LETSSettingsPart>().IdMailChimpList;
            var mailChimpSettings = _orchardServices.WorkContext.CurrentSite.As<MailChimpSettingsPart>();
            if (mailChimpSettings != null)
            {
                var apiKey = mailChimpSettings.ApiKey;
                if (!string.IsNullOrEmpty(idList) && !string.IsNullOrEmpty(apiKey))
                {
                    var mergeVarsForMailChimp = _memberService.GetMergeVarsForMailChimp(user.As<MemberPart>(), idList);
                    _mailchimpService.Subscribe(idList, user.Email, mergeVarsForMailChimp, "html", false, true, true, false);
                }
            }
        }

        public void Moderated(IUser user)
        {
            _noticeService.DeleteMemberNotices(user.Id);
            _signals.Trigger("letsMemberListChanged");
            _signals.Trigger("letsDisabledMemberListChanged");
            UnsubscribeMailchimp(user);
        }

        public void Deleting(IUser user)
        {
            _noticeService.DeleteMemberNotices(user.Id);
            _signals.Trigger("letsMemberListChanged");
            _signals.Trigger("letsDisabledMemberListChanged");
            UnsubscribeMailchimp(user);
        }

        private void UnsubscribeMailchimp(IUser user)
        {
            var idList = _orchardServices.WorkContext.CurrentSite.As<LETSSettingsPart>().IdMailChimpList;
            var mailChimpSettings = _orchardServices.WorkContext.CurrentSite.As<MailChimpSettingsPart>();
            if (mailChimpSettings != null) {
                var apiKey = mailChimpSettings.ApiKey;
                if (!string.IsNullOrEmpty(idList) && !string.IsNullOrEmpty(apiKey)) {
                    _mailchimpService.Unsubscribe(idList, user.Email, false, false, false);
                }
            }
        }

    }
}