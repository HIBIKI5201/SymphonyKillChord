using KillChord.Runtime.Utility.Identity;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Skill.Effect
{
    /// <summary>
    ///     スキルIDとエフェクト定義の対応を保持するConfig。
    ///     装備スキルに応じた事前生成対象の解決に使用する。
    /// </summary>
    [CreateAssetMenu(
        fileName = "SkillEffectCatalogConfig",
        menuName = "KillChord/View/Skill/Skill Effect Catalog")]
    public sealed class SkillEffectCatalogConfig : ScriptableObject
    {
        /// <summary> 登録されている全エフェクト定義です。 </summary>
        public IReadOnlyList<SkillEffectDefinitionConfig> CommonDefinitions => _commonDefinitions;

        /// <summary>
        ///     指定スキルに紐づくエフェクト定義一覧を取得する。
        /// </summary>
        /// <param name="skillId"> 対象のスキルIDです。 </param>
        /// <param name="definitions"> 取得したエフェクト定義一覧です。 </param>
        /// <returns> 定義が存在する場合はtrue。 </returns>
        public bool TryGetDefinitions(int skillId, out IReadOnlyList<SkillEffectDefinitionConfig> definitions)
        {
            EnsureIndexBuilt();
            if (_definitionIndex.TryGetValue(skillId, out SkillEffectDefinitionConfig[] entryDefinitions))
            {
                definitions = entryDefinitions;
                return true;
            }

            definitions = Array.Empty<SkillEffectDefinitionConfig>();
            return false;
        }

        [SerializeField, Tooltip("スキルごとのエフェクト定義の対応表です。")]
        private SkillEffectEntry[] _entries;

        [SerializeField, Tooltip("装備状況に関わらず常に事前生成するエフェクト定義です。")]
        private SkillEffectDefinitionConfig[] _commonDefinitions;

        /// <summary>
        ///     スキルID索引を必要時に構築する。
        /// </summary>
        private void EnsureIndexBuilt()
        {
            if (_definitionIndex != null)
            {
                return;
            }

            _definitionIndex = new Dictionary<int, SkillEffectDefinitionConfig[]>();
            if (_entries == null)
            {
                return;
            }

            for (int i = 0; i < _entries.Length; i++)
            {
                SkillEffectEntry entry = _entries[i];
                if (entry == null || entry.SkillId == 0 || entry.Definitions == null)
                {
                    continue;
                }

                _definitionIndex[entry.SkillId] = entry.Definitions;
            }
        }

        /// <summary>
        ///     参照が変更された際に索引を破棄する。
        /// </summary>
        private void OnValidate()
        {
            _definitionIndex = null;
        }

        private Dictionary<int, SkillEffectDefinitionConfig[]> _definitionIndex;

        /// <summary>
        ///     スキル1つ分のエフェクト定義対応を保持するクラス。
        /// </summary>
        [Serializable]
        private sealed class SkillEffectEntry
        {
            /// <summary> 対象のスキルIDです。 </summary>
            public int SkillId => _skillId.Id;

            /// <summary> 対象スキルが使用するエフェクト定義です。 </summary>
            public SkillEffectDefinitionConfig[] Definitions => _definitions;

            [SerializeField, SourceDataCollection("Skill"), Tooltip("対象のスキルIDです。")]
            private DataID _skillId;

            [SerializeField, Tooltip("対象スキルが使用するエフェクト定義です。")]
            private SkillEffectDefinitionConfig[] _definitions;
        }
    }
}
