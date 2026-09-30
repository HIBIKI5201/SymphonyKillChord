using System;

namespace KillChord.Runtime.Application.Persistent.SceneManagement
{
    /// <summary>
    ///     シーンの初期化完了を購読するためのインターフェースです。
    /// </summary>
    public interface ISceneInitializationNotifier
    {
        /// <summary> シーンの初期化が成功で完了したときに、そのシーン名を通知します。 </summary>
        event Action<string> OnSceneInitialized;
    }
}
