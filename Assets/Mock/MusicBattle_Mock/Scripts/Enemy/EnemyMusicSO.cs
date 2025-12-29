using UnityEngine;

namespace Mock.MusicBattle.Enemy
{
    /// <summary>
    ///     敵の音楽的挙動を定義するScriptableObject。
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyMusicSO", menuName = "Mock/MusicBattle/Enemy/EnemyMusicSO")]
    public class EnemyMusicSO : ScriptableObject
    {
        #region パブリックプロパティ
        /// <summary> 小節タイミングのフラグ。 </summary>
        public long BarFlg => _barFlg;
        /// <summary> 拍子スケール。 </summary>
        public long TimeSignature => _timeSignature;
        /// <summary> ターゲットとなる拍数。 </summary>
        public long TargetBeat => _targetBeat;
        #endregion

        #region インスペクター表示フィールド
        /// <summary> 小節タイミングのフラグ。 </summary>
        [Tooltip("小節タイミングのフラグ。")]
        [SerializeField]
        private long _barFlg = 2;
        /// <summary> 拍子スケール。 </summary>
        [Tooltip("拍子スケール。")]
        [SerializeField]
        private long _timeSignature = 4;
        /// <summary> ターゲットとなる拍数。 </summary>
        [Tooltip("ターゲットとなる拍数。")]
        [SerializeField]
        private long _targetBeat = 2;
        #endregion
    }}

