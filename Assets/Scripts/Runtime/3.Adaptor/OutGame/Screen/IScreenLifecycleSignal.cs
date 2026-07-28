using KillChord.Runtime.Domain.OutGame.Screen;
using System;

namespace KillChord.Runtime.Adaptor.OutGame.Screen
{
    /// <summary>
    ///     画面のライフサイクルイベントを通知するインターフェース。
    /// </summary>
    public interface IScreenLifecycleSignal
    {
        /// <summary>
        ///     画面が表示される直前に発火するイベント。
        /// </summary>
        event Action<ScreenId> OnScreenWillShow;
    }
}
