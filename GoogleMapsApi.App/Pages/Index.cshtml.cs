using GoogleMapsApi.Entities.Common;
using GoogleMapsApi.Entities.Directions.Request;
using GoogleMapsApi.Entities.Directions.Response;
using GoogleMapsApi.Entities.SnapToRoad.Request;
using GoogleMapsApi.Entities.SnapToRoad.Response;
using GoogleMapsApi.StaticMaps;
using GoogleMapsApi.StaticMaps.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GoogleMapsApi.App.Pages
{
    public class IndexModel : PageModel
    {
        string baseDynamic = "https://www.google.com/maps/embed/v1/directions?";

        public bool CanShow = false;

        string MarshrutLineColor = "#340573";

        Dictionary<string, TravelMode> dictionaryMode = new Dictionary<string, TravelMode>
        {
            {"car",  TravelMode.Driving},
            {"bicycle",  TravelMode.Bicycling},
            {"walk",  TravelMode.Walking}
        };
        public List<(string Description, string Src)> Steps { get; set; }
        DirectionsResponse directions { get; set; }
        public string ImagePolylineSrc { set; get; }
        public string ImageSnapPolylineSrc { set; get; }
        public string ImagePathSrc { set; get; }
        public string ImageSnapPathSrc { set; get; }
        public string ImageDynamicSrc { set; get; }
        public string ErrorMessage { get; set; }
        private readonly ILogger<IndexModel> _logger;
        string ApiKey
        {
            get { return "AIzaSyDikeBAymgSWrWz-9Y7Danr2mNewZV_MwI"; }
        }
        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }

        public void OnPost(string addresTo, string addresFrom, string select)
        {
            ErrorMessage = "";
            if (!String.IsNullOrEmpty(addresTo) && !String.IsNullOrEmpty(addresFrom))
            {
                Task.Run(() => GetDirectionResponse(addresTo, addresFrom, select)).Wait();
                ImagePolylineSrc = GetStaticMapByPoints(RemoveExtraPoints(GetStaticMapByPolyline()), MarshrutLineColor);
                ImagePathSrc = GetStaticMapByPoints(RemoveExtraPoints(GetStaticMapByStep()), MarshrutLineColor);
                var snapPath = Task.Run(()=> GetStaticMapBySnapStep());
                ImageSnapPathSrc = GetStaticMapByPoints(RemoveExtraPoints(snapPath.Result), MarshrutLineColor);
                ImageSnapPolylineSrc = GetStaticMapByPoints(RemoveExtraPoints((Task.Run(() => GetStaticMapBySnapPolyline())).Result), MarshrutLineColor);
                string[] separator = { " ", ",", ".", "-" };
                var options = String.Join('+', addresFrom.Split(separator, StringSplitOptions.RemoveEmptyEntries));
                var destination = String.Join('+', addresTo.Split(separator, StringSplitOptions.RemoveEmptyEntries));
                ImageDynamicSrc = $"{baseDynamic}key={ApiKey}&origin={options}&destination={destination}";

                CanShow = true;
            }
            else
                ErrorMessage = "Введите откуда и куда необходимо построить маршрут";
        }

        public async Task GetDirectionResponse(string addresTo, string addresFrom, string select)
        {
            DirectionsRequest directionsRequest = new DirectionsRequest()
            {
                Origin = addresFrom,
                Destination = addresTo,
                ApiKey = ApiKey,
                Language = "uk",
                TravelMode = dictionaryMode[select]
            };

            directions = await GoogleMaps.Directions.QueryAsync(directionsRequest);
        }

        public IList<ILocationString> GetStaticMapByPolyline()
        {
            Steps = new List<(string Description, string Src)>();
            IList<ILocationString> points = new List<ILocationString>();
            if (directions.Routes.Count() > 0)
            {
                IEnumerable<Step> steps = directions.Routes.First().Legs.First().Steps;

                foreach (Step step in steps)
                {
                    IList<ILocationString> stepPoints = new List<ILocationString>();
                    foreach (var polyline in step.PolyLine.Points)
                    {
                        points.Add(polyline);
                        stepPoints.Add(polyline);
                    }
                    Steps.Add((step.HtmlInstructions, GetStaticMapByPoints(RemoveExtraPoints(stepPoints))));
                }               
            }
            else
            {
                ErrorMessage = $"Status: {directions.StatusStr} ErrorMessage:{directions.ErrorMessage}";
            }

            return points;
        }

        public IList<ILocationString> GetStaticMapByStep()
        {
            IList<ILocationString> path = new List<ILocationString>();
            if (directions.Routes.Count() > 0)
            {
                IEnumerable<Step> steps = directions.Routes.First().Legs.First().Steps;

                path = steps.Select(step => step.StartLocation).ToList<ILocationString>();
                path.Add(steps.Last().EndLocation);
            }
            else
            {
                ErrorMessage = $"Status: {directions.StatusStr} ErrorMessage:{directions.ErrorMessage}";
            }
            return path;
        }

        public async Task<IList<ILocationString>> GetStaticMapBySnapStep()
        {
            IList<ILocationString> snapPath = new List<ILocationString>();
            if (directions.Routes.Count() > 0)
            {
                IEnumerable<Step> steps = directions.Routes.First().Legs.First().Steps;
                IList<ILocationString> path = new List<ILocationString>();
                path = steps.Select(step => step.StartLocation).ToList<ILocationString>();
                path.Add(steps.Last().EndLocation);

                snapPath = await SnapPointsToRoads(path);
            }
            else
            {
                ErrorMessage = $"Status: {directions.StatusStr} ErrorMessage:{directions.ErrorMessage}";
            }
            return snapPath;
        }

        public async Task<IList<ILocationString>> GetStaticMapBySnapPolyline()
        {
            IList<ILocationString> snapPath = new List<ILocationString>();
            
            if (directions.Routes.Count() > 0)
            {
                IList<ILocationString> points = new List<ILocationString>();
                IEnumerable<Step> steps = directions.Routes.First().Legs.First().Steps;

                foreach (Step step in steps)
                {
                    IList<ILocationString> stepPoints = new List<ILocationString>();
                    foreach (var polyline in step.PolyLine.Points)
                    {
                        points.Add(polyline);
                    }
                }

                snapPath = await SnapPointsToRoads(RemoveExtraPoints(points));
            }
            else
            {
                ErrorMessage = $"Status: {directions.StatusStr} ErrorMessage:{directions.ErrorMessage}";
            }
            return snapPath;
        }

        public IList<ILocationString> RemoveExtraPoints(IList<ILocationString> points)
        {
            int MAX_AMOUNT = 400;
            int Count = points.Count;
            int Extras = Count - MAX_AMOUNT;
            if (Extras <= 0) return points;
            double Chance = 1.0 * Extras / Count;
            for (int i = 1; i < Extras; i++)
            {
                points.RemoveAt(Count - (int)(i / Chance));
            }
            return points;
        }
        public async Task<IList<ILocationString>> SnapPointsToRoads(IList<ILocationString> points)
        {
            int HOW_MUCH = 100;

            IList<IEnumerable<ILocationString>> arrayRequest = new List<IEnumerable<ILocationString>>();
            IList<IEnumerable<ILocationString>> arrayResponse = new List<IEnumerable<ILocationString>>();
            int count = points.Count();
            int cycles = count / HOW_MUCH + 1;
            double step = count * 1.0 / cycles;
            for (int i = 0; i < cycles; i++)
            {
                int start = (int)(i * step);
                int end = (int)((i + 1) * step);
                ILocationString[] subarray = new ILocationString[end - start];
                Array.Copy(points.ToArray(), start, subarray, 0, end - start);
                arrayRequest.Add(subarray);
            }

            foreach (var sub in arrayRequest)
            {
                SnapToRoadRequest snapToRoadRequest = new SnapToRoadRequest()
                {
                    ApiKey = ApiKey,
                    Path = sub,
                    Interpolate = false
                };
                var result = await GoogleMaps.SnapToRoad.QueryAsync(snapToRoadRequest);
                arrayResponse.Add(result.SnappedPoints.Select(sp => LatLanToLocation(sp.Location)));
            }
            IList<ILocationString> outList = new List<ILocationString>();
            foreach (var ar in arrayResponse)
                foreach (var point in ar)
                    outList.Add(point);
            return outList;
        }

        public ILocationString LatLanToLocation(LatitudeLongitudeLiteral latitudeLongitudeLiteral)
        {
            return new Location(latitudeLongitudeLiteral.Latitude, latitudeLongitudeLiteral.Longitude);
        }

        public string GetStaticMapByPoints(IList<ILocationString> points, string color = "red")
        {
            StaticMapsEngine staticMapGenerator = new StaticMapsEngine();

            string url = staticMapGenerator.GenerateStaticMapURL(new StaticMapRequest(new ImageSize(1000, 1000))
            {
                Pathes = new List<GoogleMapsApi.StaticMaps.Entities.Path>(){ new GoogleMapsApi.StaticMaps.Entities.Path()
                    {
                        Style = new PathStyle()
                        {
                            Color = color
                        },
                        Locations = points
                    }},
                ApiKey = ApiKey
            });
            return url;
        }
    }
}