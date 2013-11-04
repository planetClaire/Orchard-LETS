using LETS.Models;
using LETS.Services;
using Orchard.ContentManagement.Drivers;

namespace LETS.Drivers
{
    public class StatsWidgetPartDriver : ContentPartDriver<StatsWidgetPart>
    {
        private readonly IMemberService _memberService;

        public StatsWidgetPartDriver(IMemberService memberService)
        {
            _memberService = memberService;
        }

        protected override DriverResult Display(StatsWidgetPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("StatsWidget", () => shapeHelper.StatsWidget(
                TotalTurnover: _memberService.GetTotalTurnover()
                ));
        }
    }
}