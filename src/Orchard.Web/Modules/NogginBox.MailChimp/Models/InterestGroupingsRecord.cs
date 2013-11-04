using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NogginBox.MailChimp.Models
{
	public class InterestGroupingsRecord
	{
		public virtual int Id { get; set; }

		public virtual int GroupId { get; set; }

		public virtual String Name { get; set; }

		public virtual String Type { get; set; }

		public virtual String Groups { get; set; }

		public virtual bool Show { get; set; }

		public virtual FormRecord FormRecord { get; set; }

		public virtual List<String> getGroups()
		{
			return Groups.Split(',').ToList();
		}

		public virtual void setGroups(List<String> groups)
		{
			Groups = String.Join(",", groups);
		}
	}
}