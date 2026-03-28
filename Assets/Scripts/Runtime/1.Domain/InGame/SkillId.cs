using UnityEngine;

namespace KillChord.Runtime.Domain
{
    public readonly struct SkillId
    {
        public int Id => _id;
        private readonly int _id;

        public SkillId(int id)
        {
            _id = id;
        }
    }
}