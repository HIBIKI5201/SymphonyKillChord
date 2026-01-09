using CriWare;
using UnityEngine;

namespace Mock.MusicBattle
{
    public readonly struct SpecialAttackDTO
    {
        public SpecialAttackDTO(
            GameObject playerObj,
            CriAtomSource source)
        {
            Player = playerObj;
            Source = source;
        }

        public readonly GameObject Player;
        public readonly CriAtomSource Source;
    }
}
