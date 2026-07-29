using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     敵の生成位置を探索するクラス。
    /// </summary>
    public class EnemySpawnPositionSearcher : MonoBehaviour
    {
        /// <summary>
        ///     初期化処理。
        /// </summary>
        /// <param name="playerTransform"> プレイヤーのTransformです。 </param>
        public void Initialize(Transform playerTransform)
        {
            _positionPairs = GameObject.FindObjectsByType<SpawnPositionPair>(FindObjectsSortMode.None);
            if (_positionPairs == null || _positionPairs.Length < 1)
            {
                throw new Exception("[EnemySpawnPositionSearcher] 敵生成の位置情報が見つかりませんでした。");
            }
            _playerTransform = playerTransform;
        }

        /// <summary>
        ///     敵の生成位置をランダムで１つ選定する。
        /// </summary>
        /// <param name="candidateSpawnPointHashes"> 候補とするスポーンポイントIDです。null/空の場合は全スポーンポイントが対象です。 </param>
        /// <returns> 使用する生成位置です。 </returns>
        public async ValueTask<SpawnPositionPair> GetRandomSpawnPositionAsync(
            IReadOnlyList<int> candidateSpawnPointHashes = null)
        {
            SpawnPositionPair[] candidates = ResolveCandidates(candidateSpawnPointHashes);

            int loopCnt = 0;
            while (loopCnt < _maxSearchLoop)
            {
                ShuffulePositionPairs(candidates);
                for (int i = 0; i < candidates.Length; i++)
                {
                    if ((!candidates[i].IsInUse) && !IsPositionNearPlayer(candidates[i]))
                    {
                        return candidates[i];
                    }
                }
                // 使える生成位置がない場合、一定時間待って再探索する
                await Task.Delay(_searchDelay);
                loopCnt++;
            }
            // 一定回数探索しても生成位置が見つからない場合、明示された既定位置を使う
            Debug.LogWarning("[EnemySpawnPositionSearcher] 予期せぬ原因により、敵生成位置取得が失敗しました。初期位置で敵を生成します。");
            if (_defaultPositionPair != null)
            {
                return _defaultPositionPair;
            }
            throw new Exception("[EnemySpawnPositionSearcher] 使用可能な敵生成位置が見つかりませんでした。");
        }

        /// <summary>
        ///     候補スポーンポイントIDから探索対象の配列を解決する。
        /// </summary>
        /// <param name="candidateSpawnPointHashes"> 候補とするスポーンポイントIDです。 </param>
        /// <returns> 探索対象の生成位置配列（呼び出しごとの独立コピー）です。 </returns>
        private SpawnPositionPair[] ResolveCandidates(IReadOnlyList<int> candidateSpawnPointHashes)
        {
            if (candidateSpawnPointHashes == null || candidateSpawnPointHashes.Count == 0)
            {
                return (SpawnPositionPair[])_positionPairs.Clone();
            }

            List<SpawnPositionPair> filtered = new(candidateSpawnPointHashes.Count);
            for (int i = 0; i < _positionPairs.Length; i++)
            {
                for (int j = 0; j < candidateSpawnPointHashes.Count; j++)
                {
                    if (_positionPairs[i].SpawnPointId.Id == candidateSpawnPointHashes[j])
                    {
                        filtered.Add(_positionPairs[i]);
                        break;
                    }
                }
            }

            if (filtered.Count == 0)
            {
                Debug.LogWarning(
                    "[EnemySpawnPositionSearcher] 指定された候補スポーンポイントが見つかりませんでした。全スポーンポイントから生成します。");
                return (SpawnPositionPair[])_positionPairs.Clone();
            }

            return filtered.ToArray();
        }
        
        [Header("性能調整")]
        [SerializeField, Tooltip("生成位置が見つからない場合、再探索までの待ち時間(ミリ秒)")]
        private int _searchDelay = 1000;
        [SerializeField, Tooltip("生成位置が見つからない場合、再探索を行う最大回数")]
        private int _maxSearchLoop = 10;

        [Space]
        [SerializeField, Tooltip("生成位置が見つからない場合の位置")]
        private SpawnPositionPair _defaultPositionPair;
        [SerializeField, Tooltip("生成可能なプレイヤーとの最小距離")]
        private float _minDistanceToPlayer = 10f;

        private Transform _playerTransform;
        private SpawnPositionPair[] _positionPairs;

        /// <summary>
        ///     生成位置の配列をシャッフルする。
        /// </summary>
        /// <param name="positionPairs"> シャッフル対象の配列です。 </param>
        private void ShuffulePositionPairs(SpawnPositionPair[] positionPairs)
        {
            for (int i = positionPairs.Length - 1; i > 0; i--)
            {
                int index = UnityEngine.Random.Range(0, i + 1);
                (positionPairs[i], positionPairs[index]) = (positionPairs[index], positionPairs[i]);
            }
        }

        /// <summary>
        ///     生成位置がプレイヤーに近すぎるか判定する。
        /// </summary>
        /// <param name="positionPair"> 判定対象の生成位置です。 </param>
        /// <returns> 近すぎる場合はtrue。 </returns>
        private bool IsPositionNearPlayer(SpawnPositionPair positionPair)
        {
            if (_playerTransform == null)
            {
                return false;
            }

            float distance = Vector3.Distance(_playerTransform.position, positionPair.transform.position);
            return distance < _minDistanceToPlayer;
        }
    }
}
