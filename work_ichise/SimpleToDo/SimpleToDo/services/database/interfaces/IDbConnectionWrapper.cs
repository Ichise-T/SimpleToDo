namespace SimpleToDo.services.database.interfaces
{
    /// <summary>
    /// データベース接続操作のためのラッパークラス。
    /// コマンド生成や接続のオープン、データベース切り替えなどを抽象化します。
    /// </summary>
    public interface IDbConnectionWrapper : IDisposable
    {
        /// <summary>
        /// 新しいコマンドオブジェクトを生成します。
        /// </summary>
        IDbCommandWrapper CreateCommand();

        /// <summary>
        /// データベース接続を非同期でオープンします。
        /// </summary>
        Task OpenAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 接続先のデータベースを切り替えます。
        /// </summary>
        void ChangeDatabase(string databaseName);
    }
}
