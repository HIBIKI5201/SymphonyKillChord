using KillChord.Runtime.Application.Persistent.SceneManagement;
using SymphonyFrameWork.System.ServiceLocate;

namespace KillChord.Runtime.Composition.Persistent.Bootstrap
{
    /// <summary>
    ///     常駐シーンの初期化を経て起動されたかを判定するクラスです。
    ///     <para>
    ///         エディタで任意のシーンを開いて再生すると、開いていたシーンの Start が
    ///         常駐シーンの起動より先に走り、その後の起動時のシーン整理でアンロードされる。
    ///         このシーンの初期化は常駐サービスが揃わず失敗するため、初期化を始める前にこの判定で止める。
    ///     </para>
    /// </summary>
    public static class PersistentBootState
    {
        /// <summary> 常駐シーンの初期化を経て起動された場合はtrue。 </summary>
        public static bool IsBootedThroughPersistentFlow =>
            ServiceLocator.IsExistInstance<ISceneInitializationReadiness>();
    }
}
