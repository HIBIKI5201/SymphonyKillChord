using KillChord.Runtime.Utility.Constant;
using KillChord.Runtime.Utility.Identity;
using System;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.OutGame.SkillBuild
{
    /// <summary>
    ///     ゲーム開始時点の初期解放・初期装備スキルを定義するアセット。
    /// </summary>
    [CreateAssetMenu(fileName = nameof(InitialSkillLoadoutAsset),
        menuName = PathConst.CREATE_ASSET_MENU_PATH + "SkillBuild/" + nameof(InitialSkillLoadoutAsset))]
    public sealed class InitialSkillLoadoutAsset : ScriptableObject
    {
        /// <summary>
        ///     初期解放済みスキルの ID 配列を取得する。
        /// </summary>
        /// <returns> 初期解放済みスキルの ID 配列。 </returns>
        public int[] GetUnlockedSkillIds()
        {
            return ToIds(_unlockedSkillIds);
        }

        /// <summary>
        ///     初期装備済みスキルの ID 配列を取得する。
        /// </summary>
        /// <returns> 初期装備済みスキルの ID 配列。 </returns>
        public int[] GetEquippedSkillIds()
        {
            return ToIds(_equippedSkillIds);
        }

        [SerializeField, SourceDataCollection("Skill"), Tooltip("ゲーム開始時に解放済みとするスキルの一覧です。")]
        private DataID[] _unlockedSkillIds;

        [SerializeField, SourceDataCollection("Skill"), Tooltip("ゲーム開始時に装備済みとするスキルの一覧です。")]
        private DataID[] _equippedSkillIds;

        /// <summary>
        ///     DataID 配列を数値 ID 配列へ変換する。
        /// </summary>
        /// <param name="dataIds"> 変換元の DataID 配列。 </param>
        /// <returns> 変換後の数値 ID 配列。 </returns>
        private static int[] ToIds(DataID[] dataIds)
        {
            if (dataIds == null || dataIds.Length == 0)
            {
                return Array.Empty<int>();
            }

            int[] ids = new int[dataIds.Length];
            for (int i = 0; i < dataIds.Length; i++)
            {
                ids[i] = dataIds[i].Id;
            }

            return ids;
        }
    }
}
