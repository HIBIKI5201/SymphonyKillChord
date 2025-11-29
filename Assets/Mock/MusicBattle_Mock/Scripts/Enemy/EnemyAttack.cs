using Mock.MusicBattle.Enemy;
using UnityEngine;
using Mock.MusicBattle.MusicSync;

namespace Mock.MusicBattle
{
    public class EnemyAttack
    {
        public EnemyAttack(EnemyManager enemy, MusicSyncManager music)
        {
            _enemyManager = enemy;
            _musicSyncManager = music;
        }
        
        private void ScheduledAttack()
        {
           
            if (_barTimingInfo.BarFlg % 2 == 1) return;

            if (_barTimingInfo.TimeSignature % 2 == 1) return;

            if (_barTimingInfo.TargetBeat % 3 == 0)
            {
                Attack();
            }
        }

        private void Attack()
        {
            
        }

        private bool _inrange = false;
        private bool _attackTiming = false;
        private EnemyManager _enemyManager;
        private MusicSyncManager _musicSyncManager;
        private BarTimingInfo _barTimingInfo;
    }
}