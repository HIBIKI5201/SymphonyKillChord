using KillChord.Runtime.View.InGame.Skill;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Skill
{
    /// <summary>
    ///     スキル入力進行UIの表示設定。
    /// </summary>
    [CreateAssetMenu(
        fileName = nameof(SkillInputProgressViewConfigAsset),
        menuName = "KillChord/InGame/Skill/SkillInputProgressViewConfig")]
    public class SkillInputProgressViewConfigAsset : ScriptableObject
    {
        /// <summary>
        ///     View層用の表示設定を生成する。
        /// </summary>
        public SkillInputProgressViewconfig Create()
        {
            List<SkillBeatVisualSetting> settings = new();

            for (int i = 0; i < _settings.Count; i++)
            {
                settings.Add(_settings[i].Create());
            }

            return new SkillInputProgressViewconfig(settings);
        }

        [SerializeField, Tooltip("拍子ごとの表示設定。")]
        private List<SkillBeatVisualSettingAsset> _settings = new();
    }
}
