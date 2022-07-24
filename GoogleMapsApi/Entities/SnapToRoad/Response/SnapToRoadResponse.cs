using GoogleMapsApi.Entities.Common;
using GoogleMapsApi.Entities.SnapToRoad.Request;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GoogleMapsApi.Entities.SnapToRoad.Response
{
    [DataContract(Name = "SnapToRoadResponse")]
    public class SnapToRoadResponse : IResponseFor<SnapToRoadRequest>
    {
        [DataMember(Name = "snappedPoints")]
        public IEnumerable<SnappedPoint> SnappedPoints { get; set; }

        [DataMember(Name = "warningMessage")]
        public string WarningMessage { get; set; }
    }
}
