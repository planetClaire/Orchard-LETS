using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace LETS
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission AdminMemberContent = new Permission { Description = "Administer member content like member lists, notices etc", Name = "AdminMemberContent" };
        public static readonly Permission AccessMemberContent = new Permission { Description = "Access member content like member lists, notices etc", Name = "AccessMemberContent", ImpliedBy = new[] { AdminMemberContent } };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] {
                AccessMemberContent,
                AdminMemberContent
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            var permissionViewProfiles = Contrib.Profile.Permissions.ViewProfiles;
            var permissionPublishOwnContent = Orchard.Core.Contents.Permissions.PublishOwnContent;
            var permissionDeleteOwnContent = Orchard.Core.Contents.Permissions.DeleteOwnContent;
            return new[]
                       {
                           new PermissionStereotype
                               {
                                   Name = "Administrator",
                                   Permissions = new[] {AdminMemberContent}
                               },
                           new PermissionStereotype
                               {
                                   Name = "Member",
                                   Permissions = new[] {AccessMemberContent, permissionViewProfiles, permissionPublishOwnContent, permissionDeleteOwnContent}
                               }
                       };
        }

    }
}