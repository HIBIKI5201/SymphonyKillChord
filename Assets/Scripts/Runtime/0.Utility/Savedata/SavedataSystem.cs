using SymphonyFrameWork.System.ServiceLocate;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace KillChord.Runtime.Utility.OutGame.Savedata
{
    /// <summary>
    ///     セーブデータの管理システム。
    /// </summary>
    public class SavedataSystem
    {
        public SavedataSystem()
        {
            _cache = new();
            _loadingTasks = new();
            _lock = new();
        }

        /// <summary>
        ///    指定された型のセーブデータを非同期で読み込みます。
        ///    キャッシュが存在する場合はキャッシュから返し、存在しない場合は新たに読み込みます。
        /// </summary>
        /// <typeparam name="T">セーブデータの型。</typeparam>
        /// <returns>指定された型のセーブデータ。</returns>
        public ValueTask<T> LoadAsync<T>() where T : SaveBase, new()
        {
            var type = typeof(T);

            lock (_lock)
            {
                if (_cache.TryGetValue(type, out var cached))
                    return new((T)cached);

                if (_loadingTasks.TryGetValue(type, out var loadingTask))
                    return AwaitLoadingTask<T>(loadingTask.Value);

                var lazyTask = new Lazy<Task<SaveBase>>(() => LoadInternalAsync<T>(type));
                _loadingTasks[type] = lazyTask;
                return AwaitAndCleanup<T>(type, lazyTask.Value);
            }
        }
        /// <summary>
        ///   指定されたセーブデータを非同期で保存します。
        /// </summary>
        /// <typeparam name="T">セーブデータの型。</typeparam>
        /// <param name="data">保存するセーブデータ。</param>
        /// <returns>非同期操作のタスク。</returns>
        public async ValueTask SaveAsync<T>(T data) where T : SaveBase
        {
            var writeLock = _writeLocks.GetOrAdd(typeof(T), _ => new SemaphoreSlim(1, 1));
            await writeLock.WaitAsync();
            try
            {
                await data.WriteAsync();
                lock (_lock)
                {
                    _cache[typeof(T)] = data;
                }
            }
            finally
            {
                writeLock.Release();
            }
        }
        /// <summary>
        ///   指定された型のセーブデータをキャッシュから削除します。
        /// </summary>
        /// <typeparam name="T">セーブデータの型。</typeparam>
        public void Unload<T>() where T : SaveBase
        {
            var type = typeof(T);
            // ロックを取得して、キャッシュと読み込みタスクから指定された型のデータを削除します。
            lock (_lock)
            {
                _cache.Remove(type);
                _loadingTasks.Remove(type);
            }
        }
        /// <summary>
        ///   指定された型のセーブデータの読み込みタスクを待機し、
        ///   完了後にキャッシュからクリーンアップします。
        /// </summary>
        /// <typeparam name="T">セーブデータの型。</typeparam>
        /// <param name="type">セーブデータの型情報。</param>
        /// <param name="task">読み込みタスク。</param>
        /// <returns>指定された型のセーブデータ。</returns>
        private async ValueTask<T> AwaitAndCleanup<T>(Type type, Task<SaveBase> task)
            where T : SaveBase
        {
            // タスクの完了を待機し、結果を取得します。
            try
            {
                return (T)await task;
            }
            // タスクの完了後に、ロックを取得して読み込みタスクをクリーンアップします。
            finally
            {
                lock (_lock)
                {
                    _loadingTasks.Remove(type);
                }
            }
        }
        /// <summary>
        ///    taskを待機して、完了後に指定された型のセーブデータを返します。
        /// </summary>
        /// <typeparam name="T">セーブデータの型。</typeparam>
        /// <param name="task">読み込みタスク。</param>
        /// <returns>指定された型のセーブデータ。</returns>
        private async ValueTask<T> AwaitLoadingTask<T>(Task<SaveBase> task)
            where T : SaveBase
        {
            // タスクの完了を待機し、結果を取得します。
            return (T)await task;
        }
        /// <summary>
        ///  loadAsyncの内部実装。指定された型のセーブデータを非同期で読み込み、キャッシュに保存します。
        /// </summary>
        /// <typeparam name="T">セーブデータの型。</typeparam>
        /// <param name="type">セーブデータの型情報。</param>
        /// <returns>指定された型のセーブデータ。</returns>
        private async Task<SaveBase> LoadInternalAsync<T>(Type type)
            where T : SaveBase, new()
        {
            var instance = new T();
            await instance.ReadAsync();
            lock (_lock)
            {
                _cache[type] = instance;
            }
            return instance;
        }
        /// <summary>キャッシュされたセーブデータを保持する辞書。</summary>
        private readonly Dictionary<Type, SaveBase> _cache = new();
        /// <summary>読み込み中のセーブデータのタスクを保持する辞書。</summary>
        private readonly Dictionary<Type, Lazy<Task<SaveBase>>> _loadingTasks = new();
        /// <summary>キャッシュと読み込みタスクへのアクセスを同期するためのロックオブジェクト。</summary>
        private readonly object _lock = new();
        private readonly ConcurrentDictionary<Type, SemaphoreSlim> _writeLocks = new();
    }
}
