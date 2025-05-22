using SimpleToDo.mvvm.view_models;
using System.Windows.Controls;

namespace SimpleToDo.mvvm.views
{
    /// <summary>
    /// WeatherInfo.xaml の相互作用ロジック
    /// 天気情報を表示するためのユーザーコントロール
    /// </summary>
    public partial class WeatherInfo : UserControl
    {
        /// <summary>
        /// DataContextをMainViewModelにキャストして取得するプロパティ
        /// </summary>
        private MainViewModel? ViewModel => DataContext as MainViewModel;

        /// <summary>
        /// WeatherInfo コンストラクタ - コンポーネントの初期化を行います
        /// </summary>
        public WeatherInfo()
        {
            InitializeComponent();
            
            // MVVM パターンを厳密に適用するため、コードビハインドでのViewModelとの連携は最小限に
        }
        
        /// <summary>
        /// 天気情報を手動で更新するメソッド
        /// </summary>
        public async Task RefreshWeatherInfoAsync()
        {
            if (ViewModel != null)
            {
                await ViewModel.LoadWeatherInfoAsync();
            }
        }
    }
}