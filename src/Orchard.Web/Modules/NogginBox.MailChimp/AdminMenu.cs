using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.UI.Navigation;
using Orchard.Localization;
using Orchard.Security;

namespace NogginBox.MailChimp
{
	public class AdminMenu : INavigationProvider
	{
		public Localizer T { get; set; }

		public string MenuName { get { return "admin"; } }

		public void GetNavigation(NavigationBuilder builder)
		{
			builder.Add(T("Settings"), "50",
				menu => menu
					.Add(T("MailChimp API Key"), "10", item => item.Action("Index", "Admin", new { area = "NogginBox.MailChimp" })
						.Permission(StandardPermissions.SiteOwner)));
		}
	}
}