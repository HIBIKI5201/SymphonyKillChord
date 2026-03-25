using UnityEngine;

namespace KillChord.Runtime.Adaptor
{
    public readonly ref struct IngameMusicDTO
    {
        public IngameMusicDTO(string cueName)
        {
            _cueName = cueName;
        }

        public readonly string _cueName;
    }
}