using KillChord.Runtime.View.Persistent.Music;
using KillChord.Runtime.View.Persistent.Voice;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition.Persistent.Music
{
    /// <summary>
    ///     SoundEffectVolumeManager関連の初期化をする。
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class SoundEffectInitializer : MonoBehaviour
    {
        [SerializeField]
        private bool _isDebug = true;
        private bool _initialized = false;

        private void Awake()
        {
            if (_initialized) return;
            _initialized = true;
            ServiceLocator.RegisterInstance(new SoundEffectVolumeManager());
            ServiceLocator.RegisterInstance(new VoiceVolumeManager());
        }

        private void Update()
        {
            if (_isDebug)
            {
                Debug.Log(ServiceLocator.GetInstance<VoiceVolumeManager>().GetVolume());
                Debug.Log(ServiceLocator.GetInstance<SoundEffectVolumeManager>().GetVolume());
            }

        }
    }
}
