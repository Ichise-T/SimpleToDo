# タスク管理アプリ

## 環境構築

- SimpleToDoプロジェクトのフォルダ内にApp.configを作成し、MySQL Serverの接続文字列を設定する。
- 例）↓
  
  ```App.config
    <?xml version="1.0" encoding="utf-8" ?>
    <configuration>
      <connectionStrings>
        <add 
          name="MySqlConnection" 
          connectionString="server=localhost;
          userid=root;
          password=password;
          database=my_database"
          providerName="MySql.Data.MySqlClient"
        />
      </connectionStrings>
      <appSettings>
        <add key="OpenWeatherAPIKey" value="your_api_key"/>
      </appSettings>
    </configuration>
  ```
