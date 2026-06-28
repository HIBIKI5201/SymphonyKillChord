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
        ///     指定された型のセーブファイルが存在するかを判定する。
        /// </summary>
        public bool Exists<T>() where T : SaveBase
        {
            return SaveBase.Exists<T>();
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
                {
                    return new ((T)cached);
                }

                if (_loadingTasks.TryGetValue(type, out var loadingTask))
                {
                    return AwaitLoadingTask<T>(loadingTask.Value);
                }

                var generation = GetLoadGeneration(type);
                var lazyTask = new Lazy<Task<SaveBase>>(() => LoadInternalAsync<T>(type, generation));
                _loadingTasks[type] = lazyTask;

                return AwaitAndCleanup<T>(type, lazyTask);
            }
        }

        /// <summary>
        ///   指定されたセーブデータを非同期で保存します。
        /// </summary>
        /// <typeparam name="T">保存するセーブデータの型。</typeparam>
        /// <param name="data">保存するセーブデータ。</param>
        /// <returns>非同期操作のタスク。</returns>
        public async ValueTask SaveAsync<T>(T data) where T : SaveBase
        {
            var type = typeof(T);
            var writeLock = GetWriteLock(type);

            await writeLock.WaitAsync();
            try
            {
                await data.WriteAsync();

                lock (_lock)
                {
                    _cache[type] = data;
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
        ///     指定された型のセーブデータを削除し、キャッシュからも除去します。
        /// </summary>
        /// <typeparam name="T">セーブデータの型。</typeparam>
        public async ValueTask DeleteSaveDataAsync<T>() where T : SaveBase
        {
            var type = typeof(T);

            // ロックを取得して、キャッシュと読み込みタスクから指定された型のデータを削除し、世代番号を進めます。
            lock (_lock)
            {
                IncrementLoadGeneration(type);
                _cache.Remove(type);
                _loadingTasks.Remove(type);
            }

            // 書き込みロックを取得して、セーブデータの削除を行います。
            var writeLock = GetWriteLock(type);
            await writeLock.WaitAsync();
            try
            {
                SaveBase.DeleteSaveData<T>();

                lock (_lock)
                {
                    Unload<T>();
                }
            }
            finally
            {
                writeLock.Release();
            }
        }

        /// <summary>
        ///   指定された型のセーブデータの読み込みタスクを待機し、
        ///   完了後に読み込み中タスク一覧からクリーンアップします。
        /// </summary>
        /// <typeparam name="T">セーブデータの型。</typeparam>
        /// <param name="type">セーブデータの型情報。</param>
        /// <param name="lazyTask">読み込みタスク。</param>
        /// <returns>指定された型のセーブデータ。</returns>
        private async ValueTask<T> AwaitAndCleanup<T>(Type type, Lazy<Task<SaveBase>> lazyTask)
            where T : SaveBase
        {
            // 読み込みタスクを待機し、完了後にキャッシュに保存されているかを確認します。
            try
            {
                return (T)await lazyTask.Value;
            }
            finally
            {
                lock (_lock)
                {
                    // 読み込みタスクがまだ存在し、かつ現在のタスクと同一であれば、読み込みタスク一覧から削除します。
                    if (_loadingTasks.TryGetValue(type, out var currentTask) && ReferenceEquals(currentTask, lazyTask))
                    {
                        _loadingTasks.Remove(type);
                    }
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
        ///  LoadAsync の内部実装。
        ///  指定された型のセーブデータを非同期で読み込み、無効化されていなければキャッシュに保存します。
        /// </summary>
        /// <typeparam name="T">セーブデータの型。</typeparam>
        /// <param name="type">セーブデータの型情報。</param>
        /// <param name="generation">読み込み開始時点の世代番号。</param>
        /// <returns>指定された型のセーブデータ。</returns>
        private async Task<SaveBase> LoadInternalAsync<T>(Type type, int generation)
            where T : SaveBase, new()
        {
            var writeLock = GetWriteLock(type);
            await writeLock.WaitAsync();
            try
            {
                var instance = new T();
                await instance.ReadAsync();

                lock (_lock)
                {
                    // 読み込み開始時点の世代番号と現在の世代番号が一致する場合のみ、キャッシュに保存します。
                    if (GetLoadGeneration(type) == generation)
                    {
                        _cache[type] = instance;
                    }
                }

                return instance;
            }
            finally
            {
                writeLock.Release();
            }
        }

        /// <summary>
        ///   指定された型の書き込みロックを取得します。
        /// </summary>
        /// <param name="type">セーブデータの型。</param>
        /// <returns>型ごとの排他ロック。</returns>
        private SemaphoreSlim GetWriteLock(Type type)
        {
            return _writeLocks.GetOrAdd(type, _ => new SemaphoreSlim(1, 1));
        }

        /// <summary>
        ///   指定された型の読み込み世代番号を取得します。
        /// </summary>
        /// <param name="type">セーブデータの型。</param>
        /// <returns>現在の世代番号。</returns>
        private int GetLoadGeneration(Type type)
        {
            if (_loadGenerations.TryGetValue(type, out var generation))
            {
                return generation;
            }

            return 0;
        }

        /// <summary>
        ///   指定された型の読み込み世代番号を進めます。
        /// </summary>
        /// <param name="type">セーブデータの型。</param>
        private void IncrementLoadGeneration(Type type)
        {
            _loadGenerations.AddOrUpdate(type, 1, (_, current) => current + 1);
        }

        /// <summary>キャッシュされたセーブデータを保持する辞書。</summary>
        private readonly Dictionary<Type, SaveBase> _cache = new();

        /// <summary>読み込み中のセーブデータのタスクを保持する辞書。</summary>
        private readonly Dictionary<Type, Lazy<Task<SaveBase>>> _loadingTasks = new();

        /// <summary>キャッシュと読み込みタスクへのアクセスを同期するためのロックオブジェクト。</summary>
        private readonly object _lock = new();

        /// <summary>型ごとの保存・読込・削除の直列化に使うロック。</summary>
        private readonly ConcurrentDictionary<Type, SemaphoreSlim> _writeLocks = new();

        /// <summary>削除で古い load 結果を無効化するための世代番号。</summary>
        private readonly ConcurrentDictionary<Type, int> _loadGenerations = new();
    }
}
