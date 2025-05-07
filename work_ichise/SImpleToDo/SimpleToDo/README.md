# タスク管理アプリ

## 開発環境

- Visual Studio 2022
- MySQL 9.2
- .NET 8.0

## 環境構築

- SimpleToDoプロジェクトのフォルダ内にApp.configを作成し、MySQL Serverの接続文字列とOpenWeatherのAPIキーを設定する。
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
        <add key="DatabaseName" value="SimpleToDo"/>
        <add key="TableName" value="ToDo"/>
      </appSettings>
    </configuration>
  ```
