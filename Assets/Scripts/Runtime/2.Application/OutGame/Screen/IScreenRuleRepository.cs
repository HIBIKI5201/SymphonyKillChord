using KillChord.Runtime.Domain.OutGame.Screen;

namespace KillChord.Runtime.Application.OutGame.Screen
{
    /// <summary>
    ///     画面遷移ルール取得のインターフェース。
    /// </summary>
    public interface IScreenRuleRepository
    {
        /// <summary>
        ///     指定画面の遷移ルールを取得します。
        /// </summary>
        ScreenTransitionRule GetRule(ScreenId screenId);
    }
}
