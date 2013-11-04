using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using NogginBox.MailChimp.Services;

namespace NogginBox.MailChimp.Models
{
	public class ApiKeyValidAttribute : ValidationAttribute
	{
		private readonly IMailChimpService _mailChimpService;

		public ApiKeyValidAttribute() {
			// Todo: Figure out how to inject this service
			_mailChimpService = new MailChimpService(null, null, null);
		}

		public override bool IsValid(object value)
		{
			try
			{
				var apiKey = (String)value;
				
				return _mailChimpService.IsApiKeyValid(apiKey);
			}
			catch(InvalidCastException)
			{
				return false;
			}
		}
	}
}