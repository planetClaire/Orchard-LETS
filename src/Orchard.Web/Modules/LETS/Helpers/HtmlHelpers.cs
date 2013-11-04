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
                    var radio = htmlHelper.RadioButtonFor(expression, item.Value, new {id}).ToHtmlString();
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

        // optgroup dropdown methods from http://stackoverflow.com/questions/607188/support-for-optgroup-in-dropdownlist-net-mvc/4248856#4248856

        public static MvcHtmlString DropDownGroupList(this HtmlHelper htmlHelper, string name)
        {
            return DropDownListHelper(htmlHelper, name, null, null, null);
        }

        public static MvcHtmlString DropDownGroupList(this HtmlHelper htmlHelper, string name,
                                                     IEnumerable<GroupedSelectListItem> selectList)
        {
            return DropDownListHelper(htmlHelper, name, selectList, null, null);
        }

        public static MvcHtmlString DropDownGroupList(this HtmlHelper htmlHelper, string name, string optionLabel)
        {
            return DropDownListHelper(htmlHelper, name, null, optionLabel, null);
        }

        public static MvcHtmlString DropDownGroupList(this HtmlHelper htmlHelper, string name,
                                                     IEnumerable<GroupedSelectListItem> selectList,
                                                     IDictionary<string, object> htmlAttributes)
        {
            return DropDownListHelper(htmlHelper, name, selectList, null, htmlAttributes);
        }

        public static MvcHtmlString DropDownGroupList(this HtmlHelper htmlHelper, string name,
                                                     IEnumerable<GroupedSelectListItem> selectList,
                                                     object htmlAttributes)
        {
            return DropDownListHelper(htmlHelper, name, selectList, null, new RouteValueDictionary(htmlAttributes));
        }

        public static MvcHtmlString DropDownGroupList(this HtmlHelper htmlHelper, string name,
                                                     IEnumerable<GroupedSelectListItem> selectList, string optionLabel)
        {
            return DropDownListHelper(htmlHelper, name, selectList, optionLabel, null);
        }

        public static MvcHtmlString DropDownGroupList(this HtmlHelper htmlHelper, string name,
                                                     IEnumerable<GroupedSelectListItem> selectList, string optionLabel,
                                                     IDictionary<string, object> htmlAttributes)
        {
            return DropDownListHelper(htmlHelper, name, selectList, optionLabel, htmlAttributes);
        }

        public static MvcHtmlString DropDownGroupList(this HtmlHelper htmlHelper, string name,
                                                     IEnumerable<GroupedSelectListItem> selectList, string optionLabel,
                                                     object htmlAttributes)
        {
            return DropDownListHelper(htmlHelper, name, selectList, optionLabel,
                                      new RouteValueDictionary(htmlAttributes));
        }

        public static MvcHtmlString DropDownGroupListFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper,
                                                                            Expression<Func<TModel, TProperty>>
                                                                                expression,
                                                                            IEnumerable<GroupedSelectListItem>
                                                                                selectList)
        {
            return DropDownGroupListFor(htmlHelper, expression, selectList, null /* optionLabel */, null
                /* htmlAttributes */);
        }

        public static MvcHtmlString DropDownGroupListFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper,
                                                                            Expression<Func<TModel, TProperty>>
                                                                                expression,
                                                                            IEnumerable<GroupedSelectListItem>
                                                                                selectList, object htmlAttributes)
        {
            return DropDownGroupListFor(htmlHelper, expression, selectList, null /* optionLabel */,
                                        new RouteValueDictionary(htmlAttributes));
        }

        public static MvcHtmlString DropDownGroupListFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper,
                                                                            Expression<Func<TModel, TProperty>>
                                                                                expression,
                                                                            IEnumerable<GroupedSelectListItem>
                                                                                selectList,
                                                                            IDictionary<string, object> htmlAttributes)
        {
            return DropDownGroupListFor(htmlHelper, expression, selectList, null /* optionLabel */, htmlAttributes);
        }

        public static MvcHtmlString DropDownGroupListFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper,
                                                                            Expression<Func<TModel, TProperty>>
                                                                                expression,
                                                                            IEnumerable<GroupedSelectListItem>
                                                                                selectList, string optionLabel)
        {
            return DropDownGroupListFor(htmlHelper, expression, selectList, optionLabel, null /* htmlAttributes */);
        }

        public static MvcHtmlString DropDownGroupListFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper,
                                                                            Expression<Func<TModel, TProperty>>
                                                                                expression,
                                                                            IEnumerable<GroupedSelectListItem>
                                                                                selectList, string optionLabel,
                                                                            object htmlAttributes)
        {
            return DropDownGroupListFor(htmlHelper, expression, selectList, optionLabel,
                                        new RouteValueDictionary(htmlAttributes));
        }

        public static MvcHtmlString DropDownGroupListFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper,
                                                                            Expression<Func<TModel, TProperty>>
                                                                                expression,
                                                                            IEnumerable<GroupedSelectListItem>
                                                                                selectList, string optionLabel,
                                                                            IDictionary<string, object> htmlAttributes)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            // fixing clientside validation attributes
            // http://stackoverflow.com/questions/4799958/asp-net-mvc-3-unobtrusive-client-validation-does-not-work-with-drop-down-lists/8102022#8102022
            var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
                var mergedAttributes =
                    htmlHelper.GetUnobtrusiveValidationAttributes(ExpressionHelper.GetExpressionText(expression), metadata);

                if (htmlAttributes != null)
                {
                    //foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(htmlAttributes))
                    //{
                    //    object value = descriptor.GetValue(htmlAttributes);
                    //    mergedAttributes.Add(descriptor.Name, value);
                    //}
                    foreach (var htmlAttribute in htmlAttributes) {
                        mergedAttributes.Add(htmlAttribute.Key, htmlAttribute.Value);
                    }
                }

            //return DropDownListHelper(htmlHelper, ExpressionHelper.GetExpressionText(expression), selectList, optionLabel, htmlAttributes);
            return DropDownListHelper(htmlHelper, ExpressionHelper.GetExpressionText(expression), selectList, optionLabel, mergedAttributes);
        }

        private static MvcHtmlString DropDownListHelper(HtmlHelper htmlHelper, string expression,
                                                        IEnumerable<GroupedSelectListItem> selectList,
                                                        string optionLabel, IDictionary<string, object> htmlAttributes)
        {


            return SelectInternal(htmlHelper, optionLabel, expression, selectList, false /* allowMultiple */,
                          htmlAttributes);
        }


        // Helper methods

        private static IEnumerable<GroupedSelectListItem> GetSelectData(this HtmlHelper htmlHelper, string name)
        {
            object o = null;
            if (htmlHelper.ViewData != null)
            {
                o = htmlHelper.ViewData.Eval(name);
            }
            if (o == null)
            {
                throw new InvalidOperationException(
                    String.Format(
                        CultureInfo.CurrentCulture,
                        "Missing Select Data",
                        name,
                        "IEnumerable<GroupedSelectListItem>"));
            }
            IEnumerable<GroupedSelectListItem> selectList = o as IEnumerable<GroupedSelectListItem>;
            if (selectList == null)
            {
                throw new InvalidOperationException(
                    String.Format(
                        CultureInfo.CurrentCulture,
                        "Wrong Select DataType",
                        name,
                        o.GetType().FullName,
                        "IEnumerable<GroupedSelectListItem>"));
            }
            return selectList;
        }

        internal static string ListItemToOption(GroupedSelectListItem item)
        {
            TagBuilder builder = new TagBuilder("option")
                                     {
                                         InnerHtml = HttpUtility.HtmlEncode(item.Text)
                                     };
            if (item.Value != null)
            {
                builder.Attributes["value"] = item.Value;
            }
            if (item.Selected)
            {
                builder.Attributes["selected"] = "selected";
            }
            return builder.ToString(TagRenderMode.Normal);
        }

        private static MvcHtmlString SelectInternal(this HtmlHelper htmlHelper, string optionLabel, string name,
                                                    IEnumerable<GroupedSelectListItem> selectList, bool allowMultiple,
                                                    IDictionary<string, object> htmlAttributes)
        {
            name = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(name);
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Null Or Empty", "name");
            }

            bool usedViewData = false;

            // If we got a null selectList, try to use ViewData to get the list of items.
            if (selectList == null)
            {
                selectList = htmlHelper.GetSelectData(name);
                usedViewData = true;
            }

            object defaultValue = (allowMultiple)
                                      ? htmlHelper.GetModelStateValue(name, typeof (string[]))
                                      : htmlHelper.GetModelStateValue(name, typeof (string));

            // If we haven't already used ViewData to get the entire list of items then we need to
            // use the ViewData-supplied value before using the parameter-supplied value.
            if (!usedViewData)
            {
                if (defaultValue == null)
                {
                    defaultValue = htmlHelper.ViewData.Eval(name);
                }
            }

            if (defaultValue != null)
            {
                IEnumerable defaultValues = (allowMultiple) ? defaultValue as IEnumerable : new[] {defaultValue};
                IEnumerable<string> values = from object value in defaultValues
                                             select Convert.ToString(value, CultureInfo.CurrentCulture);
                HashSet<string> selectedValues = new HashSet<string>(values, StringComparer.OrdinalIgnoreCase);
                List<GroupedSelectListItem> newSelectList = new List<GroupedSelectListItem>();

                foreach (GroupedSelectListItem item in selectList)
                {
                    item.Selected = (item.Value != null)
                                        ? selectedValues.Contains(item.Value)
                                        : selectedValues.Contains(item.Text);
                    newSelectList.Add(item);
                }
                selectList = newSelectList;
            }

            // Convert each ListItem to an <option> tag
            StringBuilder listItemBuilder = new StringBuilder();

            // Make optionLabel the first item that gets rendered.
            if (optionLabel != null)
            {
                listItemBuilder.AppendLine(
                    ListItemToOption(new GroupedSelectListItem()
                                         {Text = optionLabel, Value = String.Empty, Selected = false}));
            }

            foreach (var group in selectList.GroupBy(i => i.GroupKey))
            {
                string groupName =
                    selectList.Where(i => i.GroupKey == group.Key).Select(it => it.GroupName).FirstOrDefault();
                listItemBuilder.AppendLine(string.Format("<optgroup label=\"{0}\" value=\"{1}\">", groupName, group.Key));
                foreach (GroupedSelectListItem item in group)
                {
                    listItemBuilder.AppendLine(ListItemToOption(item));
                }
                listItemBuilder.AppendLine("</optgroup>");
            }

            TagBuilder tagBuilder = new TagBuilder("select")
                                        {
                                            InnerHtml = listItemBuilder.ToString()
                                        };
            tagBuilder.MergeAttributes(htmlAttributes);
            tagBuilder.MergeAttribute("name", name, true /* replaceExisting */);
            tagBuilder.GenerateId(name);
            if (allowMultiple)
            {
                tagBuilder.MergeAttribute("multiple", "multiple");
            }

            // If there are any errors for a named field, we add the css attribute.
            ModelState modelState;
            if (htmlHelper.ViewData.ModelState.TryGetValue(name, out modelState))
            {
                if (modelState.Errors.Count > 0)
                {
                    tagBuilder.AddCssClass(HtmlHelper.ValidationInputCssClassName);
                }
            }

            return MvcHtmlString.Create(tagBuilder.ToString());

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
