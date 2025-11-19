using System;
using System.Text;
using UnityEngine;

namespace Mock.CharacterControl
{
    [RequireComponent(typeof(ParticleSystem))]
    public class Explode : MonoBehaviour
    {
        [SerializeField]
        private float _loopDuration = 1.0f;
        [SerializeField]
        private float _explodeRadius = 1.0f;
        [SerializeField]
        private float _explodeForce = 1.0f;

        private ParticleSystem _particle;
        private void Awake()
        {
            _particle = GetComponent<ParticleSystem>();
        }

        private void Start()
        {
            LoopParticlePlay();
        }

        private async void LoopParticlePlay()
        {
            if (_particle == null) { return; }

            while (!destroyCancellationToken.IsCancellationRequested)
            {
                _particle.Play();

                Collider[] hits = Physics.OverlapSphere(
                    transform.position,
                    _explodeRadius);

                StringBuilder sb = new("Explodeヒット");
                foreach (Collider target in hits)
                {
                    Rigidbody rb = target.attachedRigidbody;

                    if (rb == null) { continue; }

                    rb.AddExplosionForce(
                        _explodeForce,
                        transform.position,
                        _explodeRadius);

                    sb.AppendLine(rb.name);
                }

                Debug.Log(sb.ToString());

                try
                {
                    await Awaitable.WaitForSecondsAsync(_loopDuration, destroyCancellationToken);
                }
                catch (OperationCanceledException) { break; }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _explodeRadius);
        }
    }
}
