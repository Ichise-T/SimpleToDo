namespace SimpleToDo.services.open_weather_map.models
{
    /// <summary>
    /// 気温や湿度などの主要な気象データを表すモデルクラス。
    /// OpenWeather APIのmain要素に対応。
    /// </summary>
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
