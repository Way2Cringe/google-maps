using GoogleMapsApi.Entities.Common;
using GoogleMapsApi.Entities.Directions.Request;
using GoogleMapsApi.Entities.Directions.Response;
using GoogleMapsApi.StaticMaps;
using GoogleMapsApi.StaticMaps.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GoogleMapsApi.App.Pages
{
    public class IndexModel : PageModel
    {
        string src = "https://maps.google.com/maps/api/staticmap?key=AIzaSyDikeBAymgSWrWz-9Y7Danr2mNewZV_MwI&zoom=14&size=1000x1000&path=color%3Ared%7C40.7422598%2C-74.0061511%7C40.7367061%2C-73.9929726%7C40.7638698%2C-73.9731727";
        string baseDynamic = "https://www.google.com/maps/embed/v1/directions?";

        public List<(string Description, string Src)> Steps { get; set; }
        public string ImageSrc { set { src = value; } get { return src; } }
        public string ImageDynamicSrc { set; get; } = "https://www.google.com/maps/embed/v1/directions?origin=24+Sussex+Drive+Ottawa+ON&destination=10+Sussex+Drive+Ottawa+ON&key=AIzaSyDikeBAymgSWrWz-9Y7Danr2mNewZV_MwI";
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

        public void OnPost(string addresTo, string addresFrom)
        {
            ErrorMessage = "";
            if (!String.IsNullOrEmpty(addresTo) && !String.IsNullOrEmpty(addresFrom))
            {
                Task.Run(() => GetStaticMapByPolyline(addresTo, addresFrom)).Wait();
                string[] separator ={ " ", ",",".","-"};
                var options = String.Join('+', addresFrom.Split(separator, StringSplitOptions.RemoveEmptyEntries));
                var destination = String.Join('+', addresTo.Split(separator, StringSplitOptions.RemoveEmptyEntries));
                ImageDynamicSrc = $"{baseDynamic}key={ApiKey}&origin={options}&destination={destination}";
            }
            else
                ErrorMessage = "Введите откуда и куда необходимо построить маршрут";
        }

        public async Task GetStaticMapByPolyline(string addresTo, string addresFrom)
        {
            Steps = new List<(string Description, string Src)>();

            DirectionsRequest directionsRequest = new DirectionsRequest()
            {
                Origin = addresFrom,
                Destination = addresTo,
                ApiKey = ApiKey,
                TravelMode = TravelMode.Driving
            };

            DirectionsResponse directions = await GoogleMaps.Directions.QueryAsync(directionsRequest);

            if (directions.Routes.Count() > 0)
            {
                IEnumerable<Step> steps = directions.Routes.First().Legs.First().Steps;

                IList<ILocationString> points = new List<ILocationString>();

                foreach (Step step in steps)
                {
                    IList<ILocationString> stepPoints = new List<ILocationString>();
                    foreach (var polyline in step.PolyLine.Points)
                    {
                        points.Add(polyline);
                        stepPoints.Add(polyline);
                    }
                    Steps.Add((step.HtmlInstructions, GetStaticMapByPoints(stepPoints)));
                }

                ImageSrc = GetStaticMapByPoints(points);

            }
            else
            {
                ErrorMessage = $"Status: {directions.StatusStr} ErrorMessage:{directions.ErrorMessage}";
            }


        }
        public string GetStaticMapByPoints(IList<ILocationString> points)
        {
            StaticMapsEngine staticMapGenerator = new StaticMapsEngine();

            string url = staticMapGenerator.GenerateStaticMapURL(new StaticMapRequest(new ImageSize(1000, 1000))
            {
                Pathes = new List<GoogleMapsApi.StaticMaps.Entities.Path>(){ new GoogleMapsApi.StaticMaps.Entities.Path()
                    {
                        Style = new PathStyle()
                        {
                            Color = "red"
                        },
                        Locations = points
                    }},
                ApiKey = ApiKey
            });
            return url;
        }
    }
}