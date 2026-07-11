using System.Threading;
using UnityEngine;

namespace KillChord.Runtime.Composition.Persistent.Bootstrap
{
    /// <summary>
    ///     常駐シーン初期化モジュールの共通インターフェースです。
    /// </summary>
    public interface IPersistentInitializationModule
    {
        /// <summary> モジュール名です。 </summary>
        string ModuleName { get; }

        /// <summary> 実行順です。 </summary>
        int Order { get; }

        /// <summary>
        ///     単体で実行できる初期化を行います。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        bool Init();

        /// <summary>
        ///     非同期のリソースロードを行います。
        /// </summary>
        /// <param name="cancellationToken"> キャンセルトークンです。 </param>
        /// <returns> 成功した場合はtrue。 </returns>
        Awaitable<bool> ResourceLoadAsync(CancellationToken cancellationToken);

        /// <summary>
        ///     ロード後に必要な生成とサービス登録を行います。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        bool Build();

        /// <summary>
        ///     他モジュールとの結合を行います。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        bool Ready();

        /// <summary>
        ///     登録済みサービスやイベント購読を解除します。
        /// </summary>
        void Shutdown();
    }
}
