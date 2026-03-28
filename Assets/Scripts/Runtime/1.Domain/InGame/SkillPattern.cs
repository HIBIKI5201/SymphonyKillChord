using UnityEngine;

namespace KillChord.Runtime.Domain
{
    public readonly struct SkillPattern
    {
        public int[] Signatures => _signatures;
        private readonly int[] _signatures;

        public SkillPattern(int[] signatures)
        {
            _signatures = signatures;
        }
    }
}