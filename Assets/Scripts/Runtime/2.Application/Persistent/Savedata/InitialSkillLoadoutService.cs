using KillChord.Runtime.Domain.Persistent.Savedata;
using System;
using System.Collections.Generic;

namespace KillChord.Runtime.Application.Persistent.Savedata
{
    /// <summary>
    ///     セーブデータへ初期解放・初期装備スキルを補完するサービス。
    ///     <para>
    ///         起動時の補完だけでなく、セーブデータリセット後の再補完にも使用する。
    ///     </para>
    /// </summary>
    public sealed class InitialSkillLoadoutService
    {
        /// <summary>
        ///     初期スキル構成を保持するサービスを生成します。
        /// </summary>
        /// <param name="unlockedSkillIds"> 初期解放スキルIDです。 </param>
        /// <param name="equippedSkillIds"> 初期装備スキルIDです。 </param>
        /// <exception cref="ArgumentNullException"> いずれかがnullの場合にスローされます。 </exception>
        public InitialSkillLoadoutService(
            IReadOnlyList<int> unlockedSkillIds,
            IReadOnlyList<int> equippedSkillIds)
        {
            _unlockedSkillIds = unlockedSkillIds ?? throw new ArgumentNullException(nameof(unlockedSkillIds));
            _equippedSkillIds = equippedSkillIds ?? throw new ArgumentNullException(nameof(equippedSkillIds));
        }

        /// <summary>
        ///     未設定の解放・装備スキルにのみ初期値を補完する。
        /// </summary>
        /// <param name="saveData"> 対象のセーブデータです。 </param>
        /// <returns> 更新した場合はtrue。 </returns>
        /// <exception cref="ArgumentNullException"> saveDataがnullの場合にスローされます。 </exception>
        public bool TryApply(SaveData saveData)
        {
            if (saveData == null)
            {
                throw new ArgumentNullException(nameof(saveData));
            }

            bool isChanged = false;

            // 保持している初期値をセーブデータへ直接渡すと、以降の変更が初期値を汚染する。
            // 補完のたびに複製を渡す。
            if (saveData.SkillUnlock.UnlockedSkillIds.Length == 0)
            {
                int[] unlockedSkillIds = new int[_unlockedSkillIds.Count];
                for (int i = 0; i < _unlockedSkillIds.Count; i++)
                {
                    unlockedSkillIds[i] = _unlockedSkillIds[i];
                }

                saveData.SkillUnlock.SetUnlockedSkillIds(unlockedSkillIds);
                isChanged = true;
            }

            if (saveData.SkillBuild.EquipmentSkillIDs.Count == 0)
            {
                saveData.SkillBuild.SetEquipmentSkillIDs(new List<int>(_equippedSkillIds));
                isChanged = true;
            }

            return isChanged;
        }

        private readonly IReadOnlyList<int> _unlockedSkillIds;
        private readonly IReadOnlyList<int> _equippedSkillIds;
    }
}
