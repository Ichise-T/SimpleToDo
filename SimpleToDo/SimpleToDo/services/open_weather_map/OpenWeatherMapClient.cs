using System.Net.Http;
using System.Text.Json;
using System.Web;
using System.Net.Http.Json;
using System.Net;
using SimpleToDo.services.weather.models;

namespace SimpleToDo.services.weather
{
    /// <summary>
    /// OpenWeatherMap APIから天気情報を取得するためのクライアントクラス。
    /// </summary>
    public class OpenWeatherMapApiClient : IDisposable
    {
        // HTTPリクエスト送信用のHttpClientインスタンス
        private readonly HttpClient _httpClient;
        // APIキー
        private readonly string _apiKey;
        // APIのベースURL
        private const string BaseApiUrl = "https://api.openweathermap.org/data/2.5/weather";
        // 破棄フラグ
        private bool _disposed;

        /// <summary>
        /// OpenWeatherMap APIクライアントのコンストラクタ。
        /// </summary>
        /// <param name="apiKey">OpenWeatherMap APIキー</param>
        public OpenWeatherMapApiClient(string apiKey)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        /// <summary>
        /// 指定した都市名の天気情報を非同期で取得します。
        /// </summary>
        /// <param name="cityName">都市名（例：Tokyo）</param>
        /// <returns>WeatherResponseオブジェクト（都市名、天気説明、気温、湿度など）</returns>
        /// <exception cref="ArgumentNullException">cityNameがnullの場合</exception>
        /// <exception cref="HttpRequestException">API呼び出しに失敗した場合</exception>
        public async Task<WeatherResponse> GetWeatherResponseAsync(string cityName)
        {
            if (string.IsNullOrWhiteSpace(cityName))
                throw new ArgumentNullException(nameof(cityName), "都市名が指定されていません。");

            try
            {
                // URLパラメータを安全に構築
                var query = HttpUtility.ParseQueryString(string.Empty);
                query["q"] = cityName;
                query["appid"] = _apiKey;
                query["units"] = "metric";
                query["lang"] = "ja";

                var requestUrl = $"{BaseApiUrl}?{query}";

                // より安全かつ効率的なHTTPレスポンス処理
                using var response = await _httpClient.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode(); // 400/500系エラーを確認

                var weatherData = await response.Content.ReadFromJsonAsync<WeatherResponse.WeatherApiResponse>()
                    ?? new WeatherResponse.WeatherApiResponse();

                // サーバーからのレスポンスが有効かチェック
                if (weatherData.Weather == null || weatherData.Weather.Count == 0)
                {
                    return new WeatherResponse
                    {
                        Name = cityName,
                        Description = "データが取得できませんでした",
                        Temperature = 0,
                        Humidity = 0
                    };
                }

                // 必要な情報をWeatherResponseに詰め替えて返却
                return new WeatherResponse
                {
                    Name = weatherData.Name,
                    Description = weatherData.Weather[0].Description,
                    Temperature = weatherData.Main.Temp,
                    Humidity = weatherData.Main.Humidity
                };
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // 都市が見つからなかった場合の専用エラーレスポンス
                return new WeatherResponse
                {
                    Name = cityName,
                    Description = "指定された都市が見つかりませんでした",
                    Temperature = 0,
                    Humidity = 0
                };
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is JsonException || ex is TaskCanceledException)
            {
                // API呼び出しやJSON変換に関するエラーを統合
                throw new HttpRequestException($"天気情報の取得に失敗しました: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// リソースを破棄します。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// リソースを破棄します。
        /// </summary>
        /// <param name="disposing">マネージドリソースも破棄するかどうか</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _httpClient?.Dispose();
                }

                _disposed = true;
            }
        }
    }
}