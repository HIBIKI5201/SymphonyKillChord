using KillChord.Runtime.Application.InGame.Mission;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.Mission.ClearCondition;
using KillChord.Runtime.Domain.InGame.StatusEffect;
using System;
using System.Collections.Generic;

namespace KillChord.Runtime.Adaptor.InGame.Mission
{
    /// <summary>
    ///     Missionの目標ステップとプレイヤーへのバフ・デバフ付与を仲介するControllerです。
    /// </summary>
    public sealed class MissionPlayerBuffController : IDisposable
    {
        /// <summary>
        ///     Missionの目標ステップとプレイヤーへのバフ・デバフ付与を結合します。
        /// </summary>
        /// <param name="missionRuntimeService"> Missionのランタイムサービスです。 </param>
        /// <param name="objectiveSequence"> ステップを含む目標シーケンスです。 </param>
        /// <param name="playerEntity"> バフ・デバフを付与するプレイヤーEntityです。 </param>
        public MissionPlayerBuffController(
            MissionRuntimeService missionRuntimeService,
            ObjectiveSequenceClearCondition objectiveSequence,
            CharacterEntity playerEntity)
        {
            _missionRuntimeService = missionRuntimeService
                ?? throw new ArgumentNullException(nameof(missionRuntimeService));
            _objectiveSequence = objectiveSequence
                ?? throw new ArgumentNullException(nameof(objectiveSequence));
            _playerEntity = playerEntity
                ?? throw new ArgumentNullException(nameof(playerEntity));

            _missionRuntimeService.OnObjectiveStepChanged += HandleObjectiveStepChanged;
        }

        /// <summary>
        ///     Missionイベントの購読を解除し、付与中のバフ・デバフを解除します。
        /// </summary>
        public void Dispose()
        {
            _missionRuntimeService.OnObjectiveStepChanged -= HandleObjectiveStepChanged;
            RemoveActiveStatusEffect();
        }

        private readonly MissionRuntimeService _missionRuntimeService;
        private readonly ObjectiveSequenceClearCondition _objectiveSequence;
        private readonly CharacterEntity _playerEntity;
        private readonly List<IStatusEffect> _activeStatusEffects = new();

        /// <summary>
        ///     目標ステップの変化に応じてプレイヤーへのバフ・デバフ付与を切り替えます。
        /// </summary>
        /// <param name="stepIndex"> 開始した目標ステップのIndexです。 </param>
        private void HandleObjectiveStepChanged(int stepIndex)
        {
            RemoveActiveStatusEffect();

            ObjectiveSequenceStep step = _objectiveSequence.GetStep(stepIndex);
            PlayerBuffClearCondition buffCondition = step != null ? ClearConditionChain.Find<PlayerBuffClearCondition>(step.Condition) : null;
            if (buffCondition == null)
            {
                return;
            }

            // このステップで付与するバフ・デバフをプレイヤーへ付与します。
            for (int i = 0; i < buffCondition.StatusEffects.Count; i++)
            {
                IStatusEffect statusEffect = buffCondition.StatusEffects[i];
                _playerEntity.StatusEffectSystem.Add(statusEffect);
                _activeStatusEffects.Add(statusEffect);
            }
        }

        /// <summary>
        ///     前のステップで付与したバフ・デバフを全て解除します。
        /// </summary>
        private void RemoveActiveStatusEffect()
        {
            for (int i = 0; i < _activeStatusEffects.Count; i++)
            {
                _playerEntity.StatusEffectSystem.Remove(_activeStatusEffects[i]);
            }
            _activeStatusEffects.Clear();
        }
    }
}
