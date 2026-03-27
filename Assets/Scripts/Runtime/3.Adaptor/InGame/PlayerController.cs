using KillChord.Runtime.Application;
using KillChord.Runtime.Domain;
using UnityEngine;

namespace KillChord.Runtime.Adaptor
{
    public class PlayerController
    {
        public PlayerController(PlayerApplication playerApplication,
            AttackExecutor attackExecutor,
            AttackCommandState commandState,
            AttackBattleState battleState)
        {
            _playerApplication = playerApplication;
            _attackExecutor = attackExecutor;
            _commandState = commandState;
            _battleState = battleState;
        }

        public bool TryDodge(Vector2 input, float time)
            => _playerApplication.TryDodge(input, time);
        public void Update(ref Quaternion rotation, Vector2 input, float time, out Vector3 velocity)
        {
            _playerApplication.Update(ref rotation, input, time, out velocity);
        }

        public void ChangeAttack(AttackCommandType commandType)
        {
            _commandState.SelectAttack(commandType);
        }

        /// <summary>
        ///     現在の戦闘状態と選択された攻撃コマンドに基づいて攻撃処理を実行しする。
        /// </summary>
        public void ExecuteAttack()
        {
            AttackId attackId = _commandState.SelectedAttackId;
            AttackResult result = _attackExecutor.Execute(
                _battleState.Attacker,
                _battleState.Target,
                attackId);
        }

        private readonly PlayerApplication _playerApplication;

        private readonly AttackExecutor _attackExecutor;
        private readonly AttackCommandState _commandState;
        private readonly AttackBattleState _battleState;
    }
}
