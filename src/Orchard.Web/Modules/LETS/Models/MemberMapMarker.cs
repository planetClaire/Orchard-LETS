using System;

namespace LETS.Models {
    [Serializable]
    public class MemberMapMarker {
        public string LatLong { get; set; }
        public string InfoHtml { get; set; }
    }
}