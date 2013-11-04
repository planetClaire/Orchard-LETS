using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard;
using Orchard.Comments.Models;

namespace LETS.Services
{
    public interface ILETSService : IDependency {
        IEnumerable<CommentPart> GetLatestComments(int count);
    }
}