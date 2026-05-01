using KillChord.Runtime.Domain.OutGame.Screen;

namespace KillChord.Runtime.Application.OutGame.Screen
{
    /// <summary>
    ///     画面表示コマンド。
    /// </summary>
    public readonly struct ShowScreenCommand
    {
        /// <summary> 
        ///     画面表示コマンドを初期化します。
        /// </summary>
        public ShowScreenCommand(ScreenId targetScreenId)
        {
            TargetScreenId = targetScreenId;
        }

        /// <summary> 遷移先画面 ID を取得します。 </summary>
        public ScreenId TargetScreenId { get; }
    }
}
