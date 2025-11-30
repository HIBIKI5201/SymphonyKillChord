using Mock.MusicBattle.MusicSync;
using Mock.MusicBattle.UI;
using UnityEngine;

namespace Mock.MusicBattle.Develop
{
    public class MusicBufferForUIDebugger : MonoBehaviour, IMusicBuffer
    {
        public double CurrentBpm => 120;

        public double BeatLength => 60 / CurrentBpm;

        public double CurrentBeat => Time.time / BeatLength;

        private void Awake()
        {
            IngameHUDManager hud = FindAnyObjectByType<IngameHUDManager>();
            hud.Initialize(this);
        }
    }
}
