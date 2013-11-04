using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using LETS.Models;
using LETS.Services;
using LETS.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Security;

namespace LETS.Drivers
{
    [UsedImplicitly]
    public class TransactionPartDriver : ContentPartDriver<TransactionPart>
    {
        private readonly ITransactionService _transactionService;
        private readonly IMemberService _memberService;
        private readonly IWorkContextAccessor _workContext;
        private readonly IContentManager _contentManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IAuthenticationService _authenticationService;

        public TransactionPartDriver(ITransactionService transactionService, IMemberService memberService, IWorkContextAccessor workContext, IContentManager contentManager, IAuthorizationService authorizationService, IAuthenticationService authenticationService)
        {
            _transactionService = transactionService;
            _memberService = memberService;
            _workContext = workContext;
            _contentManager = contentManager;
            _authorizationService = authorizationService;
            _authenticationService = authenticationService;
        }

        protected override DriverResult Display(TransactionPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_Transaction", () => shapeHelper.Parts_Transaction(ContentPart: part));
        }

        protected override DriverResult Editor(TransactionPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_Transaction_Edit",
                                () => shapeHelper.EditorTemplate(
                                    TemplateName: "Parts/Transaction",
                                    Model: BuildEditorViewModel(part),
                                    Prefix: Prefix));
        }

        protected override DriverResult Editor(TransactionPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            var model = new EditTransactionViewModel();
            if (updater.TryUpdateModel(model, Prefix, null, null))
            {
                if (part.ContentItem.Id != 0)
                {
                    _transactionService.SaveTransaction(part.ContentItem, model);
                }
            }
            return Editor(part, shapeHelper);
        }

        private EditTransactionViewModel BuildEditorViewModel(TransactionPart part)
        {
            var memberTypes = new List<MemberType> { MemberType.Member, MemberType.Admin, MemberType.LETSystem };
            if (_authorizationService.TryCheckAccess(Permissions.AdminMemberContent, _authenticationService.GetAuthenticatedUser(), part)) {
                memberTypes.Add(MemberType.SinkingFund);
                memberTypes.Add(MemberType.Archived);
            }
            var cultureInfo = CultureInfo.GetCultureInfo(_workContext.GetContext().CurrentCulture);
            var shortDateFormat = cultureInfo.DateTimeFormat.ShortDatePattern;
            var shortTimeFormat = cultureInfo.DateTimeFormat.ShortTimePattern;
            var tvm = new EditTransactionViewModel
                {
                    Members = _memberService.GetGroupedMembers(memberTypes),
                    Description = part.Description,
                    TransactionDate = part.TransactionDate == null ? DateTime.Now.ToString(shortDateFormat) : ((DateTime)part.TransactionDate).ToString(shortDateFormat),
                    TransactionTime = part.TransactionDate == null ? DateTime.Now.ToString(shortTimeFormat) : ((DateTime)part.TransactionDate).ToString(shortTimeFormat),
                    Value = part.Value,
                    TransactionType = part.TransactionType,
                    CurrencyUnit = _workContext.GetContext().CurrentSite.As<LETSSettingsPart>().CurrencyUnit
                };
            if (part.Seller != null)
            {
                tvm.IdSeller = part.Seller.Id;
            }
            if (part.Buyer != null)
            {
                tvm.IdBuyer = part.Buyer.Id;
            }
            return tvm;
        }

        protected override void Importing(TransactionPart part, Orchard.ContentManagement.Handlers.ImportContentContext context)
        {
            part.TransactionDate = DateTime.Parse(context.Attribute(part.PartDefinition.Name, "TransactionDate"), CultureInfo.InvariantCulture);
            part.Value = int.Parse(context.Attribute(part.PartDefinition.Name, "Value"));
            part.CreditValue = int.Parse(context.Attribute(part.PartDefinition.Name, "CreditValue"));
            part.Description = context.Attribute(part.PartDefinition.Name, "Description");
            part.TransactionType =
                (TransactionType)
                Enum.Parse(typeof (TransactionType), context.Attribute(part.PartDefinition.Name, "TransactionType"));
        }

        protected override void Imported(TransactionPart part, Orchard.ContentManagement.Handlers.ImportContentContext context)
        {
            var notice = context.Attribute(part.PartDefinition.Name, "Notice");
            if (notice != null)
            {
                part.Record.NoticePartRecord = context.GetItemFromSession(notice).As<NoticePart>().Record;
            }
            var seller = context.Attribute(part.PartDefinition.Name, "Seller");
            if (seller != null)
            {
                part.Record.SellerMemberPartRecord = context.GetItemFromSession(seller).As<MemberPart>().Record;
            }
            var buyer = context.Attribute(part.PartDefinition.Name, "Buyer");
            if (buyer != null)
            {
                part.Record.BuyerMemberPartRecord = context.GetItemFromSession(buyer).As<MemberPart>().Record;
            }
        }

        protected override void Exporting(TransactionPart part, Orchard.ContentManagement.Handlers.ExportContentContext context)
        {
            context.Element(part.PartDefinition.Name).SetAttributeValue("TransactionDate", part.TransactionDate);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Value", part.Value);
            context.Element(part.PartDefinition.Name).SetAttributeValue("CreditValue", part.CreditValue);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Description", part.Description);
            context.Element(part.PartDefinition.Name).SetAttributeValue("TransactionType", part.TransactionType);
            var noticePartRecord = part.Record.NoticePartRecord;
            if (noticePartRecord != null)
            {
                var noticePart =
                    _contentManager.Query<NoticePart, NoticePartRecord>("Notice").Where(x => x.Id == noticePartRecord.Id)
                        .List().FirstOrDefault();
                if (noticePart != null)
                {
                    var noticeIdentity = _contentManager.GetItemMetadata(noticePart).Identity;
                    context.Element(part.PartDefinition.Name).SetAttributeValue("Notice", noticeIdentity.ToString());
                }
            }
            var sellerPart = _contentManager.Query<MemberPart, MemberPartRecord>("User").Where(x => x.Id == part.Record.SellerMemberPartRecord.Id).List().FirstOrDefault();
            if (sellerPart != null)
            {
                var sellerIdentity = _contentManager.GetItemMetadata(sellerPart).Identity;
                context.Element(part.PartDefinition.Name).SetAttributeValue("Seller", sellerIdentity.ToString());
            }
            var buyerPart = _contentManager.Query<MemberPart, MemberPartRecord>("User").Where(x => x.Id == part.Record.BuyerMemberPartRecord.Id).List().FirstOrDefault();
            if (buyerPart != null)
            {
                var buyerIdentity = _contentManager.GetItemMetadata(buyerPart).Identity;
                context.Element(part.PartDefinition.Name).SetAttributeValue("Buyer", buyerIdentity.ToString());
            }
        }
    }
}