using KillChord.Runtime.Utility;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Enemy
{
    /// <summary>
    ///     敵の音楽同期に関するデータを保持するScriptableObject。
    /// </summary>
    [CreateAssetMenu(fileName = nameof(EnemyMusicData),
    menuName = PathConst.CREATE_ASSET_MENU_PATH + "Enemy/" + nameof(EnemyMusicData))]
    public class EnemyMusicData : ScriptableObject
    {
        /// <summary> 小節フラグ。0は現在小節、1は次の小節 </summary>
        public byte BarFlag => _barFlag;
        /// <summary> 小節の拍子 </summary>
        public double TimeSignature => _timeSignature;
        /// <summary> 拍目 </summary>
        public double TargetBeat => _targetBeat;

        [SerializeField, Tooltip("小節フラグ")] private byte _barFlag;
        [SerializeField, Tooltip("小節の拍子")] private double _timeSignature;
        [SerializeField, Tooltip("拍目")] private double _targetBeat;
    }
}