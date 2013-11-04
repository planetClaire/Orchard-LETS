using JetBrains.Annotations;
using NogginBox.MailChimp.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;

namespace NogginBox.MailChimp.Drivers
{
	[UsedImplicitly]
	public class MailChimpSettingsDriver : ContentPartDriver<MailChimpSettingsPart>
	{
		public Localizer T { get; set; }

		protected override DriverResult Editor(MailChimpSettingsPart settingsPart, IUpdateModel updater, dynamic shapeHelper)
		{
			updater.TryUpdateModel(settingsPart, Prefix, null, null);
			
			return Editor(settingsPart, shapeHelper);
		}
	}
}