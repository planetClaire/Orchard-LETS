using Orchard.Localization;
using Orchard.UI.Navigation;

namespace LETSBootstrap
{
    public class AdminMenu : INavigationProvider
    {
        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder)
        {
            builder.Add(T("Themes"),
                menu => menu.Add(T("Settings"), "4", item => item.Action("Index", "Admin", new { area = "LETSBootstrap" }).LocalNav())
            );
        }
    }
}