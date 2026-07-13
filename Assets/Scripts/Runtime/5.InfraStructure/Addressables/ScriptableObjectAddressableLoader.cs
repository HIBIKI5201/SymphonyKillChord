using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityAddressables = UnityEngine.AddressableAssets.Addressables;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace KillChord.Runtime.InfraStructure.Addressables
{
    /// <summary>
    ///     ScriptableObject 用 Addressables ロード補助です。
    /// </summary>
    public static class ScriptableObjectAddressableLoader
    {
        /// <summary>
        ///     Addressables キーから ScriptableObject をロードします。
        /// </summary>
        /// <typeparam name="TAsset"> ロードするアセット型です。 </typeparam>
        /// <param name="key"> Addressables キーです。 </param>
        /// <param name="context"> ログ出力に使うコンテキストです。 </param>
        /// <param name="cancellationToken"> キャンセルトークンです。 </param>
        /// <returns> ロード結果です。 </returns>
        /// <exception cref="System.ArgumentException"> キーが未設定の場合に発生します。 </exception>
        /// <exception cref="System.OperationCanceledException"> ロードがキャンセルされた場合に発生します。 </exception>
        /// <exception cref="System.InvalidOperationException"> アセットのロードに失敗した場合に発生します。 </exception>
        public static async Task<TAsset> LoadAssetAsync<TAsset>(
            this string key,
            Object context,
            CancellationToken cancellationToken)
            where TAsset : ScriptableObject
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new System.ArgumentException("Addressables キーが未設定です。", nameof(key));
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (TryGetHandle(context, key, out AsyncOperationHandle loadedHandle))
            {
                return await AwaitLoadedHandleAsync(loadedHandle.Convert<TAsset>(), key, context, cancellationToken);
            }

            AsyncOperationHandle<TAsset> handle = UnityAddressables.LoadAssetAsync<TAsset>(key);
            RegisterHandle(context, key, handle);
            return await AwaitLoadedHandleAsync(handle, key, context, cancellationToken);
        }

        /// <summary>
        ///     ロード済みアセットを解放します。
        /// </summary>
        /// <param name="key"> Addressables キーです。 </param>
        /// <param name="context"> ロード元のコンテキストです。 </param>
        public static void ReleaseLoadedAsset(this string key, Object context)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            ReleaseHandle(key, context);
        }

        /// <summary>
        ///     ロード済みハンドルの完了を待機します。
        /// </summary>
        /// <typeparam name="TAsset"> ロードするアセット型です。 </typeparam>
        /// <param name="handle"> Addressables ハンドルです。 </param>
        /// <param name="key"> Addressables キーです。 </param>
        /// <param name="context"> ログ出力に使うコンテキストです。 </param>
        /// <param name="cancellationToken"> キャンセルトークンです。 </param>
        /// <returns> ロード結果です。 </returns>
        private static async Task<TAsset> AwaitLoadedHandleAsync<TAsset>(
            AsyncOperationHandle<TAsset> handle,
            string key,
            Object context,
            CancellationToken cancellationToken)
            where TAsset : ScriptableObject
        {
            try
            {
                TAsset asset = await handle.Task;
                cancellationToken.ThrowIfCancellationRequested();

                if (handle.Status != AsyncOperationStatus.Succeeded || asset == null)
                {
                    throw new System.InvalidOperationException(
                        $"{typeof(TAsset).Name} のロードに失敗しました。Key={key}",
                        handle.OperationException);
                }

                return asset;
            }
            catch
            {
                ReleaseHandle(key, context);
                throw;
            }
        }

        /// <summary>
        ///     ロード済みハンドルを登録します。
        /// </summary>
        /// <param name="context"> ロード元のコンテキストです。 </param>
        /// <param name="key"> Addressables キーです。 </param>
        /// <param name="handle"> 登録するハンドルです。 </param>
        private static void RegisterHandle(Object context, string key, AsyncOperationHandle handle)
        {
            int contextId = GetContextId(context);
            if (!_handleMap.TryGetValue(contextId, out System.Collections.Generic.Dictionary<string, AsyncOperationHandle> handles))
            {
                handles = new System.Collections.Generic.Dictionary<string, AsyncOperationHandle>(System.StringComparer.Ordinal);
                _handleMap.Add(contextId, handles);
            }

            handles[key] = handle;
        }

        /// <summary>
        ///     ロード済みハンドルを取得します。
        /// </summary>
        /// <param name="context"> ロード元のコンテキストです。 </param>
        /// <param name="key"> Addressables キーです。 </param>
        /// <param name="handle"> 取得したハンドルです。 </param>
        /// <returns> 登録済みの場合はtrue。</returns>
        private static bool TryGetHandle(Object context, string key, out AsyncOperationHandle handle)
        {
            int contextId = GetContextId(context);
            if (_handleMap.TryGetValue(contextId, out System.Collections.Generic.Dictionary<string, AsyncOperationHandle> handles)
                && handles.TryGetValue(key, out handle)
                && handle.IsValid())
            {
                return true;
            }

            handle = default;
            return false;
        }

        /// <summary>
        ///     ロード済みハンドルを解放します。
        /// </summary>
        /// <param name="key"> Addressables キーです。 </param>
        /// <param name="context"> ロード元のコンテキストです。 </param>
        private static void ReleaseHandle(string key, Object context)
        {
            int contextId = GetContextId(context);
            if (!_handleMap.TryGetValue(contextId, out System.Collections.Generic.Dictionary<string, AsyncOperationHandle> handles)
                || !handles.TryGetValue(key, out AsyncOperationHandle handle))
            {
                return;
            }

            if (handle.IsValid())
            {
                UnityAddressables.Release(handle);
            }

            handles.Remove(key);
            if (handles.Count == 0)
            {
                _handleMap.Remove(contextId);
            }
        }

        /// <summary>
        ///     コンテキスト識別子を返します。
        /// </summary>
        /// <param name="context"> ロード元のコンテキストです。 </param>
        /// <returns> コンテキスト識別子です。 </returns>
        private static int GetContextId(Object context)
        {
            return context == null ? 0 : context.GetInstanceID();
        }

        private static readonly System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<string, AsyncOperationHandle>> _handleMap =
            new();
    }
}
