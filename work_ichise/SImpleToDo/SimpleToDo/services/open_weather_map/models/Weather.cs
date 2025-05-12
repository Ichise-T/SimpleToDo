namespace SimpleToDo.services.open_weather_map.models
{
    /// <summary>
    /// 天気情報（説明など）を表すモデルクラス。
    /// OpenWeather APIのweather要素に対応。
    /// </summary>
    public class Weather
    {
        /// <summary>
        /// 天気の説明（例: "clear sky", "rain" など）
        /// </summary>
        public string Description { get; set; } = string.Empty;
    }
}
