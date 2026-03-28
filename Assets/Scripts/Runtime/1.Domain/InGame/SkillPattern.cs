using System;

namespace KillChord.Runtime.Domain
{
    public readonly struct SkillPattern
    {
        public ReadOnlySpan<int> Signatures => _signatures;
        private readonly int[] _signatures;

        public SkillPattern(int[] signatures)
        {
            _signatures = signatures;
        }
    }
}