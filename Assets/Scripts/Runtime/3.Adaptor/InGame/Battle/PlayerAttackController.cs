using KillChord.Runtime.Adaptor.InGame.Skill;
using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Domain.InGame.Battle;
using System;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Battle
{
    public class PlayerAttackController
    {
        public PlayerAttackController(
            AttackResultPresenter presenter,
            PlayerBattleState battleState,
            SkillController skillController,
            TargetSelectorController targetSelectorController,
            IMusicSyncService musicSyncService
        )
        {
            _presenter = presenter;
            _battleState = battleState;
            _skillController = skillController;
            _targetSelectorController = targetSelectorController;
            _musicSyncService = musicSyncService;
        }

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

            float now = Time.unscaledTime;
            int beatType = _musicSyncService.GetCurrentBeatType(now);

            _skillController.CheckSkill(BattleActionType.Attack, beatType, now);

            AttackDefinition attackDefinition;
            try
            {
                attackDefinition = _battleState.Attacker.CombatSpec.GetAttackDefinitionByBeatType(beatType);
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
        private readonly IMusicSyncService _musicSyncService;
    }
}