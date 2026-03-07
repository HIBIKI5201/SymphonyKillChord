using System;
using System.Collections.Generic;
using System.Text;

namespace SinfoniaStudio.SinfoniaOperator
{
    internal readonly struct EnvironmentVariable
    {
        public readonly string Key { get; }
        public readonly string Value { get; }
        public EnvironmentVariable(string key)
        {
            Key = key;
            Value = Environment.GetEnvironmentVariable(key) ?? string.Empty;
        }
    }
}
