using KillChord.Runtime.Application.InGame.Skill;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.Domain.Player;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.Player
{
    /// <summary>
    ///     スキルのデータを保持し、提供するためのリポジトリクラス。
    /// </summary>
    [CreateAssetMenu(fileName = "SkillRepository", menuName = "Scriptable Objects/SkillRepository")]
    public class SkillRepository : ScriptableObject, ISkillRepository
    {
        /// <summary>
        ///     指定されたスキル ID に対応する SkillTemplateAsset を取得しようとします。
        /// </summary>
        /// <param name="id"></param>
        /// <param name="skillData"></param>
        /// <returns></returns>
        public bool TryGetSkill(SkillId id, out SkillTemplate skillData)
        {
            EnsureSkillTemplateAssetMap();
            return _skillTemplateMap.TryGetValue(id, out skillData);
        }

        public SkillDefinition GetSkill(SkillId id, double bpm)
        {
            EnsureSkillTemplateAssetMap();
            if (!_skillTemplateMap.TryGetValue(id, out SkillTemplate skillData))
            {
                throw new KeyNotFoundException($"指定されたスキルID {id} に対応するスキルデータが見つかりませんでした。");
            }
            return skillData.ToSkillDefinition(bpm);
        }

        [SerializeField] private SkillTemplateAsset[] _skillDataAssets;

        private Dictionary<SkillId, SkillTemplate> _skillTemplateMap;

        /// <summary>
        ///     Inspector 上の設定が変わった際に検索用辞書を破棄する。
        /// </summary>
        private void OnValidate()
        {
            _skillTemplateMap = null;
        }

        /// <summary>
        ///     SkillTemplateAsset の ID 検索用辞書を構築する。
        /// </summary>
        private void EnsureSkillTemplateAssetMap()
        {
            if (_skillTemplateMap != null) { return; }

            _skillTemplateMap = new Dictionary<SkillId, SkillTemplate>();

            if (_skillDataAssets == null) { return; }

            for (int i = 0; i < _skillDataAssets.Length; i++)
            {
                SkillTemplateAsset skillTemplateAsset = _skillDataAssets[i];
                if (skillTemplateAsset == null) { continue; }

                if (_skillTemplateMap.ContainsKey(skillTemplateAsset.Id))
                {
                    Debug.LogWarning($"重複したスキルIDが検出されました: {skillTemplateAsset.Id}. このエントリはスキップされます。");
                    continue;
                }

                _skillTemplateMap.Add(skillTemplateAsset.Id, skillTemplateAsset.ToDomain());
            }
        }
    }
}

