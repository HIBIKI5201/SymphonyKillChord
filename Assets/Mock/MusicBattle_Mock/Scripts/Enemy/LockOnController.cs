using UnityEngine;

namespace Mock.MusicBattle.Enemy
{
    /// <summary>
    /// ロックオン対象の管理を行う。
    /// </summary>
    public class LockOnController 
    {
        public bool IsLockedOn => _isLockedOn; 
        public LockOnController()
        {
        }
        
        /// <summary>　ロックオン状態を設定する。</summary>
        public void SetLockOn(bool isLockedOn)
        {
            _isLockedOn = isLockedOn;
        }
        
     
        private bool _isLockedOn;
        
    }
}
