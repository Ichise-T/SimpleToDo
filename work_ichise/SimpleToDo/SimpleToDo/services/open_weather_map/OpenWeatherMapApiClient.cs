using System.Net.Http;
using Newtonsoft.Json;
using SimpleToDo.services.open_weather_map.models;

namespace SimpleToDo.services.open_weather_map
{
    /// <summary>
    /// OpenWeatherMap APIから天気情報を取得するためのクライアントクラス。
    /// </summary>
    public class OpenWeatherMapApiClient(string apiKey)
    {
        // HTTPリクエスト送信用のHttpClientインスタンス
        private readonly HttpClient _httpClient = new();
        // APIキー
        private readonly string _apiKey = apiKey;

        /// <summary>
        /// 指定した都市名の天気情報を非同期で取得します。
        /// </summary>
        /// <param name="cityName">都市名（例：Tokyo）</param>
        /// <returns>WeatherInfoオブジェクト（都市名、天気説明、気温、湿度など）</returns>

        public async Task<WeatherInfo> GetWeatherInfoAsync(string cityName)
        {
            // OpenWeatherMap APIのURLを構築
            var url = $"https://api.openweathermap.org/data/2.5/weather?q={cityName}&appid={_apiKey}&units=metric&lang=ja";
            // APIリクエストを送信し、レスポンス文字列を取得
            var response = await _httpClient.GetStringAsync(url);

            // JSONをWeatherApiResponse型にデシリアライズ
            var weatherData = JsonConvert.DeserializeObject<WeatherApiResponse>(response) ;
            weatherData ??= new WeatherApiResponse();

            // 必要な情報をWeatherInfoに詰め替えて返却
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
}
