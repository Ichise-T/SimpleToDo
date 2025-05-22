using System.Collections.ObjectModel;

namespace SimpleToDo.mvvm.view_models
{
  /// <summary>
  /// 天気情報1件分を表すViewModelクラス。
  /// </summary>
  /// <remarks>
  /// 天気情報を受け取りViewModelを初期化します。
  /// </remarks>
  /// <param name="weatherInfo">表示する天気情報</param>
  public class WeatherInfoItemViewModel(string weatherInfo)
  {
    /// <summary>
    /// 天気情報の文字列（例：晴れ、曇りなど）
    /// </summary>
    public string WeatherInfo { get; } = weatherInfo ?? string.Empty;
  }

  /// <summary>
  /// 天気情報リストを管理するViewModelクラス。
  /// このクラスは現在は使用していないため、削除または将来の拡張のために保持できます。
  /// </summary>
  /// <remarks>
  /// 天気情報を受け取りViewModelを初期化します。
  /// </remarks>
  /// <param name="weatherInfo">表示する天気情報</param>
  [Obsolete("MainViewModelから直接WeatherInfoItemViewModelを使用しているため、このクラスは不要です。")]
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