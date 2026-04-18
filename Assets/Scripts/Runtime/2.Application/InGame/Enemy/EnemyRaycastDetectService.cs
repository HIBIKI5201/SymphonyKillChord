using UnityEngine;

namespace KillChord.Runtime.Application
{
    /// <summary>
    ///     敵から射線を通し、目標に直撃できるか判定する。
    /// </summary>
    public class EnemyRaycastDetectService
    {

        public EnemyRaycastDetectService(IEnemyRaycastDetectRepository raycastDetectRepo)
        {
            _raycastDetectRepo = raycastDetectRepo;
        }

        public bool CanRaycastHitTarget => _raycastDetectRepo.CanRaycastHitTarget;
       
        private IEnemyRaycastDetectRepository _raycastDetectRepo;
    }
}
