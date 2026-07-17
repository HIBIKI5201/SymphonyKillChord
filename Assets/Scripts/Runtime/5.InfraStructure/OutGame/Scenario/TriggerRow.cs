using System.Collections.Generic;

namespace KillChord.Runtime.InfraStructure.OutGame.Scenario
{
    /// <summary>
    /// 正規化 CSV から読み取ったトリガー行情報を保持する。
    /// </summary>
    internal readonly struct TriggerRow
    {
        /// <summary>
        /// トリガー行情報を初期化する。
        /// </summary>
        public TriggerRow(int lineNo, int parentStep, IReadOnlyList<string> values)
        {
            LineNo = lineNo;
            ParentStep = parentStep;
            Values = values;
        }

        /// <summary> LineNo を取得する。 </summary>
        public int LineNo { get; }

        /// <summary> ParentStep を取得する。 </summary>
        public int ParentStep { get; }

        /// <summary> Values を取得する。 </summary>
        public IReadOnlyList<string> Values { get; }
    }
}
