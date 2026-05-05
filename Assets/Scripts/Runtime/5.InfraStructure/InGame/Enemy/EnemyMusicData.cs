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
        public byte BarFlag => _barFlag;
        public double TimeSignature => _timeSignature;
        public double TargetBeat => _targetBeat;

        [SerializeField] private byte _barFlag;
        [SerializeField] private double _timeSignature;
        [SerializeField] private double _targetBeat;
    }
}