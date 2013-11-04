using LETS.Models;
using Orchard.ContentManagement.Drivers;

namespace LETS.Drivers
{
    public class AuthNavigationWidgetPartDriver: ContentPartDriver<AuthNavigationWidgetPart>
    {
        protected override DriverResult Display(AuthNavigationWidgetPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("AuthNavigationWidget", () => shapeHelper.AuthNavigationWidget(
                ));
        }
    }
}