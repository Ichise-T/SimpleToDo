using System.Net.Http;
using Newtonsoft.Json;

namespace SimpleToDo.services
{
    public class WeatherInfo
    {
        public string? Name { get; set; } // 都市名（例：東京）
        public string? Description { get; set; } // 天気の説明（例：晴れ、曇りなど）
        public float Temperature { get; set; } // 気温（摂氏）
        public int Humidity { get; set; } // 湿度（％）
    }

    public class OpenWeatherApiClient(string apiKey)
    {
        private readonly HttpClient _httpClient = new(); // HTTPリクエスト送信用のHttpClientインスタンス
        private readonly string _apiKey = apiKey;

        public async Task<WeatherInfo> GetWeatherInfoAsync(string cityName)
        {
            // OpenWeatherMap APIのURLを構築
            var url = $"https://api.openweathermap.org/data/2.5/weather?q={cityName}&appid={_apiKey}&units=metric&lang=ja";
            // APIリクエストを送信 → 文字列を取得
            var response = await _httpClient.GetStringAsync(url);

            // JSONをParseして必要な情報を抽出
            var weatherData = JsonConvert.DeserializeObject<WeatherApiResponse>(response) ;
            weatherData ??= new WeatherApiResponse();

            // 天気情報を取得
            WeatherInfo weatherInfo = new()
            {
                Name = weatherData.Name,
                Description = weatherData.Weather[0].Description,
                Temperature = weatherData.Main.Temp,
                Humidity = weatherData.Main.Humidity
            };

            return weatherInfo;
        }
    }

    public class WeatherApiResponse
    {
        public List<Weather> Weather { get; set; } = [];
        public MainData Main { get; set; } = new();
        public string Name { get; set; } = string.Empty;
    }

    public class Weather
    {
        public string Description { get; set; } = string.Empty;
    }

    public class MainData
    {
        public float Temp { get; set; } 
        public int Humidity { get; set; } 
    }
}
