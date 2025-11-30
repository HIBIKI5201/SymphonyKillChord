using Mock.MusicBattle.Enemy;
using Mock.MusicBattle.MusicSync;
using System.Threading;
using UnityEngine;

namespace Mock.MusicBattle
{
    /// <summary>
    ///     敵の攻撃を音楽同期で管理するクラス。
    ///     遭遇フェーズとバトルフェーズのタイミングで攻撃予約を行う。
    /// </summary>
    public class EnemyAttack
    {
        /// <summary>
        ///     コンストラクタ。必要なマネージャーや設定データを受け取り、イベント登録を行う。
        /// </summary>
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

            enemy.OnAttack -= OnAttackHandler;
            enemy.OnOutOfRange -= CancelScheduled;
            _enemyManager.OnAttack += OnAttackHandler;
            _enemyManager.OnOutOfRange += CancelScheduled;
        }

        /// <summary>
        ///     敵が攻撃可能になった際に呼ばれ、遭遇フェーズの攻撃スケジュールを開始する。
        /// </summary>
        public void OnAttackHandler()
        {
            ScheduledEncount();
        }

        /// <summary>
        ///     バトルフェーズの攻撃タイミングを音楽同期アクションとして予約する。
        /// </summary>
        private void ScheduledBattale()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
            }
            BarTimingInfo barTimingInfo = new BarTimingInfo(_battale.BarFlg, _battale.TimeSignature, _battale.TargetBeat);
            _cancellationTokenSource = new CancellationTokenSource();
            _musicSyncManager.RegisterAction(barTimingInfo, () => Attack(_cancellationTokenSource.Token));
        }

        /// <summary>
        ///     遭遇フェーズの攻撃タイミングを音楽同期アクションとして予約する。
        /// </summary>
        private void ScheduledEncount()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
            }
            BarTimingInfo barTimingInfo = new BarTimingInfo(_encount.BarFlg, _encount.TimeSignature, _encount.TargetBeat);
            _cancellationTokenSource = new CancellationTokenSource();
            _musicSyncManager.RegisterAction(barTimingInfo, () => Attack(_cancellationTokenSource.Token));
        }

        /// <summary>
        ///     予約中の攻撃アクションをキャンセルする。
        /// </summary>
        private void CancelScheduled()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
            }
        }

        private EnemyMusicSO _encount;
        private EnemyMusicSO _battale;
        private CancellationTokenSource _cancellationTokenSource;
        private EnemyManager _enemyManager;
        private MusicSyncManager _musicSyncManager;

        /// <summary>
        ///     攻撃実行処理。実行後、次のバトルフェーズ攻撃を予約する。
        /// </summary>
        private void Attack(CancellationToken token)
        {
            Debug.Log("Attack!");
            ScheduledBattale();
        }


    }
}