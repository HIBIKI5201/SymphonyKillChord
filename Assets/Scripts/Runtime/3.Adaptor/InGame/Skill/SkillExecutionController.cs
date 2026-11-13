using KillChord.Runtime.Application.InGame.Skill;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.Utility.Diagnostics;
using KillChord.Runtime.Utility.Persistent;
using System;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Skill
{
    /// <summary>
    ///     スキルの発動を管理するコントローラー。
    /// </summary>
    public class SkillExecutionController
    {
        /// <summary>
        ///     音楽再生位置の巻き戻しとみなす許容誤差（秒）です。
        ///     MusicSyncServiceの巻き戻し検知と同じ扱いで、入力履歴のTimingが非単調になるのを防ぎます。
        /// </summary>
        private const float PLAYBACK_REWIND_TOLERANCE_SECONDS = 0.01f;

        /// <summary>
        ///     コントローラーを初期化します。
        /// </summary>
        public SkillExecutionController(
            SkillResultPresenter presenter,
            SkillInputProgressController progressController,
            SkillCooldownState skillCooldownState,
            SkillUseCase skillUseCase,
            SkillCheckService skillCheckService,
            ISkillVisual skillVisual,
            SkillDefinition skillDefinition,
            SkillRhythmState skillRhythmState,
            bool isJustBonusEnabled = false)
        {
            _presenter = presenter;
            _progressController = progressController;
            _skillCooldownState = skillCooldownState;
            _skillUseCase = skillUseCase;
            _skillCheckService = skillCheckService;
            _skillVisual = skillVisual;
            _skillDefinition = skillDefinition;
            _skillRhythmState = skillRhythmState;
            _isJustBonusEnabled = isJustBonusEnabled;
        }

        /// <summary>
        ///     スキル発動を試す。
        /// </summary>
        /// <param name="beatType"> 現在ビートです。 </param>
        /// <param name="now"> クールダウンやUI表示に使う、ゲーム側の現在時刻です。 </param>
        /// <param name="musicTime"> リズム入力履歴に記録する、音楽の再生時間です。 </param>
        /// <param name="battleActionType"> 行動種別です。 </param>
        /// <param name="isJustHit"> ジャスト入力によるスキル発動かどうか。 </param>
        /// <returns> 実行結果です。 </returns>
        public SkillExecutionResult TryExecuteSkill(BeatType beatType, float now, float musicTime, BattleActionType battleActionType, bool isJustHit)
        {
            if (!_skillCooldownState.IsSkillReady(now))
            {
                DevLog.Log($"[SkillExecutionController] クールダウン中。ID：{_skillDefinition.Id.Value}");
                return new SkillExecutionResult(SkillExecutionResultType.CooldownBlocked);
            }

            // 音楽の再生位置が巻き戻った場合、Timingが非単調になり不整合を招くため履歴を破棄する。
            if (_skillRhythmState.Count > 0
                && musicTime + PLAYBACK_REWIND_TOLERANCE_SECONDS < _skillRhythmState.LastTiming)
            {
                _skillRhythmState.Clear();
            }

            _skillRhythmState.Enqueue(beatType, musicTime, battleActionType, isJustHit);
            ReadOnlySpan<BeatType> inputHistory = _skillRhythmState.GetHistoryBeatType();
            bool isInputMatch = _skillCheckService.CheckInput(_skillDefinition, inputHistory);

            if (!isInputMatch)
            {
                _progressController.UpdateProgress(now, _skillCooldownState.SkillReadyTimestamp, beatType);
                return new SkillExecutionResult(SkillExecutionResultType.InputProgressed);
            }

            float? justBonusEffectRate = CalculateJustBonusEffectRate();
            bool canExecute = _skillUseCase.TryExecuteSkill(_skillDefinition, beatType, isJustHit, justBonusEffectRate);
            if (canExecute)
            {
                ExecuteVisual();

                // カメラシェイクなどの演出用に、スキルが発動したことを通知する。対象の有無は問わない。
                EventBus<EOnSkillExecuted>.Raise(new EOnSkillExecuted(_skillDefinition.Id.Value));

                _presenter.Push(_skillDefinition);
                _skillRhythmState.Clear();
                _skillCooldownState.SetSkillCooldown(now);
                _progressController.SkillTriggered(now, _skillCooldownState.SkillReadyTimestamp);
                return new SkillExecutionResult(
                    SkillExecutionResultType.Executed,
                    _skillDefinition.AnimationKey,
                    _skillDefinition.EffectSpec.SkillNormalAttackDamagePolicy,
                    ResolveWeaponBeatType(),
                    _skillDefinition.RecoveryBeats);
            }

            ResetProgressOnFailure(now);
            Debug.LogWarning(
                $"[{nameof(SkillExecutionController)}] 入力パターンは一致しましたが、" +
                $"対象を解決できないためスキルを実行しませんでした。ID: {_skillDefinition.Id.Value}, " +
                $"TargetingType: {_skillDefinition.EffectSpec.TargetingType}");
            return new SkillExecutionResult(SkillExecutionResultType.RejectedByTargetPolicy);
        }

        /// <summary>
        ///     再使用可能時刻を変えずに入力履歴と進捗表示をリセットします。
        /// </summary>
        /// <param name="now"> 進捗表示に使うゲーム側の現在時刻です。 </param>
        public void ResetInputProgress(float now)
        {
            _skillRhythmState.Clear();
            _progressController.ResetProgress(now, _skillCooldownState.SkillReadyTimestamp);
        }

        /// <summary>
        ///     コマンドの入力のうちJustで入力した割合から、Justボーナスの効果の割合を求めます。
        /// </summary>
        /// <returns> 効果の割合です。Justボーナスを使わない場合はnullです。 </returns>
        private float? CalculateJustBonusEffectRate()
        {
            if (!_isJustBonusEnabled)
            {
                return null;
            }

            // 一致したコマンドは入力履歴の新しい方からパターンの長さ分にあたる。
            int inputCount = _skillDefinition.SkillPattern.Signatures.Length;
            int justCount = _skillRhythmState.CountRecentJust(inputCount);
            float effectRate = _skillDefinition.JustBonus.CalculateEffectRate(justCount, inputCount);
            DevLog.Log($"[{nameof(SkillExecutionController)}] Justボーナス。ID：{_skillDefinition.Id.Value}, Just：{justCount}/{inputCount}, 効果：{effectRate:P0}");
            return effectRate;
        }

        /// <summary>
        ///     このスキルで構える武器を決めるBeatTypeを求めます。
        /// </summary>
        /// <returns> 入力パターンの最後のBeatTypeです。 </returns>
        private BeatType ResolveWeaponBeatType()
        {
            // 決め手となる最後の拍の武器を構える。
            System.ReadOnlySpan<BeatType> signatures = _skillDefinition.SkillPattern.Signatures;
            return signatures.Length > 0 ? signatures[signatures.Length - 1] : default;
        }

        /// <summary>
        ///     視覚演出を実行します。
        /// </summary>
        private void ExecuteVisual()
        {
            _skillVisual?.Execute();
        }

        /// <summary>
        ///     対象不成立時に、入力の進行だけをリセットします。
        ///     クールダウンは消費しません。
        /// </summary>
        /// <param name="now"> 現在時刻です。 </param>
        private void ResetProgressOnFailure(float now)
        {
            _skillRhythmState.Clear();
            _progressController.ResetProgress(now, _skillCooldownState.SkillReadyTimestamp);
        }

        private readonly SkillResultPresenter _presenter;
        private readonly SkillInputProgressController _progressController;
        private readonly SkillCooldownState _skillCooldownState;
        private readonly SkillUseCase _skillUseCase;
        private readonly SkillCheckService _skillCheckService;
        private readonly ISkillVisual _skillVisual;
        private readonly SkillDefinition _skillDefinition;
        private readonly SkillRhythmState _skillRhythmState;
        private readonly bool _isJustBonusEnabled;
    }
}
