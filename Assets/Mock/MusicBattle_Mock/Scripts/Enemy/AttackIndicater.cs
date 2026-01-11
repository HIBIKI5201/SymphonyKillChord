using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Mock.MusicBattle.Enemy
{
    public class AttackIndicater
    {
        public AttackIndicater(DecalProjector decal)
        {
            _decal = decal;
        }

        private readonly DecalProjector _decal;
    }
}
