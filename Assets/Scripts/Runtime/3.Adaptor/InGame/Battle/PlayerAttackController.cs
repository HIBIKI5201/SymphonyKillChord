using KillChord.Runtime.Adaptor.InGame.Skill;
using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Battle;
using System;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Battle
{
    /// <summary>
    ///     Viewの押された拍情報の入力を受け取り、攻撃処理の実行と結果の表示を仲介するクラス。
    /// </summary>
    public class PlayerAttackController
    {
        /// <summary>
        ///     コンストラクタ。
        /// </summary>
        /// <param name="attackExecutor"></param>
        /// <param name="presenter"></param>
        /// <param name="commandState"></param>
        /// <param name="battleState"></param>
        /// <param name="skillController"></param>
        public PlayerAttackController(
            AttackResultPresenter presenter,
            PlayerBattleState battleState,
            SkillController skillController,
            TargetSelectorController targetSelectorController
        )
        {
            _presenter = presenter;
            _battleState = battleState;
            _skillController = skillController;
            _targetSelectorController = targetSelectorController;
        }

        /// <summary>
        ///     現在の戦闘状態と押された拍情報に基づいて攻撃処理を実行しする。
        /// </summary>
        public bool ExecuteAttack()
        {
            if (_targetSelectorController == null)
            {
                Debug.LogError("TargetSelectorControllerが設定されていません。");
                return false;
            }
            if (!_targetSelectorController.TryGetCurrentTargetEntity(out var targetEntity))
            {
                Debug.Log("攻撃対象が選択されていません。");
                return false;
            }
            _battleState.ChangeTarget(targetEntity);

            int beatType = _skillController.CheckSkill(BattleActionType.Attack);

            AttackDefinition attackDefinition;
            try
            {
                attackDefinition = _battleState.Attacker.CombatSpec.GetAttackDifinition(beatType);
            }
            catch (InvalidOperationException ex)
            {
                Debug.LogWarning(ex.Message);
                return false;
            }

            AttackResult result = AttackExecutor.Execute(attackDefinition,
                _battleState.Attacker,
                _battleState.Target);

            _presenter.Push(result);
            return true;
        }

        private readonly AttackResultPresenter _presenter;
        private readonly PlayerBattleState _battleState;
        private readonly SkillController _skillController;
        private readonly TargetSelectorController _targetSelectorController;
    }
}