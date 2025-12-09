using UnityEngine;

namespace Mock.MusicBattle.Basis
{
    public  class ParticleController : MonoBehaviour
    {

        public static ParticleController Instance { get; private set; }
        [SerializeField] private  ParticleSystem _hitEffect;

        private void Awake()
        {
           if(Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(Instance);
            }
        }
        public  void PlayParticle(Vector3 position)
        {
            _hitEffect.transform.position = position;
            _hitEffect.Play();
        }
    }
}
