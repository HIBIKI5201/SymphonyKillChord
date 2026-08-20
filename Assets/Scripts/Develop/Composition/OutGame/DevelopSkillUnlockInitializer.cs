using KillChord.Runtime.Composition.OutGame.Bootstrap;
using KillChord.Runtime.Domain.Persistent.Savedata;
using KillChord.Runtime.Utility.OutGame.Savedata;
using SymphonyFrameWork.System.ServiceLocate;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace KillChord.Develop.Composition.OutGame
{
    /// <summary>
    ///     開発用に追加解放スキルをセーブデータへ補完する初期化モジュールです。
    /// </summary>
    public sealed class DevelopSkillUnlockInitializer : OutGameInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(DevelopSkillUnlockInitializer);

        /// <summary> 実行順です。 </summary>
        public override int Order => 110;

        private const int SKILL_01_ID = -1127918619;
        private const int SKILL_02_ID = 634126943;
        private const int SKILL_03_ID = 1389048521;

        private static readonly int[] DEVELOP_UNLOCKED_SKILL_IDS =
        {
            SKILL_01_ID,
            SKILL_02_ID,
            SKILL_03_ID
        };

        /// <summary>
        ///     開発用の追加解放スキルをセーブデータへ補完します。
        /// </summary>
        /// <param name="cancellationToken"> キャンセルトークンです。 </param>
        /// <returns> 成功した場合はtrue。 </returns>
        public override async Awaitable<bool> ResourceLoadAsync(CancellationToken cancellationToken)
        {
            if (!ServiceLocator.TryGetInstance(out SavedataSystem savedataSystem))
            {
                Debug.LogError($"[{nameof(DevelopSkillUnlockInitializer)}] SavedataSystem が取得できませんでした。", this);
                return false;
            }

            cancellationToken.ThrowIfCancellationRequested();
            SaveData saveData = await savedataSystem.LoadAsync<SaveData>();
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

            await savedataSystem.SaveAsync(saveData);
            return true;
        }

        /// <summary>
        ///     開発用の追加解放スキルを未所持の場合のみ追加します。
        /// </summary>
        /// <param name="saveData"> 対象のセーブデータです。 </param>
        /// <returns> 更新した場合はtrue。 </returns>
        private static bool TryApplyDevelopSkillUnlocks(SaveData saveData)
        {
            List<int> unlockedSkillIds = new(saveData.SkillUnlock.UnlockedSkillIds);
            bool isChanged = false;

            for (int i = 0; i < DEVELOP_UNLOCKED_SKILL_IDS.Length; i++)
            {
                int skillId = DEVELOP_UNLOCKED_SKILL_IDS[i];
                if (unlockedSkillIds.Contains(skillId))
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
