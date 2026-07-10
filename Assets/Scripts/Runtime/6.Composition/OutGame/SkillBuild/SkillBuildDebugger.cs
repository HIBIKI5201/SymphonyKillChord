using KillChord.Runtime.Domain.OutGame.SkillBuild;
using KillChord.Runtime.Domain.Player;
using System;
using UnityEngine;

namespace KillChord.Runtime.Composition.OutGame.SkillBuild
{
    /// <summary>
    ///     現在のスキル編成を Inspector で確認するためのデバッグ表示クラス。
    /// </summary>
    public sealed class SkillBuildDebugger : MonoBehaviour
    {
        /// <summary>
        ///     デバッグ表示対象のスキル編成を設定する。
        /// </summary>
        /// <param name="skillBuildDefinition"> 表示対象のスキル編成。 </param>
        /// <exception cref="ArgumentNullException"></exception>
        public void Initialize(SkillBuildDefinition skillBuildDefinition)
        {
            if (skillBuildDefinition == null)
            {
                throw new ArgumentNullException(nameof(skillBuildDefinition));
            }

            _skillBuildDefinition = skillBuildDefinition;
            Refresh();
        }

        private const string EMPTY_ANIMATION_KEY = "<none>";
        private const string EMPTY_EFFECT_TYPE = "<null>";

        [SerializeField, Tooltip("Inspector に表示する現在のスキル編成スナップショット。")]
        private SkillBuildSlotDebugData[] _slotDebugDataArray = Array.Empty<SkillBuildSlotDebugData>();

        private SkillBuildDefinition _skillBuildDefinition;

        /// <summary>
        ///     再生中に最新状態へ追従する。
        /// </summary>
        private void LateUpdate()
        {
            Refresh();
        }

        /// <summary>
        ///     現在のスキル編成を Inspector 表示用データへ反映する。
        /// </summary>
        private void Refresh()
        {
            if (_skillBuildDefinition == null)
            {
                if (_slotDebugDataArray.Length != 0)
                {
                    _slotDebugDataArray = Array.Empty<SkillBuildSlotDebugData>();
                }

                return;
            }

            EnsureSlotBufferSize(_skillBuildDefinition.EquippedSkills.Count);

            for (int i = 0; i < _skillBuildDefinition.EquippedSkills.Count; i++)
            {
                EquippedSkill equippedSkill = _skillBuildDefinition.EquippedSkills[i];
                ApplySlot(i, equippedSkill.SkillTemplate);
            }
        }

        /// <summary>
        ///     表示用バッファのサイズを必要数へ調整する。
        /// </summary>
        /// <param name="slotCount"> 必要なスロット数。 </param>
        private void EnsureSlotBufferSize(int slotCount)
        {
            if (_slotDebugDataArray.Length == slotCount)
            {
                return;
            }

            SkillBuildSlotDebugData[] newArray = new SkillBuildSlotDebugData[slotCount];
            for (int i = 0; i < slotCount; i++)
            {
                newArray[i] = i < _slotDebugDataArray.Length
                    ? _slotDebugDataArray[i]
                    : new SkillBuildSlotDebugData();
            }

            _slotDebugDataArray = newArray;
        }

        /// <summary>
        ///     指定スロットの表示データを更新する。
        /// </summary>
        /// <param name="slotIndex"> スロット番号。 </param>
        /// <param name="skillData"> スキルデータ。 </param>
        private void ApplySlot(int slotIndex, SkillTemplate skillData)
        {
            if (skillData == null)
            {
                _slotDebugDataArray[slotIndex].Apply(slotIndex, -1, EMPTY_ANIMATION_KEY, EMPTY_EFFECT_TYPE);
                return;
            }

            string animationKey = string.IsNullOrEmpty(skillData.AnimationKey)
                ? EMPTY_ANIMATION_KEY
                : skillData.AnimationKey;
            string effectTypeName = skillData.SkillEffect == null
                ? EMPTY_EFFECT_TYPE
                : skillData.SkillEffect.GetType().Name;

            _slotDebugDataArray[slotIndex].Apply(slotIndex, skillData.Id, animationKey, effectTypeName);
        }

        /// <summary>
        ///     Inspector 表示用のスロット情報。
        /// </summary>
        [Serializable]
        private sealed class SkillBuildSlotDebugData
        {
            [SerializeField, Tooltip("スロット番号。")]
            private int _slotIndex;

            [SerializeField, Tooltip("現在装備しているスキル ID。")]
            private int _skillId;

            [SerializeField, Tooltip("スキル発動時に使用するアニメーションキー。")]
            private string _animationKey;

            [SerializeField, Tooltip("スキル効果の型名。")]
            private string _skillEffectTypeName;

            /// <summary>
            ///     Inspector 表示用データを更新する。
            /// </summary>
            /// <param name="slotIndex"> スロット番号。 </param>
            /// <param name="skillId"> スキル ID。 </param>
            /// <param name="animationKey"> アニメーションキー。 </param>
            /// <param name="skillEffectTypeName"> スキル効果の型名。 </param>
            public void Apply(int slotIndex, int skillId, string animationKey, string skillEffectTypeName)
            {
                _slotIndex = slotIndex;
                _skillId = skillId;
                _animationKey = animationKey;
                _skillEffectTypeName = skillEffectTypeName;
            }
        }
    }
}
