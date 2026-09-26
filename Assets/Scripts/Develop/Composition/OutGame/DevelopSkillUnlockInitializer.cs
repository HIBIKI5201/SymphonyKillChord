using KillChord.Runtime.Composition.OutGame.Bootstrap;
using KillChord.Runtime.Domain.Persistent.Savedata;
using KillChord.Runtime.Utility.Identity;
using SymphonyFrameWork.System.SaveSystem;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace KillChord.Develop.Composition.OutGame
{
    /// <summary>
    ///     開発用に追加解放スキルをセーブデータへ補完する初期化モジュールです。
    ///     <para>
    ///         解放するスキルはInspectorから設定します。空の場合は何もしません。
    ///         リリース時はスキルツリー経由でのみ解放させるため、空のままにしてください。
    ///     </para>
    /// </summary>
    public sealed class DevelopSkillUnlockInitializer : OutGameInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(DevelopSkillUnlockInitializer);

        /// <summary> 実行順です。 </summary>
        public override int Order => 110;

        /// <summary>
        ///     開発用の追加解放スキルをセーブデータへ補完します。
        /// </summary>
        /// <param name="cancellationToken"> キャンセルトークンです。 </param>
        /// <returns> 成功した場合はtrue。 </returns>
        public override async Awaitable<bool> ResourceLoadAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_developUnlockedSkills == null || _developUnlockedSkills.Length == 0)
            {
                return true;
            }

            SaveData saveData = SaveStore.IsLoaded<SaveData>()
                ? SaveStore.Get<SaveData>()
                : await SaveStore.LoadAsync<SaveData>();
            cancellationToken.ThrowIfCancellationRequested();

            if (saveData == null)
            {
                Debug.LogError($"[{nameof(DevelopSkillUnlockInitializer)}] SaveData が取得できませんでした。", this);
                return false;
            }

            if (!TryApplyDevelopSkillUnlocks(saveData))
            {
                return true;
            }

            await SaveStore.SaveAsync<SaveData>();
            return true;
        }

        [SerializeField]
        [SourceDataCollection("Skill")]
        [Tooltip("開発用に追加で解放するスキル。空の場合は何もしません。リリース時は空にしてください。")]
        private DataID[] _developUnlockedSkills;

        /// <summary>
        ///     開発用の追加解放スキルを未所持の場合のみ追加します。
        /// </summary>
        /// <param name="saveData"> 対象のセーブデータです。 </param>
        /// <returns> 更新した場合はtrue。 </returns>
        private bool TryApplyDevelopSkillUnlocks(SaveData saveData)
        {
            List<int> unlockedSkillIds = new(saveData.SkillUnlock.UnlockedSkillIds);
            bool isChanged = false;

            for (int i = 0; i < _developUnlockedSkills.Length; i++)
            {
                int skillId = _developUnlockedSkills[i].Id;
                if (skillId == 0 || unlockedSkillIds.Contains(skillId))
                {
                    continue;
                }

                unlockedSkillIds.Add(skillId);
                isChanged = true;
            }

            if (!isChanged)
            {
                return false;
            }

            unlockedSkillIds.Sort();
            saveData.SkillUnlock.SetUnlockedSkillIds(unlockedSkillIds.ToArray());
            return true;
        }
    }
}
