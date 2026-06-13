using KillChord.Runtime.View.Persistent.Music;
using KillChord.Runtime.View.Persistent.Voice;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition.Persistent.Music
{
    /// <summary>
    ///     SoundEffectVolumeManager関連の初期化をする。
    /// </summary>
    public class SoundEffectInitializer : MonoBehaviour
    {
        private void Awake()
        {
            ServiceLocator.RegisterInstance(new SoundEffectVolumeManager());
            ServiceLocator.RegisterInstance(new VoiceVolumeManager());
        }
    }
}
