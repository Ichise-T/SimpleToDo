using System.Data.Common;
using System.Data;
using SimpleToDo.services.database.interfaces;

namespace SimpleToDo.services.database.wrappers
{
    /// <summary>
    /// DbCommandをラップし、IDbCommandWrapperインターフェースを実装するクラス。
    /// SQLコマンドの実行やパラメータ管理などを抽象化します。
    /// </summary>
    public class DbCommandWrapper(DbCommand command) : IDbCommandWrapper
    {
        // 内部で保持するDbCommandインスタンス
        private readonly DbCommand _command = command;

        /// <summary>
        /// 実行するSQLコマンドのテキスト。
        /// </summary>
        public string CommandText { get => _command.CommandText; set => _command.CommandText = value; }

        /// <summary>
        /// コマンドに関連付けられたパラメータのコレクション。
        /// </summary>
        public IDataParameterCollection Parameters => _command.Parameters;

        /// <summary>
        /// 新しいパラメータオブジェクトを作成します。
        /// </summary>
        public IDbDataParameter CreateParameter() => _command.CreateParameter();

        /// <summary>
        /// コマンドを実行し、影響を受けた行数を返します。
        /// </summary>
        public int ExecuteNonQuery() => _command.ExecuteNonQuery();

        /// <summary>
        /// リソースを解放します。
        /// </summary>
        public void Dispose()
        {
            _command.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// コマンドを非同期で実行し、影響を受けた行数を返します。
        /// </summary>
        public async Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken = default)
        {
            return await _command.ExecuteNonQueryAsync(cancellationToken);
        }

        /// <summary>
        /// コマンドを非同期で実行し、データリーダーを返します。
        /// </summary>
        public async Task<DbDataReader> ExecuteReaderAsync(CancellationToken cancellationToken = default)
        {
            return await _command.ExecuteReaderAsync(cancellationToken);
        }

        /// <summary>
        /// コマンドを非同期で実行し、単一の値を返します。
        /// </summary>
        public async Task<object?> ExecuteScalarAsync(CancellationToken cancellationToken = default)
        {
            return await _command.ExecuteScalarAsync(cancellationToken);
        }
    }
}