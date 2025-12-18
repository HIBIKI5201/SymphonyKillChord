using UnityEngine;

namespace Mock.MusicBattle.MusicSync
{
    [CreateAssetMenu(fileName = "MusicSystemInitSO", menuName = "Mock/MusicBattle/MusicSystemInitSO")]
    public class MusicSystemInitSO : ScriptableObject
    {
        [Header("音楽同期システム初期化パラメータ")]
        public double Bgm;
        public double BgmProperTime;
        public long StartOffset;
    }
}
