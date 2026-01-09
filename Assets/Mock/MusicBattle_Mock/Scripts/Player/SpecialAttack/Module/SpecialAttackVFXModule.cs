using Mock.MusicBattle.Player;
using System.Threading;
using UnityEngine;
using UnityEngine.VFX;

namespace Mock.MusicBattle
{
    public class SpecialAttackVFXModule : ISpecialAttackModule
    {
        public void Execute(SpecialAttackDTO dto)
        {
            if (_vfxPrefab == null) { return; } 

            Vector3 pos = dto.Player.transform.position;
            pos += _offset;

            VisualEffect vfx = GetVFX(dto.DestroyToken);

            vfx.transform.position = pos;
            vfx.Play();
        }

        [SerializeField, Tooltip("使用するVFX")]
        private VisualEffect _vfxPrefab;
        [SerializeField, Tooltip("発動者からのオフセット")]
        private Vector3 _offset;

        private VisualEffect _vfxInstance;

        private VisualEffect GetVFX(CancellationToken token)
        {
            if (_vfxInstance != null) 
            { 
                return _vfxInstance;
            }

            return CreateVFX(token);
        }

        private VisualEffect CreateVFX(CancellationToken token)
        {
            VisualEffect vfx = Object.Instantiate(_vfxPrefab);
            _vfxInstance = vfx;
            token.Register(() =>
            {
                Object.Destroy(vfx.gameObject);
                _vfxInstance = null;
            });

            return vfx;
        }
    }
}
