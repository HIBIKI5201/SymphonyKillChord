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
            if (_settings == null || _settings.Count == 0)
            {
                throw new System.InvalidOperationException("スキル入力進行UIの表示設定が存在しません。");
            }

            List<SkillBeatVisualSetting> settings = new();
            HashSet<int> seenBeatTypes = new();

            for (int i = 0; i < _settings.Count; i++)
            {
                SkillBeatVisualSettingAsset asset = _settings[i]
                    ?? throw new System.InvalidOperationException($"スキル入力進行UIの表示設定がnullです。インデックス: {i}");


                SkillBeatVisualSetting setting = asset.Create();
                if (!seenBeatTypes.Add(setting.BeatType))
                {
                    throw new System.InvalidOperationException(
                    $"{name}: BeatType {setting.BeatType} が重複しています。");
                }

                settings.Add(setting);
            }

            return new SkillInputProgressViewconfig(settings);
        }

        [SerializeField, Tooltip("拍子ごとの表示設定。")]
        private List<SkillBeatVisualSettingAsset> _settings = new();
    }
}
