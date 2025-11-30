using Mock.MusicBattle.Enemy;
using Mock.MusicBattle.MusicSync;
using System.Threading;
using UnityEngine;

namespace Mock.MusicBattle
{
    public class EnemyAttack
    {
        public EnemyAttack(EnemyManager enemy, MusicSyncManager music,
            EnemyMusicSO encount, EnemyMusicSO battle)
        {
            if (music == null) Debug.LogError("music is NULL!");
            if (encount == null) Debug.LogError("encount is NULL!");
            if (battle == null) Debug.LogError("battle is NULL!");

            _enemyManager = enemy;
            _musicSyncManager = music;
            _encount = encount;
            _battale = battle;
            _enemyManager.OnAttack += OnAttackHandler;
            _enemyManager.OnOutOfRange += CancelScheduled;
        }

        public void OnAttackHandler()
        {
            ScheduledEncount();
        }


        private void ScheduledBattale()
        {
            BarTimingInfo barTimingInfo = new BarTimingInfo(_battale.BarFlg, _battale.TimeSignature, _battale.TargetBeat);
            _cancellationTokenSource = new CancellationTokenSource();
            _musicSyncManager.RegisterAction(barTimingInfo, Attack, _cancellationTokenSource.Token);
        }
        private void ScheduledEncount()
        {
            BarTimingInfo barTimingInfo = new BarTimingInfo(_encount.BarFlg, _encount.TimeSignature, _encount.TargetBeat);
            _cancellationTokenSource = new CancellationTokenSource();
            _musicSyncManager.RegisterAction(barTimingInfo, Attack, _cancellationTokenSource.Token);
        }
        private void CancelScheduled()
        {
            _cancellationTokenSource.Cancel();
        }

        private EnemyMusicSO _encount;
        private EnemyMusicSO _battale;
        private CancellationTokenSource _cancellationTokenSource;
        private EnemyManager _enemyManager;
        private MusicSyncManager _musicSyncManager;

        private void Attack()
        {
            Debug.Log("Attack!");
            ScheduledBattale();
        }


    }
}