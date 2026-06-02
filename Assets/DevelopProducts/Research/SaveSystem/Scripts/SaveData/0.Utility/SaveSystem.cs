using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevelopProducts.SaveSystem
{
    public static class SaveSystem
    {
        public static ValueTask<T> LoadAsync<T>() where T : SaveBase, new()
        {
            var type = typeof(T);

            // キャッシュヒット → 同期パス（Task不要、アロケーションなし）
            if (_cache.TryGetValue(type, out var cached))
                return new ValueTask<T>((T)cached);

            // キャッシュミス → 非同期パスへ
            return LoadCoreAsync<T>(type);
        }

        private static async ValueTask<T> LoadCoreAsync<T>(Type type) where T : SaveBase, new()
        {
            var instance = new T();
            await instance.ReadAsync();
            _cache[type] = instance;
            return instance;
        }

        public static async ValueTask SaveAsync<T>(T data) where T : SaveBase
        {
            await data.WriteAsync();
            _cache[typeof(T)] = data;
        }

        public static void Unload<T>() where T : SaveBase
        {
            _cache.Remove(typeof(T));
        }

        private static readonly Dictionary<Type, SaveBase> _cache = new();
    }
}