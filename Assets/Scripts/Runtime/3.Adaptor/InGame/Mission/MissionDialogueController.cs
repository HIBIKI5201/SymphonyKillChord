using KillChord.Runtime.Adaptor.InGame.Sequence;
using KillChord.Runtime.Adaptor.Persistent.Music;
using KillChord.Runtime.Application.InGame.Mission;
using KillChord.Runtime.Domain.InGame.Mission;
using KillChord.Runtime.Domain.InGame.Mission.StepEntryAction;
using System;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Mission
{
    /// <summary>
    ///     ステップの会話の順次再生するコントローラー。
    /// </summary>
    public sealed class MissionDialogueController : IMissionStepEntryActionExecutor, IDisposable
    {
        public MissionDialogueController(MissionRuntimeService mission, BattlePauseController pause,
            IControllableVoiceSource voice, MissionDialoguePresenter presenter)
        {
            _mission = mission ?? throw new ArgumentNullException(nameof(mission));
            _pauseController = pause ?? throw new ArgumentNullException(nameof(pause));
            _voice = voice ?? throw new ArgumentNullException(nameof(voice));
            _presenter = presenter ?? throw new ArgumentNullException(nameof(presenter));
            _isPaused = pause.IsPaused;
            _mission.OnObjectiveStepChanged += ObjectiveStepChangedHandler;
            _mission.OnMissionFinished += MissionFinishedHandler;
            _pauseController.OnPaused += PausedHandler;
            _pauseController.OnResumed += ResumedHandler;
        }

        /// <inheritdoc />
        public Type EntryActionType => typeof(PlayDialogueStepEntryAction);

        /// <inheritdoc />
        public void Execute(IMissionStepEntryAction entryAction)
        {
            if (entryAction is not PlayDialogueStepEntryAction dialogue)
            {
                throw new ArgumentException("Ingame会話アクションではありません。", nameof(entryAction));
            }
            if (_isDisposed || _mission.MissionProgress.IsFinished)
            {
                return;
            }

            _voice.StopVoice();
            _dialogue = dialogue;
            _stepIndex = _mission.MissionProgress.ObjectiveStepIndex;
            _lineIndex = -1;
            _isQueuedUntilGameplayStarts = !_isGameplayActive;
            if (_isVisible)
            {
                BeginHide();
            }
            else if (!_isClosing)
            {
                TryStartDialogue();
            }
        }

        /// <summary>
        ///     会話を再生する。
        /// </summary>
        public void StartGameplay()
        {
            if (_isDisposed)
            {
                return;
            }
            _isGameplayActive = true;
            TryStartDialogue();
        }

        /// <summary>
        ///     ゲーム終了時に音声と演出を直ちに解除する。
        /// </summary>
        public void StopGameplay()
        {
            if (_isGameplayActive || _mission.MissionProgress.IsFinished || _isDisposed)
            {
                _dialogue = null;
                _isQueuedUntilGameplayStarts = false;
            }
            _isGameplayActive = false;
            _voice.StopVoice();
            _isVisible = false;
            _isClosing = false;
            _version++;
            Present(true);
        }

        /// <summary>
        ///     音声終了、またはボイス無しの表示時間を確認する。
        /// </summary>
        public void Tick(float deltaTime)
        {
            if (_isDisposed || !_isGameplayActive || _isPaused || _isClosing || !_isVisible)
            {
                return;
            }
            if (_isVoiced)
            {
                if (_voice.IsVoiceActive && !_voice.HasVoiceError)
                {
                    return;
                }
                if (_voice.HasVoiceError)
                {
                    Debug.LogWarning($"[{nameof(MissionDialogueController)}] 音声の再生に失敗しました：{_displayedLine.VoiceCueName}");
                }
            }
            else
            {
                _remainingSeconds -= Mathf.Max(0f, deltaTime);
                if (_remainingSeconds > 0f)
                {
                    return;
                }
            }
            PlayNextLine();
        }

        /// <summary>
        ///     会話再生終了演出の完了通知を受け、次の会話を開始する。
        /// </summary>
        public void NotifyHidden(int version)
        {
            if (_isDisposed || !_isClosing || version != _version)
            {
                return;
            }
            _isClosing = false;
            TryStartDialogue();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }
            _isDisposed = true;
            _mission.OnObjectiveStepChanged -= ObjectiveStepChangedHandler;
            _mission.OnMissionFinished -= MissionFinishedHandler;
            _pauseController.OnPaused -= PausedHandler;
            _pauseController.OnResumed -= ResumedHandler;
            StopGameplay();
        }

        private readonly MissionRuntimeService _mission;
        private readonly BattlePauseController _pauseController;
        private readonly IControllableVoiceSource _voice;
        private readonly MissionDialoguePresenter _presenter;
        private PlayDialogueStepEntryAction _dialogue;
        private MissionDialogueLine _displayedLine;
        private int _stepIndex = -1;
        private int _lineIndex = -1;
        private int _version;
        private float _remainingSeconds;
        private bool _isVoiced;
        private bool _isVisible;
        private bool _isClosing;
        private bool _isGameplayActive;
        private bool _isQueuedUntilGameplayStarts;
        private bool _isPaused;
        private bool _isDisposed;

        /// <summary>
        ///     会話の所有ステップを確認する。
        /// </summary>
        private void ObjectiveStepChangedHandler(int stepIndex)
        {
            if (_stepIndex == stepIndex)
            {
                return;
            }
            if (_isQueuedUntilGameplayStarts)
            {
                return;
            }
            _dialogue = null;
            _voice.StopVoice();
            if (_isVisible)
            {
                BeginHide();
            }
        }

        /// <summary>
        ///     Mission終了時に再生を停止する。
        /// </summary>
        private void MissionFinishedHandler(MissionEndReason reason)
        {
            StopGameplay();
        }

        /// <summary>
        ///     会話再生を一時停止する。
        /// </summary>
        private void PausedHandler()
        {
            _isPaused = true;
            _voice.SetVoicePaused(true);
            Present();
        }

        /// <summary>
        ///     会話再生を再開する。
        /// </summary>
        private void ResumedHandler()
        {
            _isPaused = false;
            _voice.SetVoicePaused(false);
            Present();
            TryStartDialogue();
        }

        /// <summary>
        ///     次の会話を再生する（再生可能な場合）。
        /// </summary>
        private void TryStartDialogue()
        {
            if (_isDisposed || !_isGameplayActive || _isPaused || _isClosing || _isVisible || _dialogue == null
                || _mission.MissionProgress.IsFinished
                || (!_isQueuedUntilGameplayStarts && _stepIndex != _mission.MissionProgress.ObjectiveStepIndex))
            {
                return;
            }
            _isQueuedUntilGameplayStarts = false;
            _version++;
            PlayNextLine();
        }

        /// <summary>
        ///     次の台詞を再生する。
        /// </summary>
        private void PlayNextLine()
        {
            _voice.StopVoice();
            while (_dialogue != null && ++_lineIndex < _dialogue.Lines.Count)
            {
                _displayedLine = _dialogue.Lines[_lineIndex];
                _isVoiced = !string.IsNullOrWhiteSpace(_displayedLine.VoiceCueName);
                if (_isVoiced && !_voice.TryPlayVoice(_displayedLine.VoiceCueName))
                {
                    Debug.LogWarning($"[{nameof(MissionDialogueController)}] Cueを再生できません：{_displayedLine.VoiceCueName}");
                    _isVoiced = false;
                }
                _remainingSeconds = _displayedLine.SilentDuration;
                _isVisible = true;
                Present();
                return;
            }
            _dialogue = null;
            if (_isVisible)
            {
                BeginHide();
            }
        }

        /// <summary>
        ///     会話文字の退場演出を要求する。
        /// </summary>
        private void BeginHide()
        {
            _isVisible = false;
            _isClosing = true;
            _version++;
            Present();
        }

        /// <summary>
        ///     表示情報をPresenterへ渡す。
        /// </summary>
        private void Present(bool isImmediate = false)
        {
            _presenter.Present(_displayedLine, _isVisible, _isPaused, _version, isImmediate);
        }
    }
}
