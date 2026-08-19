using KillChord.Runtime.Composition.OutGame.Bootstrap;
using KillChord.Runtime.Domain.Persistent.Savedata;
using SymphonyFrameWork.System.SaveSystem;
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

        private const int SKILL_00_ID = -876453005;
        private const int SKILL_01_ID = -1127918619;
        private const int SKILL_02_ID = 634126943;
        private const int SKILL_03_ID = 1389048521;
        private const int SKILL_04_ID = -860903574;
        private const int SKILL_05_ID = -1146578948;
        private const int SKILL_06_ID = 581027398;
        private const int SKILL_07_ID = 1437005520;
        private const int SKILL_08_ID = -988157119;
        private const int SKILL_09_ID = -1306600489;
        private const int SKILL_10_ID = -757509582;
        private const int SKILL_13_ID = 1271923592;

        private static readonly int[] DEVELOP_UNLOCKED_SKILL_IDS =
        {
                SKILL_00_ID,
    SKILL_01_ID,
    SKILL_02_ID,
    SKILL_03_ID,
    SKILL_04_ID,
    SKILL_05_ID,
    SKILL_06_ID,
    SKILL_07_ID,
    SKILL_08_ID,
    SKILL_09_ID,
    SKILL_10_ID,
    SKILL_13_ID
        };

        /// <summary>
        ///     開発用の追加解放スキルをセーブデータへ補完します。
        /// </summary>
        /// <param name="cancellationToken"> キャンセルトークンです。 </param>
        /// <returns> 成功した場合はtrue。 </returns>
        public override async Awaitable<bool> ResourceLoadAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
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
