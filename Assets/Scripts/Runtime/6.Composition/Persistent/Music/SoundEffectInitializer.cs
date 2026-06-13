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
        private static bool _initialized;

        private void Awake()
        {
            if (_initialized) return;
            _initialized = true;

            ServiceLocator.RegisterInstance(new SoundEffectVolumeManager());
            ServiceLocator.RegisterInstance(new VoiceVolumeManager());
        }
    }
}
