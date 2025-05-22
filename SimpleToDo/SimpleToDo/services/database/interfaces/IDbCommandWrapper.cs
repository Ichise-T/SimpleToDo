using System.Data.Common;
using System.Data;

namespace SimpleToDo.services.database.interfaces
{
    /// <summary>
    /// データベースコマンド操作のためのラッパークラス。
    /// SQLコマンドの実行やパラメータ管理などを抽象化します。
    /// </summary>
    public interface IDbCommandWrapper : IDisposable
    {
        /// <summary>
        /// 実行するSQLコマンドのテキスト。
        /// </summary>
        string CommandText { get; set; }

        /// <summary>
        /// コマンドに関連付けられたパラメータのコレクション。
        /// </summary>
        IDataParameterCollection Parameters { get; }

        /// <summary>
        /// 新しいパラメータオブジェクトを作成します。
        /// </summary>
        IDbDataParameter CreateParameter();

        /// <summary>
        /// コマンドを実行し、影響を受けた行数を返します。
        /// </summary>
        int ExecuteNonQuery();

        /// <summary>
        /// コマンドを非同期で実行し影響を受けた行数を返します。
        /// </summary>
        Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// コマンドを非同期で実行し、データリーダーを返します。
        /// </summary>
        Task<DbDataReader> ExecuteReaderAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// コマンドを非同期で実行し、単一の値を返します。
        /// </summary>
        Task<object?> ExecuteScalarAsync(CancellationToken cancellationToken = default);
    }
}
