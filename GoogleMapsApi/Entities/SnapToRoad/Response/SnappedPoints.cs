using GoogleMapsApi.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GoogleMapsApi.Entities.SnapToRoad.Response
{
    [DataContract]
    public class SnappedPoint
    {
        [DataMember(Name = "location")]
        public LatitudeLongitudeLiteral Location { get; set; } // Required

        [DataMember(Name = "placeId")]
        public string PlaceId { get; set; } //Required

        [DataMember(Name = "originalIndex")]
        public string OriginalIndex { get; set; }
        
    }
}
