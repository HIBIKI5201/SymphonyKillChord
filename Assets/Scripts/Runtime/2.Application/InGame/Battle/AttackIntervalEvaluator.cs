using Cysharp.Threading.Tasks;
using KillChord.Runtime.Domain.InGame.Battle;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Battle
{
    /// <summary>
    /// 攻撃時の硬直時間を計測するクラス。攻撃の開始から指定時間が経過するまで、攻撃中フラグを立てる。
    /// </summary>
    public class AttackIntervalEvaluator
    {
        /// <summary>
        ///     初期化するコンストラクタ。
        ///     攻撃の硬直状態を管理するAttackIntervalEntityを受け取る。
        /// </summary>
        /// <param name="attackIntervalEntity"></param>
        public AttackIntervalEvaluator(AttackIntervalEntity attackIntervalEntity)
        {
            _attackIntervalEntity = attackIntervalEntity;
        }

        /// <summary>
        ///     攻撃の硬直時間を評価するメソッド。攻撃の開始から指定時間が経過するまでIsAttackingフラグを立てる。
        /// </summary>
        public void EvaluateInterval()
        {
            int attackIntervalId = ++_currentIntervalId;
            EvaluateAttackIntervalAsync(_attackIntervalEntity.Interval, attackIntervalId).Forget();
        }
        
        private readonly AttackIntervalEntity _attackIntervalEntity;
        private int _currentIntervalId;

        /// <summary>
        ///     攻撃中の硬直時間を管理する。攻撃中は開始から一定時間が経過するまで一部入力を無効化するための_isAttackingフラグを立てる。
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="attackId"></param>
        private async UniTaskVoid EvaluateAttackIntervalAsync(AttackInterval duration, int attackId)
        {
            Debug.Log($"AttackIntervalEvaluator: Start evaluating attack interval. Duration={duration.Value}, AttackId={attackId}");
            _attackIntervalEntity.UpdateAttackState(true);
            await UniTask.Delay((int)(duration * 1000f));

            // フラグの更新は与えられたIDが最新の時のみ行う。
            if (attackId == _currentIntervalId)
            {
                _attackIntervalEntity.UpdateAttackState(false);
                Debug.Log($"AttackIntervalEvaluator: Finished evaluating attack interval. AttackId={attackId}");
            }
        }
    }
}