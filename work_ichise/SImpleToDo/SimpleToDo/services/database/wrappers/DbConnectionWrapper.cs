using SimpleToDo.services.database.interfaces;
using System.Data.Common;

namespace SimpleToDo.services.database.wrappers
{
    /// <summary>
    /// DbConnectionをラップし、IDbConnectionWrapperインターフェースを実装するクラス。
    /// データベース接続の操作やコマンド生成、データベース切り替えなどを抽象化します。
    /// </summary>
    public class DbConnectionWrapper(DbConnection connection) : IDbConnectionWrapper
    {
        // 内部で保持するDbConnectionインスタンス
        private readonly DbConnection _connection = connection;

        /// <summary>
        /// 内部のDbConnection型名を取得します。
        /// </summary>
        public string InnerConnectionTypeName => _connection.GetType().Name.ToLower();

        /// <summary>
        /// 新しいコマンドオブジェクトを生成します。
        /// </summary>
        /// <returns>IDbCommandWrapperのインスタンス</returns>
        public IDbCommandWrapper CreateCommand()
        {
            // DbCommandをラップして返す
            return new DbCommandWrapper(_connection.CreateCommand());
        }

        /// <summary>
        /// データベース接続を非同期でオープンします。
        /// </summary>
        /// <param name="cancellationToken">キャンセルトークン</param>
        public async Task OpenAsync(CancellationToken cancellationToken = default)
        {
            await _connection.OpenAsync(cancellationToken);
        }

        /// <summary>
        /// 接続先のデータベースを切り替えます。
        /// </summary>
        /// <param name="databaseName">切り替えるデータベース名</param>
        public void ChangeDatabase(string databaseName)
        {
            _connection.ChangeDatabase(databaseName);
        }

        /// <summary>
        /// リソースを解放します。
        /// </summary>
        public void Dispose()
        {
            _connection.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}