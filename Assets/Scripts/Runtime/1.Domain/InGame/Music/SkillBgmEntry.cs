using System;
using System.Collections.Generic;
using System.Linq;

namespace KillChord.Runtime.Domain.InGame.Music
{
    /// <summary>
    ///     装備スキルの組み合わせと再生するBGMのキュー名の対応を表す値型オブジェクト。
    /// </summary>
    public readonly struct SkillBgmEntry
    {
        /// <summary>
        ///     装備スキルIDの組み合わせとキュー名から対応を生成する。
        /// </summary>
        /// <param name="skillIds"> 対応する装備スキルIDの集合。 </param>
        /// <param name="cueName"> 再生するBGMのキュー名。 </param>
        public SkillBgmEntry(IReadOnlyList<int> skillIds, string cueName)
        {
            _skillIds = NormalizeSkillIds(skillIds);
            _cueName = cueName ?? string.Empty;
        }

        /// <summary> 正規化された装備スキルIDの集合。 </summary>
        public IReadOnlyList<int> SkillIds => _skillIds;

        /// <summary> 再生するBGMのキュー名。 </summary>
        public string CueName => _cueName;

        /// <summary>
        ///     指定された正規化済みスキルID集合が、この対応のスキルID集合と一致するかを判定する。
        /// </summary>
        /// <param name="normalizedSkillIds"> 正規化済みの装備スキルID集合。 </param>
        /// <returns> 一致する場合は true、それ以外の場合は false を返す。 </returns>
        public bool Matches(IReadOnlyList<int> normalizedSkillIds)
        {
            if (normalizedSkillIds == null || normalizedSkillIds.Count != _skillIds.Length)
            {
                return false;
            }

            // 双方とも昇順ソート済みの前提で、要素を順に比較する
            for (int i = 0; i < _skillIds.Length; i++)
            {
                if (_skillIds[i] != normalizedSkillIds[i])
                {
                    return false;
                }
            }

            return true;
        }

        private readonly int[] _skillIds;
        private readonly string _cueName;

        /// <summary>
        ///     装備スキルIDの集合を、順序を区別しない比較のために昇順ソートかつ重複除去して正規化する。
        /// </summary>
        /// <param name="skillIds"> 正規化対象の装備スキルID集合。 </param>
        /// <returns> 正規化された装備スキルID配列。 </returns>
        internal static int[] NormalizeSkillIds(IReadOnlyList<int> skillIds)
        {
            if (skillIds == null || skillIds.Count == 0)
            {
                return Array.Empty<int>();
            }

            return skillIds.Distinct().OrderBy(id => id).ToArray();
        }
    }
}
