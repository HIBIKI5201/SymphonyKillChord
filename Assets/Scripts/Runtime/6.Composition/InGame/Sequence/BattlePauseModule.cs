using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.Adaptor.InGame.Sequence;
using KillChord.Runtime.Composition.Persistent.Input;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.View.Persistent.Input;
using KillChord.Runtime.View.Persistent.Music;
using R3;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.Sequence
{
    /// <summary>
    ///     戦闘をポーズするモジュール。
    /// </summary>
    public class BattlePauseModule : IBattlePauseModule, IDisposable
    {
        /// <summary>
        ///     ポーズ時に止める音楽プレイヤーと時計を指定して生成し、入力の制御を取得する。
        /// </summary>
        /// <param name="musicPlayer"> ポーズ時に止める音楽プレイヤー。 </param>
        /// <param name="playClock"> ポーズ中とカウントダウン中に止める戦闘の時計。 </param>
        /// <param name="musicSyncController"> カウントダウン中に同期を止める音楽同期コントローラー。nullならカウントダウンしない。 </param>
        public BattlePauseModule(
            MusicPlayer musicPlayer,
            PausableClock playClock,
            MusicSyncController musicSyncController)
        {
            InputComposition inputComposition = ServiceLocator.GetInstance<InputComposition>();
            if (inputComposition == null || inputComposition.GetInputMapController == null)
            {
                Debug.LogError($"[{nameof(BattlePauseModule)}] InputComposition の取得に失敗しました。");
            }
            _inputMapController = inputComposition?.GetInputMapController;
            _musicPlayer = musicPlayer;
            _playClock = playClock;
            _musicSyncController = musicSyncController;
            _holdSubscription = _musicSyncController?.IsResumeHolding.Subscribe(HandleResumeHoldingChanged);
        }

        /// <inheritdoc/>
        public event Action OnResumeCountdownCompleted;

        /// <summary>
        ///     音楽同期の購読を解除する。
        /// </summary>
        public void Dispose()
        {
            _holdSubscription?.Dispose();
            _holdSubscription = null;
        }

        /// <inheritdoc/>
        public bool TryPause()
        {
            if (_isPaused)
            {
                return false;
            }
            // UIと既存Optionを残し、決定・取消と同じボタンの攻撃・回避入力を停止する。
            _inputMapController?.EnableOnly(InputMapNames.Common);
            _musicPlayer.PauseBGM();
            Time.timeScale = 0f;
            _playClock?.Stop(Time.unscaledTime);
            _isPaused = true;
            return true;
        }

        /// <inheritdoc/>
        public void Resume()
        {
            if (!_isPaused)
            {
                return;
            }

            if (_isCountingDown)
            {
                // 先に状態を戻し、同期再開の通知で二重に再開しないようにする。
                _isCountingDown = false;
                _musicSyncController?.CancelResumeHold();
                ResumeGameplay();
                return;
            }

            _musicPlayer.ResumeBGM();
            ResumeGameplay();
        }

        /// <inheritdoc/>
        public bool TryBeginResumeCountdown()
        {
            if (!_isPaused || _isCountingDown || _musicSyncController == null)
            {
                return false;
            }

            // 予約済みの行動やゲージを進めないよう、BGMを鳴らす前に同期を止める。
            if (!_musicSyncController.TryBeginResumeHold())
            {
                return false;
            }

            _isCountingDown = true;
            _musicPlayer.ResumeBGM();
            return true;
        }

        /// <inheritdoc/>
        public void CancelResumeCountdown()
        {
            if (!_isCountingDown)
            {
                return;
            }

            _isCountingDown = false;
            _musicSyncController?.CancelResumeHold();
            _musicPlayer.PauseBGM();
        }

        private bool _isPaused;
        private bool _isCountingDown;
        private readonly UnityInputMapController _inputMapController;
        private readonly MusicPlayer _musicPlayer;
        private readonly PausableClock _playClock;
        private readonly MusicSyncController _musicSyncController;
        private IDisposable _holdSubscription;

        /// <summary>
        ///     同期の停止が終わった時に、カウントダウン中なら戦闘を再開する。
        /// </summary>
        /// <param name="isHolding"> 同期を止めている間はtrue。 </param>
        private void HandleResumeHoldingChanged(bool isHolding)
        {
            if (isHolding || !_isCountingDown)
            {
                return;
            }

            _isCountingDown = false;
            ResumeGameplay();
            OnResumeCountdownCompleted?.Invoke();
        }

        /// <summary>
        ///     入力・時間・時計を戦闘中の状態へ戻す。BGMは呼び出し側で再開する。
        /// </summary>
        private void ResumeGameplay()
        {
            // シナリオ終了経路もExitScenarioInputModeの後にここへ到達する。
            _inputMapController?.EnableCommonWith(InputMapNames.InGame);
            Time.timeScale = 1f;
            _playClock?.Start(Time.unscaledTime);
            _isPaused = false;
        }
    }
}
