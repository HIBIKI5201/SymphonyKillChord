using System.Collections.Generic;

namespace KillChord.Runtime.InfraStructure.OutGame.Scenario
{
    /// <summary>
    /// 正規化 CSV から読み取ったイベント行情報を保持する。
    /// </summary>
    internal readonly struct EventRow
    {
        /// <summary>
        /// イベント行情報を初期化する。
        /// </summary>
        public EventRow(int lineNo, int step, string type, IReadOnlyList<string> values)
        {
            LineNo = lineNo;
            Step = step;
            Type = type;
            Values = values;
        }

        /// <summary> LineNo を取得する。 </summary>
        public int LineNo { get; }
        /// <summary> Step を取得する。 </summary>
        public int Step { get; }
        /// <summary> Type を取得する。 </summary>
        public string Type { get; }
        /// <summary> Values を取得する。 </summary>
        public IReadOnlyList<string> Values { get; }
    }

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