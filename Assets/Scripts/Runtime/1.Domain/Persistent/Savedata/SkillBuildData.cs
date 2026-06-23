using System;
using System.Collections.Generic;

namespace KillChord.Runtime.Domain.Persistent.Savedata
{
    /// <summary>
    ///     プレイヤーの装備スキル構成のセーブデータを表すクラス。
    ///     <para> プレイヤーの装備スキルの ID のリストと、スキルレベルアップポイントを保持する。 </para>
    /// </summary>
    [Serializable]
    public sealed class SkillBuildData
    {
        /// <summary>
        ///     プレイヤーの装備スキルの ID のリスト。
        ///     <para> 装備は最低 2 つのスロットが初期状態であり、null が起きることはないため、
        ///      set 可能なプロパティとしている。</para>
        /// </summary>
        public List<int> EquipmentSkillIDs { get; set; } = new();

        /// <summary>
        ///     プレイヤーのスキルレベルアップポイント。
        ///     バリデーションを掛けたいので、直接 set できないようにしている。
        /// </summary>
        public int SkillLevelupPoint { get; private set; } = 0;

        /// <summary>
        ///    プレイヤーのスキルレベルアップポイントを設定する。
        /// </summary>
        /// <param name="point"> 設定するスキルレベルアップポイントの値。 </param>
        /// <exception cref="ArgumentOutOfRangeException"> point が 0 未満の場合にスローされます。</exception>
        public void SetSkillLevelupPoint(int point)
        {
            if (point < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(point), "スキルレベルアップポイントは 0 以上である必要があります。");
            }
            SkillLevelupPoint = point;
        }
    }
}
