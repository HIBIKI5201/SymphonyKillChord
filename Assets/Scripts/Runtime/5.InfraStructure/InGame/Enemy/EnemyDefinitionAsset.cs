using KillChord.Runtime.Domain.InGame.Enemy;
using KillChord.Runtime.InfraStructure.InGame.Character;
using KillChord.Runtime.InfraStructure.InGame.Mission;
using KillChord.Runtime.Utility.Identity;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Enemy
{
    /// <summary>
    ///     雑魚敵1種類分のゲームデータと使用プレハブを定義します。
    /// </summary>
    [CreateAssetMenu(
        fileName = nameof(EnemyDefinitionAsset),
        menuName = "KillChord/Enemy/" + nameof(EnemyDefinitionAsset))]
    public sealed class EnemyDefinitionAsset : ScriptableObject
    {
        /// <summary> 敵定義IDです。 </summary>
        public EnemyDefinitionId Id => new EnemyDefinitionId(_id.Id);

        /// <summary> 敵の処理種別です。 </summary>
        public EnemyType EnemyType => _enemyType;

        /// <summary> 使用する敵プレハブです。 </summary>
        public GameObject ViewPrefab => _viewPrefab;

        /// <summary> キャラクターステータス定義です。 </summary>
        public CharacterDefinitionAsset CharacterDefinition => _characterDefinition;

        /// <summary> 移動仕様です。 </summary>
        public EnemyMoveSpecAsset MoveSpec => _moveSpec;

        /// <summary> 初回攻撃用の音楽仕様です。 </summary>
        public EnemyMusicSpecAsset EncounterMusicSpec => _encounterMusicSpec;

        /// <summary> 通常攻撃用の音楽仕様です。 </summary>
        public EnemyMusicSpecAsset BattleMusicSpec => _battleMusicSpec;

        /// <summary> 撃破時に通知するミッションキーです。 </summary>
        public EnemyMissionKeyAsset MissionKey => _missionKey;

        /// <summary> 使用する攻撃定義Indexです。 </summary>
        public int AttackIndex => _attackIndex;

        /// <summary> ObjectPoolの初期容量です。 </summary>
        public int DefaultPoolSize => _defaultPoolSize;

        /// <summary> ObjectPoolの最大容量です。 </summary>
        public int MaxPoolSize => _maxPoolSize;

        [SerializeField, SourceDataCollection("EnemyData"), Tooltip("個別の敵定義を一意に識別するIDです。")]
        private DataID _id;

        [SerializeField, Tooltip("使用する敵の処理種別です。")]
        private EnemyType _enemyType;

        [SerializeField, Tooltip("この敵定義が使用する敵プレハブです。")]
        private GameObject _viewPrefab;

        [SerializeField, Tooltip("HPや攻撃力などのキャラクターステータス定義です。")]
        private CharacterDefinitionAsset _characterDefinition;

        [SerializeField, Tooltip("移動速度や攻撃距離などの移動仕様です。")]
        private EnemyMoveSpecAsset _moveSpec;

        [SerializeField, Tooltip("初回攻撃に使用する音楽仕様です。")]
        private EnemyMusicSpecAsset _encounterMusicSpec;

        [SerializeField, Tooltip("通常攻撃に使用する音楽仕様です。")]
        private EnemyMusicSpecAsset _battleMusicSpec;

        [SerializeField, Tooltip("この敵を撃破した際にMissionへ通知するキーです。")]
        private EnemyMissionKeyAsset _missionKey;

        [SerializeField, Min(0), Tooltip("CharacterDefinition内で使用する攻撃定義のIndexです。")]
        private int _attackIndex;

        [SerializeField, Min(1), Tooltip("この敵定義専用ObjectPoolの初期容量です。")]
        private int _defaultPoolSize = 4;

        [SerializeField, Min(1), Tooltip("この敵定義専用ObjectPoolの最大容量です。")]
        private int _maxPoolSize = 20;
    }
}
