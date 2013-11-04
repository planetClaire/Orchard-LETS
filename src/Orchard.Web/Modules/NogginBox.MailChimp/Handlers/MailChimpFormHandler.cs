using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement.Handlers;
using NogginBox.MailChimp.Models;
using Orchard.Data;

namespace NogginBox.MailChimp.Handlers
{
	public class MailChimpFormHandler : ContentHandler
	{
		public MailChimpFormHandler(IRepository<FormRecord> repository)
		{
			Filters.Add(StorageFilter.For(repository));
		}
	}
}