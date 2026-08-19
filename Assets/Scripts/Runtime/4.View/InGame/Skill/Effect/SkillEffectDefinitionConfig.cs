using KillChord.Runtime.Adaptor.InGame.Skill.Effect;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Skill.Effect
{
    /// <summary>
    ///     スキルエフェクト1種類分の生成設定を保持するConfig。
    /// </summary>
    [CreateAssetMenu(
        fileName = "SkillEffectDefinitionConfig",
        menuName = "KillChord/View/Skill/Skill Effect Definition")]
    public sealed class SkillEffectDefinitionConfig : ScriptableObject
    {
        /// <summary> エフェクトの文字列キーです。 </summary>
        public string Key => _key;

        /// <summary> 文字列キーから導出したエフェクトIDです。 </summary>
        public SkillEffectId Id => SkillEffectId.FromKey(_key);

        /// <summary> 生成するエフェクトのプレハブです。 </summary>
        public SkillEffectInstance Prefab => _prefab;

        /// <summary> エフェクトの配置方式です。 </summary>
        public SkillEffectAttachMode AttachMode => _attachMode;

        /// <summary> シーンロード時に事前生成する数です。 </summary>
        public int PrewarmCount => _prewarmCount;

        /// <summary> プールが保持する最大数です。 </summary>
        public int MaxPoolSize => Mathf.Max(_prewarmCount, _maxPoolSize);

        /// <summary> 設定として有効かどうかです。 </summary>
        public bool IsValid => _prefab != null && Id.IsValid;

        [SerializeField, Tooltip("エフェクトを識別する文字列キーです。IDはこの文字列から導出します。")]
        private string _key;

        [SerializeField, Tooltip("生成するエフェクトのプレハブです。")]
        private SkillEffectInstance _prefab;

        [SerializeField, Tooltip("エフェクトの配置方式です。")]
        private SkillEffectAttachMode _attachMode = SkillEffectAttachMode.PlayerPoint;

        [SerializeField, Min(0), Tooltip("シーンロード時に事前生成する数です。同時再生数の想定値を設定します。")]
        private int _prewarmCount = 2;

        [SerializeField, Min(1), Tooltip("プールが保持する最大数です。")]
        private int _maxPoolSize = 8;
    }
}
