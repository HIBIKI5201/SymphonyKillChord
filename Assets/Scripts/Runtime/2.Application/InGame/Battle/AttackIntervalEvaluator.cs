using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using KillChord.Runtime.Domain.InGame.Battle;

namespace KillChord.Runtime.Application.InGame.Battle
{
    /// <summary>
    /// 攻撃時の硬直時間を計測及び管理するクラス。攻撃の開始から指定時間が経過するまで、攻撃中フラグを立てる。
    /// </summary>
    public class AttackIntervalEvaluator
    {
        /// <summary>
        ///     初期化するコンストラクタ。
        ///     攻撃の硬直状態を管理するAttackIntervalEntityを受け取る。
        /// </summary>
        /// <param name="attackIntervalEntity"></param>
        /// <param name="lifetimeToken"> 所有者の寿命のトークン。キャンセルされると硬直待ちを打ち切る。 </param>
        public AttackIntervalEvaluator(AttackIntervalEntity attackIntervalEntity, CancellationToken lifetimeToken)
        {
            if (attackIntervalEntity == null)
            {
                throw new ArgumentNullException(nameof(attackIntervalEntity));
            }
            
            _attackIntervalEntity = attackIntervalEntity;
            _lifetimeToken = lifetimeToken;
        }
        
        /// <summary>
        ///     現在攻撃中かどうかを表すプロパティ。
        ///     保持しているAttackIntervalEntityのIsAttackingプロパティを参照する。
        /// </summary>
        public bool IsAttacking => _attackIntervalEntity.IsAttacking;

        /// <summary>
        ///     攻撃の硬直時間を評価するメソッド。攻撃の開始から指定時間が経過するまでIsAttackingフラグを立てる。
        /// </summary>
        public void EvaluateInterval()
        {
            int attackIntervalId = ++_currentIntervalId;
            EvaluateAttackIntervalAsync(_attackIntervalEntity.Interval, attackIntervalId).Forget();
        }
        
        private readonly AttackIntervalEntity _attackIntervalEntity;
        private readonly CancellationToken _lifetimeToken;
        private int _currentIntervalId;

        /// <summary>
        ///     攻撃中の硬直時間を管理する。攻撃中は開始から一定時間が経過するまで一部入力を無効化するための_isAttackingフラグを立てる。
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="attackId"></param>
        private async UniTaskVoid EvaluateAttackIntervalAsync(AttackInterval duration, int attackId)
        {
            _attackIntervalEntity.UpdateAttackState(true);
            // 所有者（シーン）が破棄されたら待機を打ち切り、破棄後に Entity を書き換えない。
            bool isCanceled = await UniTask.Delay((int)(duration * 1000f), cancellationToken: _lifetimeToken)
                .SuppressCancellationThrow();
            if (isCanceled)
            {
                return;
            }

            // フラグの更新は与えられたIDが最新の時のみ行う。
            if (attackId == _currentIntervalId)
            {
                _attackIntervalEntity.UpdateAttackState(false);
            }
        }
    }
}