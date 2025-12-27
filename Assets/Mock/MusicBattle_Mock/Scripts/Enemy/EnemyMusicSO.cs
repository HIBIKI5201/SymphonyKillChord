using UnityEngine;

namespace Mock.MusicBattle.Enemy
{
    /// <summary>
    ///     敵の音楽的挙動を定義するScriptableObject。
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyMusicSO", menuName = "Mock/MusicBattle/Enemy/EnemyMusicSO")]
    public class EnemyMusicSO : ScriptableObject
    {
        /// <summary> 小節タイミングのフラグ。 </summary>
        [Tooltip("小節タイミングのフラグ。")]
        public long BarFlg = 2;
        /// <summary> 拍子スケール。 </summary>
        [Tooltip("拍子スケール。")]
        public long TimeSignature = 4;
        /// <summary> ターゲットとなる拍数。 </summary>
        [Tooltip("ターゲットとなる拍数。")]
        public long TargetBeat = 2;
    }
}

