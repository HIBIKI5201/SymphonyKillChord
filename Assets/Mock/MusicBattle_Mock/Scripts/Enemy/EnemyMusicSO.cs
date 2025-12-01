using UnityEngine;

namespace Mock.MusicBattle
{
    [CreateAssetMenu(fileName = "EnemyMusicSO", menuName = "Mock/MusicBattle/Enemy/EnemyMusicSO")]
    public class EnemyMusicSO : ScriptableObject
    {
        [Header("小節タイミング")]
        public long BarFlg = 2;
        [Header("拍子スケール")]
        public long TimeSignature = 4;
        [Header("拍数")]
        public long TargetBeat = 2;
    }
}
