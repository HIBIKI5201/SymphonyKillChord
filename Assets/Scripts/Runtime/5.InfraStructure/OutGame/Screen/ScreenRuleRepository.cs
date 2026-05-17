using KillChord.Runtime.Application.OutGame.Screen;
using KillChord.Runtime.Domain.OutGame.Screen;
using System;
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
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (data.Entries == null)
            {
                throw new ArgumentNullException(nameof(data.Entries));
            }

            foreach (var entry in data.Entries)
            {
                if (!_rules.TryAdd(entry.ScreenId, new ScreenTransitionRule(entry.TransitionType, entry.IsAddToHistory)))
                {
                    throw new InvalidOperationException($"Duplicate ScreenId rule detected: {entry.ScreenId}");
                }
            }
        }
        /// <summary>
        ///     指定画面の遷移ルールを取得します。
        /// </summary>
        public ScreenTransitionRule GetRule(ScreenId screenId)
        {
            if (_rules.TryGetValue(screenId, out var rule))
            {
                return rule;
            }
            throw new KeyNotFoundException($"Screen transition rule is not defined for: {screenId}");
        }

        private readonly Dictionary<ScreenId, ScreenTransitionRule> _rules = new();
    }
}
