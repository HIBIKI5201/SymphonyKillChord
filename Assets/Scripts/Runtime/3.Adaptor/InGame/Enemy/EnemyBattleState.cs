using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Character;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Enemy
{
    /// <summary>
    ///     敵Aiの攻撃に関する戦闘状態をまとめたクラス。
    /// </summary>
    public class EnemyBattleState
    {
        public EnemyBattleState(CharacterEntity attacker,
            CharacterEntity target,
            AttackDefinition currentAttack)
        {
            Attacker = attacker;
            Target = target;
            CurrentAttack = currentAttack;
            FirstAttack = true;
            IsStunned = false;
            IsBattleAIActivated = true;
            IsDiscovered = false;
        }

        /// <summary> 攻撃者（自身）のエンティティ </summary>
        public CharacterEntity Attacker { get; }
        /// <summary> 攻撃目標のエンティティ </summary>
        public CharacterEntity Target { get; }
        /// <summary> 攻撃情報 </summary>
        public AttackDefinition CurrentAttack { get; }

        /// <summary> 攻撃目標が攻撃可能な範囲に居るか </summary>
        public bool IsInAttackRange { get; private set; }
        /// <summary> 初回攻撃か </summary>
        public bool FirstAttack { get; private set; }
        /// <summary> 硬直中か </summary>
        public bool IsStunned { get; private set; }
        public bool IsBattleAIActivated { get; private set; }
        /// <summary> 攻撃後の行動選択によって一時的に上書きされた移動先 </summary>
        public Vector3? OverrideDestination { get; private set; }
        /// <summary> プレイヤーを発見済みか </summary>
        public bool IsDiscovered { get; private set; }

        /// <summary> 攻撃目標が攻撃範囲に入った </summary>
        public void EnterRange() => IsInAttackRange = true;
        /// <summary> 攻撃目標が攻撃範囲から出た </summary>
        public void ExitRange() => IsInAttackRange = false;
        /// <summary> 攻撃を実行した </summary>
        public void AttackExcuted() => FirstAttack = false;
        /// <summary> 硬直発生 </summary>
        public void Stunned() => IsStunned = true;
        /// <summary> 硬直から回復した </summary>
        public void StunRecover() => IsStunned = false;
        /// <summary> 敵の戦闘系AIを有効化 </summary>
        public void StartBattleAI() => IsBattleAIActivated = true;
        /// <summary> 敵の戦闘系AIを無効化 </summary>
        public void StopBattleAI() => IsBattleAIActivated = false;
        /// <summary> 攻撃後の一時的な移動先を設定する </summary>
        public void SetOverrideDestination(Vector3 destination) => OverrideDestination = destination;
        /// <summary> 一時的な移動先の上書きを解除する </summary>
        public void ClearOverrideDestination() => OverrideDestination = null;
        /// <summary> プレイヤーを発見済みにする </summary>
        public void Discover() => IsDiscovered = true;

        /// <summary>
        ///     再初期化処理。
        /// </summary>
        public void Reset()
        {
            IsInAttackRange = false;
            FirstAttack = true;
            IsStunned = false;
            IsBattleAIActivated = true;
            OverrideDestination = null;
            IsDiscovered = false;
        }
    }
}
