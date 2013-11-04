using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NogginBox.MailChimp.Models;
using System.Collections.Specialized;

namespace NogginBox.MailChimp
{
	public static class Helpers
	{
		public static HtmlString ShowVarType(this HtmlHelper html, int? type)
		{
			if(Enum.IsDefined(typeof(MergeVariableType), (MergeVariableType)type))
			{
				var typename = Enum.GetName(typeof(MergeVariableType), (MergeVariableType)type);
				return new HtmlString(typename.Replace('_', ' '));
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Convert a NameValueCollection into a KeyValue pair so it is a generic IEnumerable and accessible to Linq
		/// </summary>
		/// <param name="collection">A NameValueCollection</param>
		/// <returns></returns>
		/// <remarks>Source: http://stackoverflow.com/questions/391023/make-namevaluecollection-accessible-to-linq-query/396504#396504 </remarks>
		public static IEnumerable<KeyValuePair<string, string>> ToPairs(this NameValueCollection collection)
		{
			if (collection == null)
			{
				return null;
			}

			return collection.Cast<string>().Select(key => new KeyValuePair<string, string>(key, collection[key]));
		}

	}
}