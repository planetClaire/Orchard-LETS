using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NogginBox.MailChimp.Models;
using System.Web.Mvc;

namespace NogginBox.MailChimp.ViewModels
{
	public class MergeVariableModel
	{
		public MergeVariableModel()
		{
			Value = Message = "";
			Fields = new Dictionary<string, string>();
		}

		public MergeVariableRecord Variable { get; set; }
		
		public String Value { get; set; }

		public Dictionary<String, String> Fields { get; private set; }

		public String Message { get; set; }

		public DisplayType DisplayType
		{
			get
			{
				var vType = (Variable.Type != null) ? (MergeVariableType)Variable.Type : MergeVariableType.Text;
				switch (vType)
				{
					case MergeVariableType.Multi_choice:
						return DisplayType.Radio;
					case MergeVariableType.Drop_down:
						return DisplayType.DropDown;
					case MergeVariableType.Address:
						return DisplayType.TextArea;
					default:
						return DisplayType.Text;
				}
			}
		}

		public SelectList Countries { get; set; }

		public String getField(String key)
		{
			return (Fields.ContainsKey(key)) ? Fields[key] : null;
		}
	}

	public enum DisplayType
	{
		Text, Radio, DropDown, TextArea
	}
}