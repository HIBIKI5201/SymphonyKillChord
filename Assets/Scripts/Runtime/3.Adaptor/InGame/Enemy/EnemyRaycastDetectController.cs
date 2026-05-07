using KillChord.Runtime.Application;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Enemy
{
    /// <summary>
    ///     敵から射線を通し、目標に直撃できるか判定する。
    /// </summary>
    public class EnemyRaycastDetectController : IEnemyRaycastDetectRepository
    {
        public EnemyRaycastDetectController(IEnemyRaycastDetectViewModel model)
        {
            _model = model;
        }
        /// <summary> 射線が通っているか </summary>
        public bool CanRaycastHitTarget => _model.CanRaycastHitTarget;

        private IEnemyRaycastDetectViewModel _model;
    }
}
