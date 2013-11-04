using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement.Records;
using Orchard.ContentManagement;
using System.ComponentModel;

namespace NogginBox.MailChimp.Models
{
	public class MergeVariableRecord
	{
		public virtual int Id { get; set; }

		public virtual String Tag { get; set; }

		public virtual String Label { get; set; }

		public virtual int? Type { get; set; }

		public virtual bool Required { get; set; }

		public virtual int DisplayOrder { get; set; }

		public virtual String Choices { get; set; }

		public virtual FormRecord FormRecord { get; set; }
	}


	public enum MergeVariableType
	{
		Email, Text, Number, Multi_choice, Drop_down, Date, Address, Phone, Website, Image_url
	}
}