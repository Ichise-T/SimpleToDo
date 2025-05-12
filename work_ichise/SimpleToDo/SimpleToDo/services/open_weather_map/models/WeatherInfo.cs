namespace SimpleToDo.services.open_weather_map.models
{
    /// <summary>
    /// アプリケーション内で利用する天気情報をまとめたモデルクラス。
    /// 都市名、天気の説明、気温、湿度などを保持します。
    /// </summary>
    public class WeatherInfo
    {
        /// <summary>
        /// 都市名（例：東京）
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 天気の説明（例：晴れ、曇りなど）
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 気温（摂氏）
        /// </summary>
        public float Temperature { get; set; }

        /// <summary>
        /// 湿度（％）
        /// </summary>
        public int Humidity { get; set; }
    }
}
