using System.Collections.ObjectModel;

namespace SimpleToDo.mvvm.view_models
{
    public class WeatherInfoItemViewModel(string weatherInfo) 
    {
        public string WeatherInfo => weatherInfo;
    }

    public class WeatherInfoViewModel(string weatherInfo)
    {
        public ObservableCollection<WeatherInfoItemViewModel> WeatherInfoItems { get; } =
               [
                   new WeatherInfoItemViewModel(weatherInfo)
               ];
    }
}
