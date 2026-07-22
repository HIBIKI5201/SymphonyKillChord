using System;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Skill
{
    /// <summary>
    ///     スキル入力の拍子ごとの表示設定を保持するクラス。
    /// </summary>
    [Serializable]
    public class SkillBeatVisualSettingConfig
    {
        /// <summary>
        ///     Viewで使用するSkillBeatVisualSettingを生成する。
        /// </summary>
        /// <returns> 生成されたSkillBeatVisualSetting。 </returns>
        public SkillBeatVisualSetting Create()
        {
            return new SkillBeatVisualSetting(
                _beatType,
                _normalColor,
                _activeColor,
                _icon
            );
        }

        [SerializeField, Min(0), Tooltip("対応する拍子のBeatType値。")]
        private int _beatType;

        [SerializeField, Tooltip("未入力時の色。")]
        private Color _normalColor = Color.gray;

        [SerializeField, Tooltip("入力済み時の色。")]
        private Color _activeColor = Color.white;

        [SerializeField, Tooltip("表示アイコン。未設定なら数字を表示する。")]
        private Sprite _icon;
    }
}
