using KillChord.Runtime.Application;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.Domain.OutGame.SkillBuild;
using KillChord.Runtime.Domain.Persistent.Savedata;
using KillChord.Runtime.InfraStructure.Player;
using KillChord.Runtime.Utility.Constant;
using SymphonyFrameWork.System.SaveSystem;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure
{
    /// <summary>
    ///     入手済みスキルリポジトリの実装クラス。
    /// </summary>
    [CreateAssetMenu(fileName = nameof(OwnedSkillRepository),
        menuName = PathConst.CREATE_ASSET_MENU_PATH + "SkillBuild/" + nameof(OwnedSkillRepository))]
    public class OwnedSkillRepository : ScriptableObject, IOwnedSkillRepository
    {
        /// <summary>
        ///     入手済みスキル一覧を取得する。
        /// </summary>
        /// <returns> 入手済みスキル一覧です。 </returns>
        public async ValueTask<IReadOnlyList<EquippedSkill>> GetOwnedSkills()
        {
            if (_ownedSkills == null)
            {
                await LoadOwnedSkillsAsync();
            }

            // 入手済みスキルのコピーを返す。
            return new List<EquippedSkill>(_ownedSkills).AsReadOnly();
        }

        /// <summary>
        ///    セーブデータから入手済みスキルをロードする。
        /// </summary>
        /// <returns> 入手済みスキルのリスト。 </returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async ValueTask<IReadOnlyList<EquippedSkill>> LoadOwnedSkillsAsync()
        {
            ValidateDependencies();
            SaveData saveData = SaveStore.IsLoaded<SaveData>()
                ? SaveStore.Get<SaveData>()
                : await SaveStore.LoadAsync<SaveData>();
            BuildOwnedSkills(saveData.SkillUnlock.UnlockedSkillIds);

            // 入手済みスキルのコピーを返す。
            return new List<EquippedSkill>(_ownedSkills).AsReadOnly();
        }

        [SerializeField, Tooltip("スキル ID から SkillTemplate を取得するリポジトリ。")]
        private SkillRepository _skillRepository;

        private List<EquippedSkill> _ownedSkills;

        /// <summary>
        ///     依存関係が設定されているか確認する。
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        private void ValidateDependencies()
        {
            if (_skillRepository == null)
            {
                throw new InvalidOperationException($"{nameof(_skillRepository)}: SkillRepository が設定されていません。");
            }
        }

        /// <summary>
        ///    入手済みスキルを構築する。
        /// </summary>
        /// <param name="skillIds"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private void BuildOwnedSkills(IReadOnlyList<int> skillIds)
        {
            _ownedSkills ??= new List<EquippedSkill>();
            _ownedSkills.Clear();

            if (skillIds == null) { return; }

            // 重複するスキル ID を除外するための HashSet を作成する。
            HashSet<int> loadedSkillIds = new HashSet<int>();

            for (int i = 0; i < skillIds.Count; i++)
            {
                int skillId = skillIds[i];
                if (!loadedSkillIds.Add(skillId)) { continue; }

                if (!_skillRepository.TryGetSkill(new SkillId(skillId), out var skillData))
                {
#if UNITY_EDITOR
                    Debug.LogWarning($"SkillRepository からスキル ID {skillId} の SkillTemplate を取得できませんでした。");
#endif
                    continue;
                }

                _ownedSkills.Add(new EquippedSkill(skillData));
            }
        }
    }
}

