using KillChord.Runtime.Adaptor.OutGame.SkillTree;
using KillChord.Runtime.View.OutGame.Screen;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.SkillTree
{
    /// <summary>
    ///     プレイヤーステータス画面のViewクラス。
    /// </summary>
    public class PlayerStatusScreenView : ScreenViewBase, IPlayerStatusShowable, IPlayerStatusViewModel
    {
        public PlayerStatusScreenView(VisualElement root, OutGameUIEvent outGameUIEvent) : base(root, outGameUIEvent)
        {
            _healthPreviewLabel = root.Q<Label>(name: E_NAME_HEALTH_PREVIEW_LABEL);
            _attackPreviewLabel = root.Q<Label>(name: E_NAME_ATTACK_PREVIEW_LABEL);
            _criticalChancePreviewLabel = root.Q<Label>(name: E_NAME_CRITICAL_CHANCE_PREVIEW_LABEL);
        }

        /// <summary>
        ///     プレイヤーステータスのデータを反映する。
        /// </summary>
        /// <param name="dto"></param>
        public void Apply(PlayerStatusDTO dto)
        {
            _healthPreviewLabel.text = dto.PlayerHealth.ToString();
            _attackPreviewLabel.text = dto.PlayerAttack.ToString();
            _criticalChancePreviewLabel.text = dto.CriticalChance.ToString();
        }

        private Label _healthPreviewLabel;
        private Label _attackPreviewLabel;
        private Label _criticalChancePreviewLabel;

        private const string E_NAME_HEALTH_PREVIEW_LABEL = "HealthPreviewLabel";
        private const string E_NAME_ATTACK_PREVIEW_LABEL = "AttackPreviewLabel";
        private const string E_NAME_CRITICAL_CHANCE_PREVIEW_LABEL = "CriticalChancePreviewLabel";
    }
}
