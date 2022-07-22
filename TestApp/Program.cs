using GoogleMapsApi;
using GoogleMapsApi.Entities.Common;
using GoogleMapsApi.Entities.Directions.Request;
using GoogleMapsApi.Entities.Directions.Response;
using GoogleMapsApi.Entities.Geocoding.Request;
using GoogleMapsApi.Entities.Geocoding.Response;
using GoogleMapsApi.Entities.SnapToRoad.Request;
using GoogleMapsApi.Entities.SnapToRoad.Response;
using GoogleMapsApi.StaticMaps;
using GoogleMapsApi.StaticMaps.Entities;
using GoogleMapsApi.StaticMaps.Enums;
using System;

namespace Hackathon
{
    class Program
    {
		static  void Main(string[] args)
        {
			Task.Run(() => Test2()).Wait();
			Console.ReadLine();
        }

		public static async Task Test2()
        {
			//Static class use (Directions) (Can be made from static/instance class)
			DirectionsRequest directionsRequest = new DirectionsRequest()
			{
				Origin = "NYC, 5th and 39",
				Destination = "Philladephia, Chesnut and Wallnut",
				ApiKey = "AIzaSyDikeBAymgSWrWz-9Y7Danr2mNewZV_MwI",
				TravelMode = TravelMode.Bicycling
			};

			DirectionsResponse directions = await GoogleMaps.Directions.QueryAsync(directionsRequest);
			Console.WriteLine(directions);

			// Static maps API - get static map of with the path of the directions request
			StaticMapsEngine staticMapGenerator = new StaticMapsEngine();

			//Path from previos directions request
			IEnumerable<Step> steps = directions.Routes.First().Legs.First().Steps;
			// All start locations
			IList<ILocationString> path = steps.Select(step => step.StartLocation).ToList<ILocationString>();
			// also the end location of the last step
			path.Add(steps.Last().EndLocation);

			SnapToRoadRequest snapToRoadRequest = new SnapToRoadRequest()
			{
				ApiKey = "AIzaSyDikeBAymgSWrWz-9Y7Danr2mNewZV_MwI",
				Path = path
			};

			SnapToRoadResponse snaps = await GoogleMaps.SnapToRoad.QueryAsync(snapToRoadRequest);
			IList<ILocationString> path2 = snaps.SnappedPoints.Select(step => new Location(step.Location.Latitude, step.Location.Longitude)).ToList<ILocationString>();
			;

			string url = staticMapGenerator.GenerateStaticMapURL(new StaticMapRequest(13, new ImageSize(1000, 1000))
			{
				Pathes = new List<GoogleMapsApi.StaticMaps.Entities.Path>(){ new GoogleMapsApi.StaticMaps.Entities.Path()
	{
			Style = new PathStyle()
			{
					Color = "red"
			},
			Locations = path
	}},
				ApiKey = "AIzaSyDikeBAymgSWrWz-9Y7Danr2mNewZV_MwI"
			});
			Console.WriteLine("Map with path: " + url);
			string url2 = staticMapGenerator.GenerateStaticMapURL(new StaticMapRequest( 13, new ImageSize(1000, 1000))
			{
				Pathes = new List<GoogleMapsApi.StaticMaps.Entities.Path>(){ new GoogleMapsApi.StaticMaps.Entities.Path()
	{
			Style = new PathStyle()
			{
					Color = "red"
			},
			Locations = path2
	}},
				ApiKey = "AIzaSyDikeBAymgSWrWz-9Y7Danr2mNewZV_MwI"
			});
			;
		}

		public static async Task Test()
        {
			StaticMapRequest request = new(new AddressLocation("Brooklyn Bridge,New York,NY"), 14, new ImageSize(512, 512))
			{
				MapType = MapType.Roadmap,
				Markers =
						new List<Marker>
							{
								new Marker
									{
										Style = new MarkerStyle { Color = "blue", Label = "S" },
										Locations = new List<ILocationString> { new Location(40.702147, -74.015794) }
									},
								new Marker
									{
										Style = new MarkerStyle { Color = "green", Label = "G" },
										Locations = new List<ILocationString> { new Location(40.711614, -74.012318) }
									},
								new Marker
									{
										Style = new MarkerStyle { Color = "red", Label = "C" },
										Locations = new List<ILocationString> { new Location(40.718217, -73.998284) }
									}
							}
			};
			request.ApiKey = "AIzaSyDikeBAymgSWrWz-9Y7Danr2mNewZV_MwI";
			string expectedResult = "http://maps.google.com/maps/api/staticmap" +
									"?center=Brooklyn%20Bridge%2CNew%20York%2CNY&zoom=14&size=512x512&maptype=roadmap" +
									"&markers=color%3Ablue%7Clabel%3AS%7C40.702147%2C-74.015794&markers=color%3Agreen%7Clabel%3AG%7C40.711614%2C-74.012318" +
									"&markers=color%3Ared%7Clabel%3AC%7C40.718217%2C-73.998284";

			string generateStaticMapURL = new StaticMapsEngine().GenerateStaticMapURL(request);

			var requestDirection = new DirectionsRequest { Origin = "Odessa, Ukraine", Destination = "Kiev, Ukraine" };
			requestDirection.ApiKey = "AIzaSyDikeBAymgSWrWz-9Y7Danr2mNewZV_MwI";
			var result = await GoogleMaps.Directions.QueryAsync(requestDirection);
			
			GoogleMapsApi.StaticMaps.Entities.Path path = new GoogleMapsApi.StaticMaps.Entities.Path
			{
				Locations = new List<ILocationString>(),
				Style = new PathStyle
				{
					Color = "purple"
				}

			};

			foreach (var route in result.Routes)
            {
				foreach(var legs in route.Legs)
                {
					foreach(var step in legs.Steps)
                    {
						path.Locations.Add(new Location(step.StartLocation.Latitude, step.StartLocation.Longitude));
						path.Locations.Add(new Location(step.EndLocation.Latitude, step.EndLocation.Longitude));
						
                    }
                }
            }

			StaticMapRequest request2 = new(1, new ImageSize(1000, 1000))
			{
				Pathes = new List<GoogleMapsApi.StaticMaps.Entities.Path>() { path }
			};
			request2.ApiKey = "AIzaSyDikeBAymgSWrWz-9Y7Danr2mNewZV_MwI";
			generateStaticMapURL = new StaticMapsEngine().GenerateStaticMapURL(request2);
			;
		}
    }
}
