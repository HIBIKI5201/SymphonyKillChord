using KillChord.Runtime.Application.InGame.Mission;
using KillChord.Runtime.Domain.InGame.Buff;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.Mission.ClearCondition;
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
            RemoveActiveBuffs();
        }

        private readonly MissionRuntimeService _missionRuntimeService;
        private readonly ObjectiveSequenceClearCondition _objectiveSequence;
        private readonly CharacterEntity _playerEntity;
        private readonly List<IBuff> _activeBuffs = new();

        /// <summary>
        ///     目標ステップの変化に応じてプレイヤーへのバフ・デバフ付与を切り替えます。
        /// </summary>
        /// <param name="stepIndex"> 開始した目標ステップのIndexです。 </param>
        private void HandleObjectiveStepChanged(int stepIndex)
        {
            RemoveActiveBuffs();

            ObjectiveSequenceStep step = _objectiveSequence.GetStep(stepIndex);
            PlayerBuffClearCondition buffCondition = step != null ? ClearConditionChain.Find<PlayerBuffClearCondition>(step.Condition) : null;
            if (buffCondition == null)
            {
                return;
            }

            for (int i = 0; i < buffCondition.Buffs.Count; i++)
            {
                IBuff buff = buffCondition.Buffs[i];
                _playerEntity.BuffSystem.Add(buff);
                _activeBuffs.Add(buff);
            }
        }

        /// <summary>
        ///     前のステップで付与したバフ・デバフを全て解除します。
        /// </summary>
        private void RemoveActiveBuffs()
        {
            for (int i = 0; i < _activeBuffs.Count; i++)
            {
                _playerEntity.BuffSystem.Remove(_activeBuffs[i]);
            }
            _activeBuffs.Clear();
        }
    }
}
