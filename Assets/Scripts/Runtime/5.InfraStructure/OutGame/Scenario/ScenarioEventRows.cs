using System.Collections.Generic;

namespace KillChord.Runtime.InfraStructure
{
    internal readonly struct EventRow
    {
        public EventRow(int lineNo, int step, string type, IReadOnlyList<string> values)
        {
            LineNo = lineNo;
            Step = step;
            Type = type;
            Values = values;
        }

        public int LineNo { get; }
        public int Step { get; }
        public string Type { get; }
        public IReadOnlyList<string> Values { get; }
    }

    internal readonly struct TriggerRow
    {
        public TriggerRow(int lineNo, int parentStep, IReadOnlyList<string> values)
        {
            LineNo = lineNo;
            ParentStep = parentStep;
            Values = values;
        }

        public int LineNo { get; }
        public int ParentStep { get; }
        public IReadOnlyList<string> Values { get; }
    }

    internal readonly struct AuthoringTriggerRow
    {
        public AuthoringTriggerRow(int lineNo, IReadOnlyList<string> fields)
        {
            LineNo = lineNo;
            Fields = fields;
        }

        public int LineNo { get; }
        public IReadOnlyList<string> Fields { get; }
    }
}
