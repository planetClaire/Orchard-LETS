using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace Orchard.Comments.Models {
    [UsedImplicitly]
    public class CommentDriver : ContentItemDriver<Comment> {
        public readonly static ContentType ContentType = new ContentType {
                                                                             Name = "comment",
                                                                             DisplayName = "Comment"
                                                                         };

        protected override ContentType GetContentType() {
            return ContentType;
        }

        protected override string Prefix { get { return ""; } }
    }
}