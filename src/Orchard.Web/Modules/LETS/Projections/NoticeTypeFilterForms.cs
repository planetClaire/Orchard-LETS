using System;
using System.Web.Mvc;
using LETS.Services;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.DisplayManagement;
using Orchard.Events;
using Orchard.Localization;

namespace LETS.Projections
{
    public interface IFormProvider : IEventHandler
    {
        void Describe(dynamic context);
    }

    public class NoticeTypeFilterForms : IFormProvider
    {
        private readonly INoticeService _noticeService;
        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public NoticeTypeFilterForms(
            IShapeFactory shapeFactory,
            INoticeService noticeService)
        {
            _noticeService = noticeService;
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(dynamic context)
        {
            Func<IShapeFactory, dynamic> form =
                shape =>
                {

                    var f = Shape.Form(
                        Id: "SelectNoticeTypes",
                        _NoticeTypes: Shape.SelectList(
                            Id: "noticetypeids", Name: "NoticeTypeIds",
                            Title: T("Notice Types"),
                            Description: T("Select some Notice Types."),
                            Size: 10,
                            Multiple: true
                            )
                        );

                    foreach (var noticeType in _noticeService.GetNoticeTypes())
                    {
                        f._NoticeTypes.Add(new SelectListItem { Value = noticeType.Id.ToString(), Text = noticeType.Title });
                    }

                    return f;
                };

            context.Form("SelectNoticeTypes", form);

        }
    }
}