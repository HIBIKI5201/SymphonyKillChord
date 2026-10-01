using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Enemy
{
    /// <summary>
    ///     周囲の障害物を検索する。
    /// </summary>
    public class ObstacleSearchService
    {
        /// <summary>
        ///     障害物探索のリポジトリを指定して生成する。
        /// </summary>
        public ObstacleSearchService(IObstacleSearchRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        ///     指定位置周辺で最も近い障害物の位置を検索する。
        /// </summary>
        /// <param name="sourcePosition"></param>
        /// <param name="obstaclePosition"></param>
        /// <returns> 障害物が見つかった場合はtrue。 </returns>
        public bool TryFindNearestObstaclePosition(Vector3 sourcePosition, out Vector3 obstaclePosition)
        {
            return _repository.TryFindNearestObstaclePosition(sourcePosition, out obstaclePosition);
        }

        private IObstacleSearchRepository _repository;
    }
}
