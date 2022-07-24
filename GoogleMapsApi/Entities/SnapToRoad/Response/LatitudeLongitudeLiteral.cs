using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GoogleMapsApi.Entities.SnapToRoad.Response
{
	[DataContract]
	public class LatitudeLongitudeLiteral
	{
		[DataMember(Name = "latitude")]
		public double Latitude { get; set; }

		[DataMember(Name = "longitude")]
		public double Longitude { get; set; }
	}
}
