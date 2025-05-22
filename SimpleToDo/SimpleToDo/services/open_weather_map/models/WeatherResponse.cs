namespace SimpleToDo.services.weather.models
{
    /// <summary>
    /// アプリ内で利用する天気情報のDTO。
    /// 必要な情報のみを保持し、ViewModelやViewで扱いやすくする。
    /// </summary>
    public class WeatherResponse
    {
        /// <summary>
        /// 都市名
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 天気の説明
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 気温（摂氏）
        /// </summary>
        public float Temperature { get; set; }

        /// <summary>
        /// 湿度（％）
        /// </summary>
        public int Humidity { get; set; }

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

        public class Weather
        {
            /// <summary>
            /// 天気の説明（例: "clear sky", "rain" など）
            /// </summary>
            public string Description { get; set; } = string.Empty;
        }

        public class MainData
        {
            /// <summary>
            /// 現在の気温（摂氏）
            /// </summary>
            public float Temp { get; set; }

            /// <summary>
            /// 湿度（パーセント）
            /// </summary>
            public int Humidity { get; set; }
        }
    }
}