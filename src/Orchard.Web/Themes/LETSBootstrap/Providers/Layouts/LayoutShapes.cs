﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Orchard;
using Orchard.DisplayManagement;
using Orchard.Localization;

namespace LETSBootstrap.Providers.Layouts {
    public class LayoutShapes : IDependency {
        public LayoutShapes() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        [Shape]
        public void Carousel(dynamic Display, TextWriter Output, HtmlHelper Html, string Id, IEnumerable<dynamic> Items, IEnumerable<string> OuterClasses, IDictionary<string, string> OuterAttributes, IEnumerable<string> InnerClasses, IDictionary<string,string> InnerAttributes, IEnumerable<string> FirstItemClasses, IDictionary<string, string> FirstItemAttributes, IEnumerable<string> ItemClasses, IDictionary<string, string> ItemAttributes) {
            if (Items == null) return;

            var items = Items.ToList();
            var itemsCount = items.Count;

            if (itemsCount < 1) return;

            var outerDivTag = GetTagBuilder("div", Id, OuterClasses, OuterAttributes);
            var innerDivTag = GetTagBuilder("div", string.Empty, InnerClasses, InnerAttributes);
            var firstItemTag = GetTagBuilder("div", string.Empty, FirstItemClasses, FirstItemAttributes);
            var itemTag = GetTagBuilder("div", string.Empty, ItemClasses, ItemAttributes);

            Output.Write(outerDivTag.ToString(TagRenderMode.StartTag));
            Output.Write(innerDivTag.ToString(TagRenderMode.StartTag));

            int i = 0;

            foreach (var item in items) {
                if (i == 0)
                    Output.Write(firstItemTag.ToString(TagRenderMode.StartTag));
                else
                    Output.Write(itemTag.ToString(TagRenderMode.StartTag));

                Output.Write(Display(item));
                Output.Write(itemTag.ToString(TagRenderMode.EndTag));
                i++;
            }

            Output.Write(innerDivTag.ToString(TagRenderMode.EndTag));

            Output.Write("<a href=\"#{0}\" class=\"carousel-control left\" data-slide=\"prev\">&lsaquo;</a>", Id);
            Output.Write("<a href=\"#{0}\" class=\"carousel-control right\" data-slide=\"next\">&rsaquo;</a>", Id);

            Output.Write(outerDivTag.ToString(TagRenderMode.EndTag));

            //Output.Write("<script type='text/javascript'>$(document).ready(function () {$('.carousel').carousel()});</script>");
            // disable automatic scrolling, doesn't slide in IE9, just pops. interval:false doesn't work
            Output.Write("<script type='text/javascript'>$(document).ready(function () {$('.carousel').carousel();$('.carousel').carousel('pause');$('.carousel').off('mouseenter').off('mouseleave');});</script>");
        }
            
        

        static TagBuilder GetTagBuilder(string tagName, string id, IEnumerable<string> classes, IDictionary<string, string> attributes) {
            var tagBuilder = new TagBuilder(tagName);
            tagBuilder.MergeAttributes(attributes, false);
            foreach (var cssClass in classes ?? Enumerable.Empty<string>())
                tagBuilder.AddCssClass(cssClass);
            if (!string.IsNullOrWhiteSpace(id))
                tagBuilder.GenerateId(id);
            return tagBuilder;
        }

    }
}
