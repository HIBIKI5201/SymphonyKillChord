using KillChord.Runtime.Application.OutGame.Screen;
using KillChord.Runtime.Domain.OutGame.Screen;
using System.Collections.Generic;

namespace KillChord.Runtime.InfraStructure.OutGame.Screen
{
    /// <summary>
    ///     画面遷移ルールリポジトリクラス。
    /// </summary>
    public sealed class ScreenRuleRepository : IScreenRuleRepository
    {
        /// <summary>
        ///     画面遷移ルールリポジトリを初期化します。
        /// </summary>
        public ScreenRuleRepository(ScreenRuleData data)
        {
            foreach (var entry in data.Entries)
            {
                _rules[entry.ScreenId] = new ScreenTransitionRule(entry.TransitionType, entry.IsAddToHistory);
            }
        }

        /// <summary>
        ///     指定画面の遷移ルールを取得します。
        /// </summary>
        public ScreenTransitionRule GetRule(ScreenId screenId)
        {
            return _rules[screenId];
        }

        private readonly Dictionary<ScreenId, ScreenTransitionRule> _rules = new();
    }
}
