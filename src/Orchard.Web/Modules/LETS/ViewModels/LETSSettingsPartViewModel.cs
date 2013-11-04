using System.Collections.Generic;
using Orchard.Taxonomies.Models;
using LETS.Models;
using Orchard.Roles.Models;

namespace LETS.ViewModels
{
    public class LETSSettingsPartViewModel
    {
        public LETSSettingsPart LETSSettings { get; set; }
        public IEnumerable<RoleRecord> Roles { get; set; }
        public IEnumerable<TaxonomyPart> Taxonomies { get; set; }
        public IEnumerable<MemberPart> AdminMembers { get; set; }
    }
}