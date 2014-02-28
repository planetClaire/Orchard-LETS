﻿using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Taxonomies.Services;
using LETS.Models;
using LETS.Services;
using LETS.ViewModels;
using Orchard;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Settings.Models;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Mvc.Extensions;
using Orchard.Security;
using Orchard.Themes;
using Orchard.UI.Notify;
using Orchard.Users.Models;
using Orchard.Users.Services;
using Orchard.Users.Events;
using Orchard.Utility.Extensions;

namespace LETS.Controllers
{
    [Themed]
    public class AccountController : Orchard.Users.Controllers.AccountController, IUpdateModel 
    {
        private readonly IMembershipService _membershipService;
        private readonly IOrchardServices _orchardServices;
        private readonly IContentManager _contentManager;
        private readonly INoticeService _noticeService;
        private readonly ITaxonomyService _taxonomyService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IUserEventHandler _userEventHandlers;
        private readonly IUserService _userService;

        public AccountController(IAuthenticationService authenticationService, IMembershipService membershipService, IUserService userService, IOrchardServices orchardServices, IUserEventHandler userEventHandlers, IContentManager contentManager, INoticeService noticeService, ITaxonomyService taxonomyService) 
            : base(authenticationService, membershipService, userService, orchardServices, userEventHandlers)
        {
            _membershipService = membershipService;
            _orchardServices = orchardServices;
            _contentManager = contentManager;
            _noticeService = noticeService;
            _taxonomyService = taxonomyService;
            _authenticationService = authenticationService;
            _userEventHandlers = userEventHandlers;
            _userService = userService;
        }

        public ActionResult RegisterMember()
        {
            // start with the base & add a user profile
            dynamic shapeResult = Register();
            var user = _contentManager.New("User");
            shapeResult.Model.UserProfile = _contentManager.BuildEditor(user);
            shapeResult.Model.RequiredNoticeTypes = BuildRegisterNoticeTypesViewModel();
            return new ShapeResult(this, shapeResult.Model);
        }

        [HttpPost]
        public ActionResult RegisterMember(RegisterMemberViewModel memberVM, RegisterNoticeTypesViewModel noticeTypesVM)
        {
            ActionResult actionResult = null;
            // validate user with attached parts
            var dummyUser = _contentManager.New("User");
            var registerShape = _orchardServices.New.Register();
            registerShape.UserProfile = _contentManager.UpdateEditor(dummyUser, this);
            if (!ModelState.IsValid)
            {
                _orchardServices.TransactionManager.Cancel();
                registerShape.RequiredNoticeTypes = BuildRegisterNoticeTypesViewModel();
                return new ShapeResult(this, registerShape);
            }
            // validated, proceed to register member
            dynamic usersRegisterResult = Register(memberVM.Email, memberVM.Email, memberVM.Password, memberVM.ConfirmPassword);
            if (usersRegisterResult is RedirectResult || usersRegisterResult is RedirectToRouteResult)
            {
                // successful result, now update the new user with the parts
                //_contentManager.Flush();
                var newUser = _membershipService.GetUser(memberVM.Email);
                if (newUser != null)
                {
                    _orchardServices.ContentManager.UpdateEditor(newUser, this);
                    newUser.As<MemberAdminPart>().JoinDate = DateTime.Now;
                    actionResult = usersRegisterResult;
                    try
                    {
                        SaveNoticeDrafts(noticeTypesVM, newUser);
                    }
                    catch (Exception)
                    {
                        ModelState.AddModelError("NoticesFailed", "You have been registered, but something went wrong creating your notices, please try again after you have logged in");
                        registerShape.RequiredNoticeTypes = BuildRegisterNoticeTypesViewModel();
                        return new ShapeResult(this, registerShape);
                    }
                }
            }
            else
            {
                _orchardServices.TransactionManager.Cancel();
                registerShape.RequiredNoticeTypes = BuildRegisterNoticeTypesViewModel();
                actionResult = new ShapeResult(this, registerShape);
            }
            // we're using email as username
            ModelState.Remove("username");
            return actionResult;
        }

        /// <summary>
        /// override Orchard.AccountController.LogOn - would like to give user better feedback about the reason for login failure, although Orchard team sees this as a security risk
        /// http://orchard.codeplex.com/workitem/18079 
        /// </summary>
        /// <returns></returns>
        [AlwaysAccessible]
        public ActionResult LogOnMember()
        {
            if (_authenticationService.GetAuthenticatedUser() != null)
                return Redirect("~/");

            var shape = _orchardServices.New.LogOn().Title(T("Log On").Text);
            return new ShapeResult(this, shape);
        }

        [HttpPost]
        [AlwaysAccessible]
        public ActionResult LogOnMember(string userNameOrEmail, string password, string returnUrl, bool createPersistentCookie)
        {
            var user = ValidateLogOn(userNameOrEmail, password);
            if (!ModelState.IsValid)
            {
                var shape = _orchardServices.New.LogOn().Title(T("Log On").Text);
                return new ShapeResult(this, shape);
            }

            _authenticationService.SignIn(user, createPersistentCookie);
            //foreach (var userEventHandler in _userEventHandlers)
            //{
            //    userEventHandler.LoggedIn(user);
            //}
            _userEventHandlers.LoggedIn(user);

            return this.RedirectLocal(returnUrl);
        }

        [AlwaysAccessible]
        public ActionResult RequestLostPasswordMember()
        {
            // ensure users can request lost password
            var registrationSettings = _orchardServices.WorkContext.CurrentSite.As<RegistrationSettingsPart>();
            if (!registrationSettings.EnableLostPassword)
            {
                return HttpNotFound();
            }

            return View();
        }


        [HttpPost]
        [AlwaysAccessible]
        public ActionResult RequestLostPasswordMember(string username)
        {
            // ensure users can request lost password
            var registrationSettings = _orchardServices.WorkContext.CurrentSite.As<RegistrationSettingsPart>();
            if (!registrationSettings.EnableLostPassword)
            {
                return HttpNotFound();
            }

            if (String.IsNullOrWhiteSpace(username))
            {
                _orchardServices.Notifier.Error(T("Invalid email"));
                return View();
            }

            var siteUrl = _orchardServices.WorkContext.CurrentSite.As<SiteSettings2Part>().BaseUrl;
            if (String.IsNullOrWhiteSpace(siteUrl))
            {
                siteUrl = HttpContext.Request.ToRootUrlString();
            }

            var success = _userService.SendLostPasswordEmail(username, nonce => Url.MakeAbsolute(Url.Action("LostPasswordMember", "Account", new { Area = "LETS", nonce = nonce }), siteUrl));

            if (success)
            {
                _orchardServices.Notifier.Information(T("Check your email for instructions."));
            }
            else
            {
                var message = ReasonFailure(username);
                _orchardServices.Notifier.Error(T(message));
            }

            return RedirectToAction("LogOn");
        }

        public ActionResult LostPasswordMember(string nonce) {
            if (_userService.ValidateLostPassword(nonce) == null)
            {
                return RedirectToAction("LogOn");
            }

            ViewData["PasswordLength"] = MinPasswordLength;

            return View();
        }

        [HttpPost]
        public ActionResult LostPasswordMember(string nonce, string newPassword, string confirmPassword)
        {
            IUser user;
            if ((user = _userService.ValidateLostPassword(nonce)) == null)
            {
                return Redirect("~/");
            }

            ViewData["PasswordLength"] = MinPasswordLength;

            if (newPassword == null || newPassword.Length < MinPasswordLength)
            {
                ModelState.AddModelError("newPassword", T("You must specify a new password of {0} or more characters.", MinPasswordLength));
            }

            if (!String.Equals(newPassword, confirmPassword, StringComparison.Ordinal))
            {
                ModelState.AddModelError("_FORM", T("The new password and confirmation password do not match."));
            }

            if (!ModelState.IsValid)
            {
                return View();
            }

            _membershipService.SetPassword(user, newPassword);

            //foreach (var userEventHandler in _userEventHandlers)
            //{
            //    userEventHandler.ChangedPassword(user);
            //}
            _userEventHandlers.ChangedPassword(user);

            return RedirectToAction("ChangePasswordSuccessMember");
        }

        public ActionResult ChangePasswordSuccessMember()
        {
            return View();
        }

        int MinPasswordLength
        {
            get
            {
                return _membershipService.GetSettings().MinRequiredPasswordLength;
            }
        }

        private IUser ValidateLogOn(string userNameOrEmail, string password)
        {
            bool validate = true;

            if (String.IsNullOrEmpty(userNameOrEmail))
            {
                ModelState.AddModelError("userNameOrEmail", T("You must specify an email."));
                validate = false;
            }
            if (String.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("password", T("You must specify a password."));
                validate = false;
            }

            if (!validate)
                return null;

            var user = _membershipService.ValidateUser(userNameOrEmail, password);
            if (user == null)
            {
                // why did validateUser fail?  todo improve this but wait to see what happens with issue above
                var message = ReasonFailure(userNameOrEmail);
                ModelState.AddModelError("_FORM", T(message));
            }

            return user;
        }

        private string ReasonFailure(string userNameOrEmail)
        {
            var lowerName = userNameOrEmail.ToLowerInvariant();
            IUser user = _orchardServices.ContentManager.Query<UserPart, UserPartRecord>().Where(u => u.NormalizedUserName == lowerName).
                             List().FirstOrDefault() ??
                         _orchardServices.ContentManager.Query<UserPart, UserPartRecord>().Where(u => u.Email == lowerName).List().
                             FirstOrDefault();
            string message;
            if (user == null)
            {
                message = "We do not have that email address in our records";
            }
            else if (((UserPart) user).EmailStatus.Equals(UserStatus.Pending))
            {
                message =
                    "You haven't confirmed your email address yet, please check your email for the confirmation email, or contact us for help";
            }
            else if (((UserPart) user).RegistrationStatus.Equals(UserStatus.Pending))
            {
                message =
                    "Your membership hasn't been approved yet.  Please wait a bit longer, or contact us for help";
            }
            else
            {
                message = "The password provided is incorrect.";
            }
            return message;
        }

        private RegisterNoticeTypesViewModel BuildRegisterNoticeTypesViewModel()
        {
            var noticeTypesVM = new RegisterNoticeTypesViewModel
            {
                CategoryTerms = _noticeService.GetCategoryTerms()
            };
            var requiredNoticeTypes = _noticeService.GetRequiredNoticeTypes();
            foreach (var noticeType in requiredNoticeTypes)
            {
                var noticesVM = new RegisterNoticesViewModel(noticeType.RequiredCount)
                {
                    IdNoticeType = noticeType.Id,
                    NoticeTypeName =
                        _contentManager.GetItemMetadata(noticeType).DisplayText
                };
                noticeTypesVM.NoticeTypes.Add(noticesVM);
            }
            return noticeTypesVM;
        }

        private void SaveNoticeDrafts(RegisterNoticeTypesViewModel noticeTypesVM, IUser newUser)
        {
            foreach (var noticeTypeVM in noticeTypesVM.NoticeTypes)
            {
                var noticeType = _noticeService.GetNoticeType(noticeTypeVM.IdNoticeType);
                foreach (var noticeVM in noticeTypeVM.Notices)
                {
                    var noticePart = _contentManager.Create<NoticePart>("Notice", VersionOptions.Draft);
                    noticePart.Title = noticeVM.Title;
                    noticePart.As<CommonPart>().Owner = newUser;
                    noticePart.NoticeType = noticeType;
                    _taxonomyService.UpdateTerms(noticePart.ContentItem,
                                                 new[] {_taxonomyService.GetTerm(noticeVM.IdCategoryTerm)}, "Category");
                }
            }
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties)
        {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage)
        {
            ModelState.AddModelError(key, errorMessage.ToString());
        }

    }
}