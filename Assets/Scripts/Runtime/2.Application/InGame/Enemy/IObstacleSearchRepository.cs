using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Enemy
{
    /// <summary>
    ///     周囲の障害物を検索するRepositoryインターフェース。
    /// </summary>
    public interface IObstacleSearchRepository
    {
        /// <summary>
        ///     指定位置周辺で最も近い障害物の位置を検索する。
        /// </summary>
        /// <param name="sourcePosition"></param>
        /// <param name="obstaclePosition"></param>
        /// <returns> 障害物が見つかった場合はtrue。 </returns>
        public bool TryFindNearestObstaclePosition(Vector3 sourcePosition, out Vector3 obstaclePosition);
    }
}
