using System.Collections.ObjectModel;

namespace SimpleToDo.mvvm.view_models
{
    /// <summary>
    /// 天気情報1件分を表すViewModelクラス。
    /// </summary>
    public class WeatherInfoItemViewModel(string weatherInfo) 
    {
        /// <summary>
        /// 天気情報の文字列（例：晴れ、曇りなど）
        /// </summary>
        public string WeatherInfo => weatherInfo;
    }

    /// <summary>
    /// 天気情報リストを管理するViewModelクラス。
    /// </summary>
    public class WeatherInfoViewModel(string weatherInfo)
    {
        /// <summary>
        /// 天気情報アイテムのコレクション。
        /// </summary>
        public ObservableCollection<WeatherInfoItemViewModel> WeatherInfoItems { get; } =
            [
                new WeatherInfoItemViewModel(weatherInfo)
            ];
    }
}