﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Security.Permissions;

namespace Orchard.Core.Common {
    public class Permissions : IPermissionProvider {
        public static Permission ChangeOwner = new Permission { Name = "ChangeOwner", Description = "Change the owner of content items" };

        public string PackageName {
            get { return "Common"; }
        }

        public IEnumerable<Permission> GetPermissions() {
            return new[] { ChangeOwner };
        }
    }
}