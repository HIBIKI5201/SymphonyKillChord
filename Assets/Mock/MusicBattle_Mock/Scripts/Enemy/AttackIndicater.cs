using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Mock.MusicBattle.Enemy
{
    public class AttackIndicater
    {
        public AttackIndicater(DecalProjector decal)
        {
            _decal = decal;
            _transform = _decal.transform;
        }

        public void Move(float range)
        {
            Vector3 size = _decal.size;
            size.z = range;
            _decal.size = size;

            Vector3 pos = _transform.localPosition;
            pos.z = range / 2;
            _transform.localPosition = pos;
        }

        private readonly DecalProjector _decal;
        private readonly Transform _transform;
    }
}
