using KillChord.Runtime.View.Persistent.Music;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.Music
{
    /// <summary>
    ///     SoundEffectVolumeManager関連の初期化をする。
    /// </summary>
    public class SoundEffectInitializer : MonoBehaviour
    {
        private void Awake()
        {
            SoundEffectVolumeManager manager = new();
            ServiceLocator.RegisterInstance(manager);
        }
    }
}
