using CriWare;
using Mock.MusicBattle.Battle;
using Mock.MusicBattle.Camera;
using Mock.MusicBattle.Enemy;
using Mock.MusicBattle.MusicSync;
using Mock.MusicBattle.Player;
using Mock.MusicBattle.UI;
using Unity.Cinemachine;
using UnityEngine;

namespace Mock.MusicBattle.Basis
{
    /// <summary>
    ///     ゲーム全体の管理クラス。
    /// </summary>
    [DefaultExecutionOrder(-900)]
    public class GameManager : MonoBehaviour
    {
        #region インスペクター表示フィールド
        /// <summary> プレイヤーマネージャーの参照。 </summary>
        [SerializeField, Tooltip("プレイヤーマネージャーの参照。")]
        private PlayerManager _playerManager;
        /// <summary> Cinemachineカメラの参照。 </summary>
        [SerializeField, Tooltip("Cinemachineカメラの参照。")]
        private CinemachineCamera _camera;
        /// <summary> 入力バッファの参照。 </summary>
        [SerializeField, Tooltip("入力バッファの参照。")]
        private InputBuffer _inputBuffer;
        /// <summary> カメラマネージャーの参照。 </summary>
        [SerializeField, Tooltip("カメラマネージャーの参照。")]
        private CameraManager _cameraManager;
        /// <summary> 敵マネージャーの参照。 </summary>
        [SerializeField, Tooltip("敵マネージャーの参照。")]
        private EnemyManager _enemyManager;
        /// <summary> 敵のステータス。 </summary>
        [SerializeField, Tooltip("敵のステータス。")]
        private EnemyStatus _enemystatus;
        /// <summary> プレイヤーのTransform。 </summary>
        [SerializeField, Tooltip("プレイヤーのTransform。")]
        private Transform _player;
        /// <summary> 敵のスポーン間隔時間。 </summary>
        [SerializeField, Tooltip("敵のスポーン間隔時間。")]
        private float _enemySpawnTime = 1f;
        /// <summary> 音楽同期マネージャー。 </summary>
        [SerializeField, Tooltip("音楽同期マネージャー。")]
        private MusicSyncManager _musicSyncManager;
        /// <summary> 音楽システム初期化ScriptableObject。 </summary>
        [SerializeField, Tooltip("音楽システム初期化ScriptableObject。")]
        private MusicSystemInitSO _musicSystemInitSO;
        /// <summary> CRI Atom Sourceの参照。 </summary>
        [SerializeField, Tooltip("CRI Atom Sourceの参照。")]
        private CriAtomSource _source;
        /// <summary> CRI Music Bufferの参照。 </summary>
        [SerializeField, Tooltip("CRI Music Bufferの参照。")]
        private CriMusicBuffer _criMusicBuffer;
        /// <summary> 敵のスポーン情報ScriptableObject。 </summary>
        [SerializeField, Tooltip("敵のスポーン情報ScriptableObject。")]
        private EnemySpawnSO _enemySpawnSO;
        /// <summary> インゲームHUDマネージャー。 </summary>
        [SerializeField, Tooltip("インゲームHUDマネージャー。")]
        private IngameHUDManager _hudManager;
        #endregion

        #region プライベートフィールド
        /// <summary> 敵のファクトリー。 </summary>
        private EnemyFactory _factory;
        /// <summary> ロックオンマネージャー。 </summary>
        private LockOnManager _lockOnManager;
        /// <summary> 敵のコンテナ。 </summary>
        private EnemyContainer _enemyContainer;
        #endregion

        #region Unityライフサイクルメソッド
        /// <summary>
        ///     スクリプトインスタンスがロードされたときに呼び出されます。
        ///     ゲームの初期化を行います。
        /// </summary>
        private void Awake()
        {
            Init();
        }

        /// <summary>
        ///     最初のフレームアップデートの前に呼び出されます。
        ///     音楽同期の初期化と敵のスポーンループを開始します。
        /// </summary>
        private void Start()
        {
            _musicSyncManager.Init(_source, _musicSystemInitSO.Bpm, _musicSystemInitSO.TimeSignature, _musicSystemInitSO.StartOffset);
            StartCoroutine(EnemyUtility.SpawnLoop(
                _enemyContainer,
                _enemySpawnSO,
                _factory,
                _enemystatus,
                _enemySpawnTime));
        }
        #endregion

        #region Privateメソッド
        /// <summary>
        ///     ゲームの主要なコンポーネントを初期化します。
        /// </summary>
        private void Init()
        {
            _enemyContainer = new EnemyContainer();
            _lockOnManager = new LockOnManager(_cameraManager.transform,
              _enemyContainer, _inputBuffer);

            PlayerInitUtility.InitPlayer(_playerManager, _inputBuffer,
                _cameraManager, _camera, _lockOnManager, _musicSyncManager);

            HudUtility.Init(_hudManager, _playerManager,
                _criMusicBuffer, _inputBuffer,
                _lockOnManager,
                this.destroyCancellationToken);

            _factory = new EnemyFactory(
                _enemyContainer, _player,
                _enemyManager, _musicSyncManager,
                _lockOnManager, _hudManager);
            EnemyUtility.EnemyContainerInit(_enemyContainer, _playerManager, _cameraManager, _lockOnManager);
        }
        #endregion
    }
}