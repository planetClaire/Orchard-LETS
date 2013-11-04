using System;
using System.Linq;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Rules.Models;
using Orchard.Rules.Services;

namespace LETS.Rules
{
    [UsedImplicitly]
    public class LETSEvents : IEventProvider
    {
        private readonly Localizer _t;

        public LETSEvents()
        {
            _t = NullLocalizer.Instance;
        }

        public void Describe(DescribeEventContext describe)
        {
            Func<dynamic, bool> contentHasPart = ContentHasPart;

            describe.For("Content", _t("Content Items"), _t("Content Items"))
                .Element("Archived", _t("Content Archived"), _t("Content is actually unpublished via the archive later module."), contentHasPart,
                         (Func<dynamic, LocalizedString>)
                         (context => _t("When content with types ({0}) is unpublished via the archive later module.", FormatPartsList(context))),
                         "SelectContentTypes");
        }

        private string FormatPartsList(dynamic context)
        {
            var contenttypes = context.Properties["contenttypes"];

            if (String.IsNullOrEmpty(contenttypes))
            {
                return _t("Any").Text;
            }

            return contenttypes;
        }

        private static bool ContentHasPart(dynamic context)
        {
            string contenttypes = context.Properties["contenttypes"];
            var content = context.Tokens["Content"] as IContent;

            // "" means 'any'
            if (String.IsNullOrEmpty(contenttypes))
            {
                return true;
            }

            if (content == null)
            {
                return false;
            }

            var contentTypes = contenttypes.Split(new[] { ',' });

            return contentTypes.Any(contentType => content.ContentItem.TypeDefinition.Name == contentType);
        }

    }
}