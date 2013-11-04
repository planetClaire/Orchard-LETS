using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NogginBox.MailChimp.Models
{
	public class SettingsRecord : ContentPartRecord
	{
		[DisplayName("Mailchimp API Key")]
		[Required]
		[ApiKeyValid(ErrorMessage="That key is bad")]
		public virtual String ApiKey { get; set; }
	}



	public class MailChimpSettingsPart : ContentPart<SettingsRecord>
	{
		public String ApiKey
		{
			get { return Record.ApiKey; }
			set { Record.ApiKey = value; }
		}
	}
}