using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevelopProducts.SaveSystem
{
    /// <summary>
    ///     セーブデータのロード・保存を管理する静的クラス。
    ///
    ///     同一型のセーブデータはメモリ上にキャッシュされ、
    ///     2回目以降のロードはディスクアクセスを行わない。
    /// </summary>
    public static class SaveSystem
    {
        /// <summary>
        ///     指定型のセーブデータを非同期でロードします。
        /// </summary>
        public static ValueTask<T> LoadAsync<T>() where T : SaveBase, new()
        {
            var type = typeof(T);

            // キャッシュ済み
            if (_cache.TryGetValue(type, out var cached))
                return new((T)cached);

            // 読み込み中
            if (_loadingTasks.TryGetValue(type, out var loadingTask))
                return AwaitLoadingTask<T>(loadingTask.Value);

            // 新規ロード開始
            return LoadCoreAsync<T>(type);
        }

        /// <summary>
        ///     指定型のセーブデータを保存します。
        /// </summary>
        public static async ValueTask SaveAsync<T>(T data) where T : SaveBase
        {
            await data.WriteAsync();
            _cache[typeof(T)] = data;
        }

        /// <summary>
        ///     指定型のキャッシュを破棄します。
        /// </summary>
        public static void Unload<T>() where T : SaveBase
        {
            var type = typeof(T);

            _cache.Remove(type);
            _loadingTasks.Remove(type);
        }

        /// <summary>
        ///     キャッシュミス時のロード処理。
        /// </summary>
        private static async ValueTask<T> LoadCoreAsync<T>(Type type)
            where T : SaveBase, new()
        {
            var lazyTask = new Lazy<Task<SaveBase>>(
                () => LoadInternalAsync<T>(type));

            _loadingTasks[type] = lazyTask;

            try
            {
                return (T)await lazyTask.Value;
            }
            finally
            {
                _loadingTasks.Remove(type);
            }
        }

        /// <summary>
        ///     読み込み中タスクの完了を待機する。
        /// </summary>
        private static async ValueTask<T> AwaitLoadingTask<T>(Task<SaveBase> task)
            where T : SaveBase
        {
            return (T)await task;
        }

        /// <summary>
        ///     実際のファイルロード処理。
        /// </summary>
        private static async Task<SaveBase> LoadInternalAsync<T>(Type type)
            where T : SaveBase, new()
        {
            var instance = new T();

            await instance.ReadAsync();

            _cache[type] = instance;

            return instance;
        }

        /// <summary> ロード済みセーブデータ。</summary>
        private static readonly Dictionary<Type, SaveBase> _cache = new();

        /// <summary>ロード中タスク。同一型への同時ロード要求を共有する。</summary>
        private static readonly Dictionary<Type, Lazy<Task<SaveBase>>> _loadingTasks = new();
    }
}