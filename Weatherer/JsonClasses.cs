using System.Text.Json.Serialization;

namespace Weatherer
{
    class JsonClasses
    {
        public class WeatherApiResponse
        {
            [JsonPropertyName("weather")]
            public Weather[] Weather { get; set; }

            [JsonPropertyName("main")]
            public MainInfo Main { get; set; }

            [JsonPropertyName("wind")]
            public Wind Wind { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; }
        }

        public class Weather
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }

            [JsonPropertyName("description")]
            public string Description { get; set; }
        }

        public class MainInfo
        {
            [JsonPropertyName("temp")]
            public double Temp { get; set; }

            [JsonPropertyName("humidity")]
            public int Humidity { get; set; }
        }

        public class Wind
        {
            [JsonPropertyName("speed")]
            public double Speed { get; set; }
        }
    }
}
