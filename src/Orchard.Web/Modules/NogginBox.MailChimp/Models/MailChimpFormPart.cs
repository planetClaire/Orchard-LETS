using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement.Records;
using Orchard.ContentManagement;
using System.ComponentModel;

namespace NogginBox.MailChimp.Models
{
	public class FormRecord : ContentPartRecord
	{
		public virtual String ListId { get; set; }
		public virtual String Message { get; set; }
		public virtual String ThankyouMessage { get; set; }
		public virtual IList<MergeVariableRecord> MergeVariables { get; set; }
		public virtual IList<InterestGroupingsRecord> InterestGroupings { get; set; }
	}

	public class MailChimpFormPart : ContentPart<FormRecord>
	{
		[DisplayName("List ID")]
		public String ListId
		{
			get { return Record.ListId; }
			set { Record.ListId = value; }
		}

		public String Message
		{
			get { return Record.Message; }
			set { Record.Message = value; }
		}

		[DisplayName("Thankyou message")]
		public String ThankyouMessage
		{
			get { return Record.ThankyouMessage; }
			set { Record.ThankyouMessage = value; }
		}

		public IEnumerable<MergeVariableRecord> MergeVariables
		{
			get { return Record.MergeVariables; }
		}

		public IEnumerable<InterestGroupingsRecord> InterestGroups
		{ 
			get { return Record.InterestGroupings; }
		}
	}
}