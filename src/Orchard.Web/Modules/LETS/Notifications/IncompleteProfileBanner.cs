using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using JetBrains.Annotations;
using LETS.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Fields;
using Orchard.Fields.Fields;
using Orchard.Localization;
//using Orchard.MediaPicker.Fields;
using Orchard.UI.Notify;

namespace LETS.Notifications
{
    [UsedImplicitly]
    public class IncompleteProfileBanner : INotificationProvider
    {
        private readonly IOrchardServices _orchardServices;
        public Localizer T { get; set; }
 

        public IncompleteProfileBanner(IOrchardServices orchardServices)
        {
            _orchardServices = orchardServices;
            T = NullLocalizer.Instance;
        }

        public IEnumerable<NotifyEntry> GetNotifications()
        {
            if (!_orchardServices.Authorizer.Authorize(Permissions.AccessMemberContent))
                yield break;

            var currentUser = _orchardServices.WorkContext.CurrentUser;
            var currentMember = currentUser.As<MemberPart>();
            var complete = true;
            foreach (var requiredForCompleteField in currentMember.Fields.Where(f => f.Name.StartsWith("rfc")))
            {
                switch (requiredForCompleteField.FieldDefinition.Name)  
                {
                    case "TextField":
                        if (string.IsNullOrEmpty(((TextField)requiredForCompleteField).Value))
                            complete = false;
                        break;
                    case "EnumerationField":
                        if ((((EnumerationField)requiredForCompleteField).SelectedValues).Length.Equals(0))
                            complete = false;
                        break;
                    case "InputField":
                        if (string.IsNullOrEmpty(((InputField)requiredForCompleteField).Value))
                            complete = false;
                        break;
                    case "LinkField":
                        if (string.IsNullOrEmpty(((LinkField)requiredForCompleteField).Value))
                            complete = false;
                        break;
                    //case "MediaPickerField":
                    //    if (string.IsNullOrEmpty(((MediaPickerField)requiredForCompleteField).Url))
                    //        complete = false;
                    //    break;
                    case "NumericField":
                        if (((NumericField)requiredForCompleteField).Value == null)
                            complete = false;
                        break;
                    case "BooleanField":
                        if (((BooleanField)requiredForCompleteField).Value == null)
                            complete = false;
                        break;
                }
            }
            if (!complete)
            {
                var urlHelper = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
                yield return new NotifyEntry { Message = T("Your profile isn't quite complete. Members will be more likely to trade with you if you <a href=\"{0}\">tell us more about yourself</a>.", urlHelper.Action("Edit", "Home", new { area = "Contrib.Profile" })), Type = NotifyType.Warning };
            }
        }
    }
}