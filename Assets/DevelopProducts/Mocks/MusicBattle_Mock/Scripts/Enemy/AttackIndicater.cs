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

            _decal.fadeFactor = 0;
        }

        public bool Visible
        {
            set
            {
                _visible = value;
                _decal.fadeFactor = value ? 1 : 0;
            }
            get => _visible;
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

        private bool _visible;
    }
}
