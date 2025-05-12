namespace SimpleToDo.services.open_weather_map.models
{    
    /// <summary>
    /// OpenWeather APIのレスポンス全体を表すモデルクラス。
    /// 天気情報、主要気象データ、都市名などを保持します。
    /// </summary>
    public class WeatherApiResponse
    {
        /// <summary>
        /// 天気情報のリスト（weather要素）
        /// </summary>
        public List<Weather> Weather { get; set; } = [];

        /// <summary>
        /// 主要な気象データ（main要素）
        /// </summary>
        public MainData Main { get; set; } = new();

        /// <summary>
        /// 都市名（name要素）
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }
}
