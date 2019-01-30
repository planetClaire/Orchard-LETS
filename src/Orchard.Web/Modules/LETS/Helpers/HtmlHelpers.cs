using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using Orchard.Mvc.Html;

namespace LETS.Helpers
{
    public static class HtmlHelpers
    {
        /// <summary>
        /// enable adding html Attributes to form
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="htmlAttributes"></param>
        /// <returns></returns>
        public static MvcForm BeginFormAntiForgeryPost(this HtmlHelper htmlHelper, object htmlAttributes)
        {
            return htmlHelper.BeginFormAntiForgeryPost(htmlHelper.ViewContext.HttpContext.Request.Url.PathAndQuery, FormMethod.Post, new RouteValueDictionary(htmlAttributes));
        }

        public static MvcHtmlString RadioButtonForSelectList<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper,
                                                                                Expression<Func<TModel, TProperty>>
                                                                                    expression,
                                                                                IEnumerable<SelectListItem> listOfValues)
        {
            var metaData = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var sb = new StringBuilder();

            if (listOfValues != null)
            {
                sb.Append("<div class=\"radioButtonList\">");
                foreach (var item in listOfValues)
                {
                    var id = string.Format("{0}_{1}", metaData.PropertyName, item.Value);
                    var radio = htmlHelper.RadioButtonFor(expression, item.Value, new { id }).ToHtmlString();
                    sb.AppendFormat("<label class=\"radioButton\">{0}{1}</label>", radio,
                                    HttpUtility.HtmlEncode(item.Text));
                }
                sb.Append(htmlHelper.HiddenFor(expression).ToHtmlString());
                sb.Append("</div>");
            }
            return MvcHtmlString.Create(sb.ToString());
        }

        /// <summary>
        /// fixes MVC3 bug where client validation attributes aren't emitted
        /// http://stackoverflow.com/questions/4799958/asp-net-mvc-3-unobtrusive-client-validation-does-not-work-with-drop-down-lists/8102022#8102022
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="helper"></param>
        /// <param name="expression"></param>
        /// <param name="items"></param>
        /// <param name="blankOption"></param>
        /// <param name="htmlAttributes"></param>
        /// <returns></returns>
        public static IHtmlString DropDownListWithClientValidationFor<TModel, TProperty>(this HtmlHelper<TModel> helper,
                                                                                         Expression
                                                                                             <Func<TModel, TProperty>>
                                                                                             expression,
                                                                                         IEnumerable<SelectListItem>
                                                                                             items, string blankOption,
                                                                                         object htmlAttributes = null)
        {
            var metadata = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
            var mergedAttributes =
                helper.GetUnobtrusiveValidationAttributes(ExpressionHelper.GetExpressionText(expression), metadata);

            if (htmlAttributes != null)
            {
                foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(htmlAttributes))
                {
                    object value = descriptor.GetValue(htmlAttributes);
                    mergedAttributes.Add(descriptor.Name, value);
                }
            }

            return
                MvcHtmlString.Create(helper.DropDownListFor(expression, items, blankOption, mergedAttributes).ToString());
        }

        public static string IfSelected(this HtmlHelper html, string action, string controller)
        {
            var routeData = html.ViewContext.RouteData.Values;
            return routeData["action"].ToString().ToLower() == action.ToLower() && routeData["controller"].ToString().ToLower() == controller.ToLower() ? " active" : "";
        }

        internal static object GetModelStateValue(this HtmlHelper helper, string key, Type destinationType)
        {
            ModelState modelState;
            if (helper.ViewData.ModelState.TryGetValue(key, out modelState))
            {
                if (modelState.Value != null)
                {
                    return modelState.Value.ConvertTo(destinationType, null /* culture */);
                }
            }
            return null;
        }

    }
}
