using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NogginBox.MailChimp.ViewModels
{
	public class InterestGroupsModel
	{
		public InterestGroupsModel()
		{
			Groups = new List<InterestGroupItemModel>();
		}

		public int GroupId { get; set; }
		public String Name { get; set; }
		public List<InterestGroupItemModel> Groups { get; private set; }

		public void AddAllGroupItems(List<String> groupNames)
		{
			foreach (var groupName in groupNames)
			{
				Groups.Add(new InterestGroupItemModel { Name = groupName });
			}
		}
	}

	public class InterestGroupItemModel
	{
		public bool Checked { get; set; }
		public String Name { get; set; }
	}
}