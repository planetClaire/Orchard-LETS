using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement.Handlers;
using NogginBox.MailChimp.Models;
using Orchard.Data;

namespace NogginBox.MailChimp.Handlers
{
	public class MailChimpSettingsHandler : ContentHandler
	{
		public MailChimpSettingsHandler(IRepository<SettingsRecord> repository)
		{
			Filters.Add(new ActivatingFilter<MailChimpSettingsPart>("MailChimpSettings"));
			Filters.Add(StorageFilter.For(repository));

			Filters.Add(new TemplateFilterForRecord<SettingsRecord>("MailChimpSettings", "Parts/MailChimpSettings"));
		}
	}
}