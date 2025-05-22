# タスク管理アプリ

## 開発環境

- Visual Studio 2022
- MySQL 9.2
- .NET 8.0

## 環境構築

- SimpleToDoプロジェクトのフォルダ内にApp.configを作成し、MySQL Serverの接続文字列とOpenWeatherMapのAPIキーを設定する。
- 例）
  
  ```xml
    <?xml version="1.0" encoding="utf-8" ?>
    <configuration>
      <connectionStrings>
        <add 
          name="MySqlConnection" 
          connectionString="server=localhost;
          userid=root;
          password=your_password;
          database=mysql"
          providerName="MySql.Data.MySqlClient"
        />
      </connectionStrings>
      <appSettings>
        <add key="OpenWeatherAPIKey" value="your_api_key"/>;
        <add key="DatabaseName" value="simple_todo"/>
        <add key="TableName" value="todo"/>
      </appSettings>
    </configuration>
  ```

## ディレクトリ構造

```text
SimpleToDo/
│
├── App.config                - アプリケーション設定（接続文字列、APIキーなど）
├── App.xaml                  - アプリケーション定義とリソース
├── App.xaml.cs               - アプリケーションのコードビハインド
├── MainWindow.xaml           - メインウィンドウのUI定義
├── MainWindow.xaml.cs        - メインウィンドウのコードビハインド
│
├── mvvm/                     - MVVMパターン関連のクラス
│   ├── models/               - データモデル
│   │   └── ToDo.cs           - ToDoアイテムのデータモデル
│   │
│   ├── views/                - ビュー（UserControlなど）
│   │   ├── ToDo.xaml         - ToDoリスト表示用のUserControl
│   │   └── WeatherInfo.xaml  - 天気情報表示用のUserControl
│   │
│   └── view_models/          - ビューモデル
│       ├── MainViewModel.cs  - アプリケーション全体の状態管理
│       ├── ToDo.cs           - ToDoアイテム用のビューモデル
│       └── WeatherInfo.cs    - 天気情報用のビューモデル
│
├── services/                 - サービスクラス群
│   ├── database/             - データベース操作関連
│   │   ├── DatabaseCrudManager.cs - データベースCRUD操作
│   │   ├── interfaces/       - データベース操作のインターフェース
│   │   └── wrappers/         - データベース接続のラッパークラス
│   │
│   └── open_weather_map/     - 天気情報API関連
│       ├── OpenWeatherMapApiClient.cs - API通信処理
│       └── models/           - APIレスポンスのデータモデル
│
└── utils/                    - ユーティリティクラス
    ├── RelayCommand.cs       - コマンドパターン実装
    └── DataConvert.cs        - データ変換ユーティリティ
```
