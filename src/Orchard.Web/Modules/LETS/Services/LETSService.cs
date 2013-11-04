using System.Collections.Generic;
using System.Linq;
using Orchard.Comments.Models;
using Orchard.Comments.Services;
using Orchard.ContentManagement;

namespace LETS.Services
{
    public class LETSService : ILETSService
    {
        private readonly ICommentService _commentService;
        private readonly IContentManager _contentManager;

        public LETSService(ICommentService commentService, IContentManager contentManager) {
            _commentService = commentService;
            _contentManager = contentManager;
        }

        public IEnumerable<CommentPart> GetLatestComments(int count) {
            var query = _commentService.GetComments();// "Get is not implemented" .Where(c => _contentManager.Get<IContent>(c.CommentedOn) != null);
            var commentsOnPublishedItems = query.List<CommentPart>().Where(c => _contentManager.Get(c.Record.CommentedOn) != null);
            var comments = commentsOnPublishedItems.OrderByDescending(c => c.Record.CommentDateUtc).Take(count).ToList();
            //var comments = query.OrderByDescending(c => c.CommentDateUtc).Slice(0, count);
            return comments;
        }
    }
}