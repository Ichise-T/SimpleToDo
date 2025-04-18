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
                password=3500BC;
                database=my_db"
                providerName="MySql.Data.MySqlClient"
            />
        </connectionStrings>
    </configuration>
  ```
