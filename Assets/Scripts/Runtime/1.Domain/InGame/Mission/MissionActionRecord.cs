using System.Collections.Generic;

namespace KillChord.Runtime.Domain.InGame.Mission
{
    /// <summary>
    ///     プレイヤー行動の発動回数を記録するEntityクラス。
    /// </summary>
    public class MissionActionRecord
    {
        /// <summary>
        ///     行動の発動を記録します。すでに記録されている行動の場合はカウントを増やします。
        /// </summary>
        /// <param name="actionKind"> 行動の種別です。 </param>
        public void RecordAction(MissionActionKind actionKind)
        {
            if (_actionCounts.ContainsKey(actionKind))
            {
                _actionCounts[actionKind]++;
            }
            else
            {
                _actionCounts[actionKind] = 1;
            }
        }

        /// <summary>
        ///     指定した行動の発動回数を取得します。記録されていない場合は0を返します。
        /// </summary>
        /// <param name="actionKind"> 行動の種別です。 </param>
        /// <returns> 発動回数です。 </returns>
        public int GetCount(MissionActionKind actionKind)
        {
            return _actionCounts.TryGetValue(actionKind, out int count) ? count : 0;
        }

        /// <summary> 行動種別ごとの発動回数。 </summary>
        private readonly Dictionary<MissionActionKind, int> _actionCounts = new();
    }
}
