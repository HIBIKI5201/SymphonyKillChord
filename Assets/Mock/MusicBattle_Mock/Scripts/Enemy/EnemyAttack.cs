using Codice.Client.BaseCommands.FastExport;
using Mock.MusicBattle.Basis;
using Mock.MusicBattle.Character;
using Mock.MusicBattle.MusicSync;
using System;
using System.Threading;
using UnityEngine;

namespace Mock.MusicBattle.Enemy
{
    /// <summary>
    ///     敵の攻撃を音楽同期で管理するクラス。
    ///     遭遇フェーズとバトルフェーズのタイミングで攻撃予約を行う。
    /// </summary>
    public class EnemyAttack : IDisposable
    {
        /// <summary>
        ///     <see cref="EnemyAttack"/>クラスの新しいインスタンスを初期化します。
        ///     必要なマネージャーや設定データを受け取り、イベント登録を行います。
        /// </summary>
        public EnemyAttack(EnemyManager enemy, MusicSyncManager music,
            EnemyMusicSO encount, EnemyMusicSO battle, ICharacter player,
            EnemyStatus enemyStatus, AttackIndicater indicater)

        {
            if (music == null) Debug.LogError("musicがNULLです！");
            if (encount == null) Debug.LogError("encountがNULLです！");
            if (battle == null) Debug.LogError("battleがNULLです！");

            _enemyManager = enemy;
            _musicSyncManager = music;
            _encount = encount;
            _battle = battle;
            _player = player;
            _enemyStatus = enemyStatus;
            _indicater = indicater;

            _enemyManager.OnAttack += OnAttackHandler;
            _enemyManager.OnOutOfRange += CancelScheduled;
        }

        #region パブリックインターフェースメソッド
        /// <summary>
        ///     このインスタンスによって使用されているリソースを解放します。
        /// </summary>
        public void Dispose()
        {
            _enemyManager.OnAttack -= OnAttackHandler;
            _enemyManager.OnOutOfRange -= CancelScheduled;
            _cancellationTokenSource?.Cancel(); // 予約中のタスクをキャンセル
            _cancellationTokenSource?.Dispose(); // CancellationTokenSourceを破棄
        }
        #endregion

        #region プライベートフィールド
        /// <summary> 遭遇フェーズの音楽SO。 </summary>
        private readonly EnemyMusicSO _encount;
        /// <summary> バトルフェーズの音楽SO。 </summary>
        private readonly EnemyMusicSO _battle;
        /// <summary> 攻撃予約キャンセル用CancellationTokenSource。 </summary>
        private CancellationTokenSource _cancellationTokenSource;
        /// <summary> 敵マネージャーの参照。 </summary>
        private readonly EnemyManager _enemyManager;
        /// <summary> 音楽同期マネージャーの参照。 </summary>
        private readonly MusicSyncManager _musicSyncManager;
        /// <summary> プレイヤーキャラクターの参照。 </summary>
        private readonly ICharacter _player;
        /// <summary> 敵のステータス。 </summary>
        private readonly EnemyStatus _enemyStatus;
        /// <summary> 攻撃インジケーター。 </summary>
        private readonly AttackIndicater _indicater;
        /// <summary> 現在バトルフェーズ中かどうかを示すフラグ。 </summary>
        private bool _isBattlePhase = false;
        #endregion

        #region イベントハンドラメソッド
        /// <summary>
        ///     敵が攻撃可能になった際に呼ばれ、遭遇フェーズの攻撃スケジュールを開始します。
        /// </summary>
        public void OnAttackHandler()
        {
            ScheduledEncount();
        }
        #endregion

        #region Privateメソッド
        /// <summary>
        ///     バトルフェーズの攻撃タイミングを音楽同期アクションとして予約します。
        /// </summary>
        private void ScheduledBattle()
        {
            CancelScheduled();
            _indicater.Move(_enemyStatus.AttackRange);
            _indicater.Visible = true;
            BarTimingInfo barTimingInfo = new BarTimingInfo(_battle.BarFlg, _battle.TimeSignature, _battle.TargetBeat);
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationTokenSource.Token.Register(() => _indicater.Visible = false);
            _musicSyncManager.RegisterAction(barTimingInfo, () =>
            {
                _isBattlePhase = false;
                Attack(_cancellationTokenSource.Token);
            }, _cancellationTokenSource.Token);
        }

        /// <summary>
        ///     遭遇フェーズの攻撃タイミングを音楽同期アクションとして予約します。
        /// </summary>
        private void ScheduledEncount()
        {
            CancelScheduled();
            _indicater.Move(_enemyStatus.AttackRange);
            _indicater.Visible = true;
            BarTimingInfo barTimingInfo = new BarTimingInfo(_encount.BarFlg, _encount.TimeSignature, _encount.TargetBeat);
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationTokenSource.Token.Register(() => _indicater.Visible = false);
            _musicSyncManager.RegisterAction(barTimingInfo, () => Attack(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
        }

        /// <summary>
        ///     予約中の攻撃アクションをキャンセルします。
        /// </summary>
        private void CancelScheduled()
        {
            if (_cancellationTokenSource == null) { return; }

            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose(); // CancellationTokenSourceを破棄。
            _cancellationTokenSource = null;
        }

        /// <summary>
        ///     攻撃実行処理。実行後、次のバトルフェーズ攻撃を予約します。
        /// </summary>
        /// <param name="token">非同期処理のキャンセルトークン。</param>
        private void Attack(CancellationToken token)
        {
            _indicater.Visible = false;
            if (token.IsCancellationRequested)
            {
                Debug.Log("敵側：予約アクションキャンセル済み、何もしません。");
                _isBattlePhase = false;
                return;
            }
            if (!_isBattlePhase)
            {
                ParticleController.Instance.PlayParticle(_enemyManager.transform.position);
                _player.TakeDamage(_enemyStatus.AttackPower);
                ScheduledBattle();
                _isBattlePhase = true;
            }
        }
        #endregion
    }
}
