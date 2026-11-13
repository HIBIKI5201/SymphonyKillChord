using System.Threading;
using UnityEngine;

namespace KillChord.Runtime.Composition.Bootstrap
{
    /// <summary>
    ///     各フェーズ共通の初期化モジュール基底クラスです。何もしない既定実装を持ちます。
    /// </summary>
    public abstract class InitializationModuleBase : MonoBehaviour, IInitializationModule
    {
        /// <summary> モジュール名です。 </summary>
        public abstract string ModuleName { get; }

        /// <summary> 実行順です。 </summary>
        public abstract int Order { get; }

        /// <summary>
        ///     単体で実行できる初期化を行います。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public virtual bool Init()
        {
            return true;
        }

        /// <summary>
        ///     非同期のリソースロードを行います。
        /// </summary>
        /// <param name="cancellationToken"> キャンセルトークンです。 </param>
        /// <returns> 成功した場合はtrue。 </returns>
        public virtual async Awaitable<bool> ResourceLoadAsync(CancellationToken cancellationToken)
        {
            return true;
        }

        /// <summary>
        ///     ロード後に必要な生成とサービス登録を行います。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public virtual bool Build()
        {
            return true;
        }

        /// <summary>
        ///     他モジュールとの結合を行います。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public virtual bool Ready()
        {
            return true;
        }

        /// <summary>
        ///     登録済みサービスやイベント購読を解除します。
        /// </summary>
        public virtual void Shutdown()
        {
        }
    }
}
