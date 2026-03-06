using CriWare;
using System.Threading;
using UnityEngine;

namespace Mock.MusicBattle
{
    public readonly struct SpecialAttackDTO
    {
        public SpecialAttackDTO(
            GameObject playerObj,
            CriAtomSource source,
            CancellationToken destroyToken)
        {
            Player = playerObj;
            Source = source;
            DestroyToken = destroyToken;
        }

        public readonly GameObject Player;
        public readonly CriAtomSource Source;
        public readonly CancellationToken DestroyToken;
    }
}
