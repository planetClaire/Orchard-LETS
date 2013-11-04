using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NogginBox.MailChimp.Models;

namespace NogginBox.MailChimp.ViewModels
{
	public class EditFormModel
	{
		public String ListId { get; set; }

		public string Message { get; set; }

		public String ThankyouMessage { get; set; }

		public IList<MergeVariableEntry> MergeVariables { get; set; }

		public bool HasApiKey { get; set; }

		public Dictionary<String, String> PossibleLists { get; set; }

		public IEnumerable<MergeVariableRecord> AvailableMergeVariables { get; set; }

		public List<InterestGroupingsRecord> InterestGroups { get; set; }
	}

	public class MergeVariableEntry
	{
		public bool Checked { get; set; }
		public String Tag { get; set; }
	}
}