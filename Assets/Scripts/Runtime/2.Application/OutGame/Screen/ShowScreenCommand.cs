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
        public ShowScreenCommand(ScreenId targetScreenId, string targetSceneName = null)
        {
            TargetScreenId = targetScreenId;
            TargetSceneName = targetSceneName;
        }

        /// <summary> 遷移先画面 ID を取得します。 </summary>
        public ScreenId TargetScreenId { get; }
        /// <summary> 遷移先シーン名を取得します。 </summary>
        public string TargetSceneName { get; }
    }
}
