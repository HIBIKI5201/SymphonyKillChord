using R3;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace KillChord.Runtime.View
{
    [RequireComponent(typeof(Light))]
    public class MuzzleFlashLight : MonoBehaviour
    {
        public async ValueTask Flash(CancellationToken token = default)
        {
            _light.enabled = true;
            await Awaitable.WaitForSecondsAsync(_duration, token);
            _light.enabled = false;
        }

        [SerializeField, Tooltip("フラッシュの持続時間")]
        private float _duration = 0.1f;

        private Light _light;

        private void Awake()
        {
            _light = GetComponent<Light>();
        }

        private void OnEnable()
        {
            _light.enabled = false;
        }

        private void OnDisable()
        {
            _light.enabled = false;
        }
    }
}
