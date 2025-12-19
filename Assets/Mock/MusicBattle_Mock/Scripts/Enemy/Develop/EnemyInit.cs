using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Mock.MusicBattle.MusicSync;

namespace Mock.MusicBattle.Enemy
{
    /// <summary>
    ///     エネミー全体の初期化テストクラス。
    /// </summary>
    [DefaultExecutionOrder(-800)]
    public class EnemyInit : MonoBehaviour
    {
        #region インスペクター表示フィールド
        /// <summary> エネミーマネージャーの参照。 </summary>
        [SerializeField, Tooltip("エネミーマネージャーの参照。")]
        private EnemyManager _enemyManager;
        /// <summary> エネミーのステータス。 </summary>
        [SerializeField, Tooltip("エネミーのステータス。")]
        private EnemyStatus _enemystatus;
        /// <summary> プレイヤーのTransform。 </summary>
        [SerializeField, Tooltip("プレイヤーのTransform。")]
        private Transform _player;
        /// <summary> エネミーのスポーン間隔時間。 </summary>
        [SerializeField, Tooltip("エネミーのスポーン間隔時間。")]
        private float _enemySpawnTime = 1f;
        /// <summary> エネミースポーンX範囲。 </summary>
        [SerializeField, Tooltip("エネミースポーンX範囲。")]
        private float _xrange = 50f;
        /// <summary> エネミースポーンY範囲。 </summary>
        [SerializeField, Tooltip("エネミースポーンY範囲。")]
        private float _yrange = 1f;
        /// <summary> エネミースポーンZ範囲。 </summary>
        [SerializeField, Tooltip("エネミースポーンZ範囲。")]
        private float _zrange = 50f;
        #endregion

        #region プライベートフィールド
        /// <summary> エネミーコンテナ。 </summary>
        private EnemyContainer _enemyContainer;
        /// <summary> エネミーファクトリー。 </summary>
        private EnemyFactory _factory;
        /// <summary> 音楽同期マネージャー。 </summary>
        private MusicSyncManager _musicSyncManager;
        #endregion

        #region Unityライフサイクルメソッド
        /// <summary>
        ///     スクリプトインスタンスがロードされたときに呼び出されます。
        ///     エネミーコンテナの初期化を行います。
        /// </summary>
        private void Awake()
        {
            _enemyContainer = new EnemyContainer();
       
        }

        /// <summary>
        ///     最初のフレームアップデートの前に呼び出されます。
        ///     敵のスポーンループコルーチンを開始します。
        /// </summary>
        private void Start()
        {
          StartCoroutine(SpawnLoop());
        }
        #endregion

        #region Privateメソッド
        /// <summary>
        ///     敵を定期的にスポーンさせるコルーチン。
        /// </summary>
        private IEnumerator SpawnLoop()
        {
            while (true)
            {
                // ランダムな位置を生成。
                Vector3 randomPos = new Vector3(
                    Random.Range(-_xrange, _xrange),
                    _yrange,
                    Random.Range(-_zrange, _zrange));
                
                // 敵をスポーンさせる。
                _factory.Spawn(_enemystatus, randomPos);

                yield return new WaitForSeconds(_enemySpawnTime);
            }
        }
        #endregion
    }
}