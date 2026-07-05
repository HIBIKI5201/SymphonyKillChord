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
        ///     指定されたスキル ID に対応する SkillDataAsset を取得しようとします。
        /// </summary>
        /// <param name="id"></param>
        /// <param name="skillData"></param>
        /// <returns></returns>
        public bool TryGetSkill(int id, out SkillData skillData)
        {
            EnsureSkillDataAssetMap();
            return _skillDataAssetMap.TryGetValue(id, out skillData);
        }

        public SkillDefinition GetSkill(int id, double bpm)
        {
            EnsureSkillDataAssetMap();
            if (!_skillDataAssetMap.TryGetValue(id, out SkillData skillData))
            {
                throw new KeyNotFoundException($"指定されたスキルID {id} に対応するスキルデータが見つかりませんでした。");
            }
            return skillData.ToSkillDefinition(bpm);
        }

        [SerializeField] private SkillDataAsset[] _skillDataAssets;

        private Dictionary<int, SkillData> _skillDataAssetMap;

        /// <summary>
        ///     Inspector 上の設定が変わった際に検索用辞書を破棄する。
        /// </summary>
        private void OnValidate()
        {
            _skillDataAssetMap = null;
        }

        /// <summary>
        ///     SkillDataAsset の ID 検索用辞書を構築する。
        /// </summary>
        private void EnsureSkillDataAssetMap()
        {
            if (_skillDataAssetMap != null) { return; }

            _skillDataAssetMap = new Dictionary<int, SkillData>();

            if (_skillDataAssets == null) { return; }

            for (int i = 0; i < _skillDataAssets.Length; i++)
            {
                SkillDataAsset skillDataAsset = _skillDataAssets[i];
                if (skillDataAsset == null) { continue; }

                if (_skillDataAssetMap.ContainsKey(skillDataAsset.Id))
                {
                    Debug.LogWarning($"重複したスキルIDが検出されました: {skillDataAsset.Id}. このエントリはスキップされます。");
                    continue;
                }

                _skillDataAssetMap.Add(skillDataAsset.Id, skillDataAsset.ToDomain());
            }
        }
    }
}
