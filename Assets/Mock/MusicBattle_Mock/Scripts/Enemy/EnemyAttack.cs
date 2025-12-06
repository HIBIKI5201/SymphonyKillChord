using Mock.MusicBattle.Character;
using Mock.MusicBattle.Enemy;
using Mock.MusicBattle.MusicSync;
using Mock.MusicBattle.Player;
using System.Threading;
using UnityEngine;

namespace Mock.MusicBattle.Enemy
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
            EnemyMusicSO encount, EnemyMusicSO battle,ICharacter player,
            EnemyStatus enemyStatus)
            
        {
            if (music == null) Debug.LogError("music is NULL!");
            if (encount == null) Debug.LogError("encount is NULL!");
            if (battle == null) Debug.LogError("battle is NULL!");

            _enemyManager = enemy;
            _musicSyncManager = music;
            _encount = encount;
            _battale = battle;
            _player = player;
            _enemyStatus = enemyStatus;

            _enemyManager.OnAttack += OnAttackHandler;
            _enemyManager.OnOutOfRange += CancelScheduled;
        }

        public void Dispose()
        {
            _enemyManager.OnAttack -= OnAttackHandler;
            _enemyManager.OnOutOfRange -= CancelScheduled;
        }

        /// <summary>
        ///     敵が攻撃可能になった際に呼ばれ、遭遇フェーズの攻撃スケジュールを開始する。
        /// </summary>
        public void OnAttackHandler()
        {
            Debug.Log("攻撃予約可能");
            _player.TakeDamage(_enemyStatus.AttackPower);
            ScheduledEncount();
        }

        /// <summary>
        ///     バトルフェーズの攻撃タイミングを音楽同期アクションとして予約する。
        /// </summary>
        private void ScheduledBattale()
        {
            Debug.Log("バトルフェーズ攻撃予約");
            CancelScheduled();
            BarTimingInfo barTimingInfo = new BarTimingInfo(_battale.BarFlg, _battale.TimeSignature, _battale.TargetBeat);
            _cancellationTokenSource = new CancellationTokenSource();
            _musicSyncManager.RegisterAction(barTimingInfo, () =>
            {
                _isBattlePhase = false;
                Attack(_cancellationTokenSource.Token);
            });
        }

        /// <summary>
        ///     遭遇フェーズの攻撃タイミングを音楽同期アクションとして予約する。
        /// </summary>
        private void ScheduledEncount()
        {
            Debug.Log("エンカウント攻撃予約");
            CancelScheduled();
            BarTimingInfo barTimingInfo = new BarTimingInfo(_encount.BarFlg, _encount.TimeSignature, _encount.TargetBeat);
            _cancellationTokenSource = new CancellationTokenSource();
            _musicSyncManager.RegisterAction(barTimingInfo, () => Attack(_cancellationTokenSource.Token));
        }

        /// <summary>
        ///     予約中の攻撃アクションをキャンセルする。
        /// </summary>
        private void CancelScheduled()
        {
           _cancellationTokenSource?.Cancel();
        }

        private EnemyMusicSO _encount;
        private EnemyMusicSO _battale;
        private CancellationTokenSource _cancellationTokenSource;
        private EnemyManager _enemyManager;
        private MusicSyncManager _musicSyncManager;
        private ICharacter _player;
        private EnemyStatus _enemyStatus;
        private bool _isBattlePhase = false;

        /// <summary>
        ///     攻撃実行処理。実行後、次のバトルフェーズ攻撃を予約する。
        /// </summary>
        private void Attack(CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                Debug.Log("敵側：予約アクションキャンセル済み、何もしない");
                _isBattlePhase = false;
                return;
            }
            if (!_isBattlePhase)
            {
                _isBattlePhase = true;
                ScheduledBattale();
            }
        }
    }
}