using System.Collections.Generic;

namespace KillChord.Runtime.InfraStructure.OutGame.Scenario
{
    /// <summary>
    /// オーサリング CSV から読み取ったトリガー行情報を保持する。
    /// </summary>
    internal readonly struct AuthoringTriggerRow
    {
        /// <summary>
        /// オーサリング用トリガー行情報を初期化する。
        /// </summary>
        public AuthoringTriggerRow(int lineNo, IReadOnlyList<string> fields)
        {
            LineNo = lineNo;
            Fields = fields;
        }

        /// <summary> LineNo を取得する。 </summary>
        public int LineNo { get; }

        /// <summary> Fields を取得する。 </summary>
        public IReadOnlyList<string> Fields { get; }
    }
}
