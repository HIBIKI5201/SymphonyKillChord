using KillChord.Runtime.Application.OutGame.SkillTree;
using KillChord.Runtime.Domain.OutGame.SkillTree;
using KillChord.Runtime.Domain.Persistent.Savedata;
using SymphonyFrameWork.System.SaveSystem;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.InfraStructure.OutGame.SkillTree
{
    /// <summary>
    ///     セーブデータから解放済みスキルノードを取得するRepositoryです。
    /// </summary>
    public sealed class SavedataSkillUnlockRepository : ISkillUnlockRepository
    {
        /// <summary>
        ///     解放済みスキルノードIDを読み込みます。
        /// </summary>
        /// <param name="cancellationToken"> キャンセルトークンです。 </param>
        /// <returns> 解放済みスキルノードID一覧です。 </returns>
        public async ValueTask<IReadOnlyCollection<SkillNodeId>> LoadUnlockedNodeIdsAsync(
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            SaveData saveData = SaveStore.IsLoaded<SaveData>()
                ? SaveStore.Get<SaveData>()
                : await SaveStore.LoadAsync<SaveData>();
            cancellationToken.ThrowIfCancellationRequested();

            if (saveData?.SkillUnlock == null)
            {
                throw new InvalidOperationException("スキル解放情報がセーブデータに存在しません。");
            }

            int[] unlockedNodeIds = saveData.SkillUnlock.UnlockedSkillNodeIds;
            if (unlockedNodeIds == null || unlockedNodeIds.Length == 0)
            {
                return Array.Empty<SkillNodeId>();
            }

            List<SkillNodeId> result = new List<SkillNodeId>(unlockedNodeIds.Length);
            for (int i = 0; i < unlockedNodeIds.Length; i++)
            {
                result.Add(new SkillNodeId(unlockedNodeIds[i]));
            }

            return result;
        }
    }
}
