using System.Collections.Generic;
using LETS.Models;
using Orchard;

namespace LETS.Services
{
    public interface IMembersMapService : IDependency
    {
        IEnumerable<MemberMapMarker> MemberMarkers();
    }
}