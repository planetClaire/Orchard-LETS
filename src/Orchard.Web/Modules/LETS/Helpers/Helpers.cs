using System.Text.RegularExpressions;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;

namespace LETS.Helpers
{
    public static class Helpers
    {
        public static bool IsPublished(IContent content)
        {
            var common = content.As<ICommonPart>();
            return common.IsPublished();
        }

        public static string Linkify(string text)
        {
            // linkify urls http://stackoverflow.com/questions/758135/c-sharp-code-to-linkify-urls-in-a-string
            return Regex.Replace(text, @"((http|https|ftp)\://[a-zA-Z0-9\-\.]+\.[a-zA-Z]{2,3}(:[a-zA-Z0-9]*)?/?([a-zA-Z‌​0-9\-\._\?\,\'/\\\+&amp;%\$#\=~])*)", @"<a href='$1' target='_blank'>$1</a>");
        }

    }
}