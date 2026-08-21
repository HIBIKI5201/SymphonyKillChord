using KillChord.Runtime.Utility.Identity;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Skill.Effect
{
    /// <summary>
    ///     スキルIDとエフェクトプレハブの対応だけを保持するConfig。
    ///     エフェクトの内容や配置はプレハブ側が持ち、ここは対応表に徹する。
    /// </summary>
    [CreateAssetMenu(
        fileName = "SkillEffectCatalogConfig",
        menuName = "KillChord/View/Skill/Skill Effect Catalog")]
    public sealed class SkillEffectCatalogConfig : ScriptableObject
    {
        /// <summary>
        ///     指定スキルに対応するエフェクトプレハブを取得する。
        /// </summary>
        /// <param name="skillId"> 対象のスキルIDです。 </param>
        /// <param name="prefab"> 取得したエフェクトプレハブです。 </param>
        /// <returns> 対応するプレハブが存在する場合はtrue。 </returns>
        public bool TryGetPrefab(int skillId, out SkillEffectInstance prefab)
        {
            EnsureIndexBuilt();
            return _prefabIndex.TryGetValue(skillId, out prefab) && prefab != null;
        }

        [SerializeField, Tooltip("スキルとエフェクトプレハブの対応表です。")]
        private SkillEffectEntry[] _entries;

        /// <summary>
        ///     スキルID索引を必要時に構築する。
        /// </summary>
        private void EnsureIndexBuilt()
        {
            if (_prefabIndex != null)
            {
                return;
            }

            _prefabIndex = new Dictionary<int, SkillEffectInstance>();
            if (_entries == null)
            {
                return;
            }

            for (int i = 0; i < _entries.Length; i++)
            {
                SkillEffectEntry entry = _entries[i];
                if (entry == null || entry.SkillId == 0 || entry.Prefab == null)
                {
                    continue;
                }

                _prefabIndex[entry.SkillId] = entry.Prefab;
            }
        }

        /// <summary>
        ///     参照が変更された際に索引を破棄する。
        /// </summary>
        private void OnValidate()
        {
            _prefabIndex = null;
        }

        private Dictionary<int, SkillEffectInstance> _prefabIndex;

        /// <summary>
        ///     スキル1つ分のエフェクト対応を保持するクラス。
        /// </summary>
        [Serializable]
        private sealed class SkillEffectEntry
        {
            /// <summary> 対象のスキルIDです。 </summary>
            public int SkillId => _skillId.Id;

            /// <summary> 対象スキルのエフェクトプレハブです。 </summary>
            public SkillEffectInstance Prefab => _prefab;

            [SerializeField, SourceDataCollection("Skill"), Tooltip("対象のスキルIDです。")]
            private DataID _skillId;

            [SerializeField, Tooltip("対象スキルのエフェクトプレハブです。")]
            private SkillEffectInstance _prefab;
        }
    }
}
