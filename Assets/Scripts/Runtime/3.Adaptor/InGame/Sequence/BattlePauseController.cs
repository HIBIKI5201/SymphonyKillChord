using System;

namespace KillChord.Runtime.Adaptor.InGame.Sequence
{
    /// <summary>
    ///     戦闘ポーズのコントローラー。
    /// </summary>
    public class BattlePauseController : IScenarioBattlePauseController
    {
        /// <summary>
        ///     ポーズ処理のモジュールを指定して生成する。
        /// </summary>
        public BattlePauseController(IBattlePauseModule module)
        {
            _module = module;
            _isPaused = false;
            if (_module != null)
            {
                _module.OnResumeCountdownCompleted += HandleResumeCountdownCompleted;
            }
        }

        /// <summary> ポーズした時に発火するイベント </summary>
        public event Action OnPaused;

        /// <summary> ポーズが解除された時に発火するイベント </summary>
        public event Action OnResumed;

        /// <summary> ポーズ解除のカウントダウンを始めた時に発火するイベント </summary>
        public event Action OnResumeCountdownStarted;

        /// <summary> シナリオ開始時に発火するイベント </summary>
        public event Action OnScenarioPauseStarted;

        /// <summary> シナリオ終了時に発火するイベント </summary>
        public event Action OnScenarioPauseEnded;

        /// <summary>
        ///     ポーズ状態。カウントダウン中も戦闘は止まっているためtrue。
        /// </summary>
        public bool IsPaused => _isPaused;

        /// <summary> ポーズ解除のカウントダウン中はtrue。 </summary>
        public bool IsResumeCountingDown => _isResumeCountingDown;

        /// <summary>
        ///     ポーズ状態を切り替える。ポーズ中ならカウントダウンを経て解除する。
        /// </summary>
        public void Toggle()
        {
            if (_isScenarioPauseActive || _isResumeCountingDown || (_isPaused && _isResumeLocked))
            {
                return;
            }

            if (_isPaused)
            {
                RequestResume();
            }
            else
            {
                Pause();
            }
        }

        /// <summary>
        ///     ポーズを実行する。
        /// </summary>
        /// <returns> ポーズできた場合はtrue。 </returns>
        public bool Pause()
        {
            if (_isScenarioPauseActive || _isPaused || _module == null)
            {
                return false;
            }
            if (!_module.TryPause())
            {
                return false;
            }
            _isPaused = true;
            OnPaused?.Invoke();
            return true;
        }

        /// <summary>
        ///     拍に合わせたカウントダウンを経てポーズを解除する。
        ///     カウントダウンを始められない場合は、すぐに解除する。
        /// </summary>
        /// <returns> カウントダウンを始めたか、解除した場合はtrue。 </returns>
        public bool RequestResume()
        {
            if (_isScenarioPauseActive || !_isPaused || _isResumeCountingDown || _isResumeLocked || _module == null)
            {
                return false;
            }

            if (!_module.TryBeginResumeCountdown())
            {
                return Resume();
            }

            _isResumeCountingDown = true;
            OnResumeCountdownStarted?.Invoke();
            return true;
        }

        /// <summary>
        ///     ポーズ画面の中で別のパネルを開いている間、ポーズ操作による解除を受け付けないようにする。
        ///     シーン遷移のための即時解除（<see cref="Resume"/>）は妨げない。
        /// </summary>
        /// <param name="isLocked"> 解除を受け付けない場合はtrue。 </param>
        public void SetResumeLocked(bool isLocked)
        {
            _isResumeLocked = isLocked;
        }

        /// <summary>
        ///     ポーズをすぐに解除する。カウントダウン中なら取りやめて解除する。
        /// </summary>
        /// <returns> 解除した場合はtrue。 </returns>
        public bool Resume()
        {
            if (_isScenarioPauseActive || !_isPaused || _module == null)
            {
                return false;
            }
            _isResumeCountingDown = false;
            _isResumeLocked = false;
            _isPaused = false;
            _module.Resume();
            OnResumed?.Invoke();
            return true;
        }

        /// <summary>
        ///     シナリオによるポーズを行う。
        /// </summary>
        /// <returns> シナリオ用ポーズを開始できた場合はtrue。 </returns>
        public bool BeginScenarioPause()
        {
            if (_isScenarioPauseActive)
            {
                return false;
            }

            if (_isResumeCountingDown)
            {
                // カウントダウン中は解除する途中のため、シナリオ終了後に戦闘へ戻す。
                _isResumeCountingDown = false;
                _module.CancelResumeCountdown();
                _wasPausedBeforeScenario = false;
            }
            else
            {
                _wasPausedBeforeScenario = IsPaused;
                if (!_wasPausedBeforeScenario && !Pause())
                {
                    return false;
                }
            }

            _isScenarioPauseActive = true;
            OnScenarioPauseStarted?.Invoke();
            return true;
        }

        /// <summary>
        ///     シナリオによるポーズを解除する。
        /// </summary>
        /// <returns> シナリオ用ポーズを終了できた場合はtrue。 </returns>
        public bool EndScenarioPause()
        {
            if (!_isScenarioPauseActive)
            {
                return false;
            }

            bool shouldResume = !_wasPausedBeforeScenario;
            _isScenarioPauseActive = false;
            _wasPausedBeforeScenario = false;

            if (shouldResume && !Resume())
            {
                OnScenarioPauseEnded?.Invoke();
                return false;
            }

            OnScenarioPauseEnded?.Invoke();
            return true;
        }

        private bool _wasPausedBeforeScenario;
        private bool _isScenarioPauseActive;
        private bool _isPaused;
        private bool _isResumeCountingDown;
        private bool _isResumeLocked;
        private readonly IBattlePauseModule _module;

        /// <summary>
        ///     カウントダウンが終わって戦闘を再開した時に、ポーズ状態を解除して通知する。
        /// </summary>
        private void HandleResumeCountdownCompleted()
        {
            if (!_isResumeCountingDown)
            {
                return;
            }

            _isResumeCountingDown = false;
            _isPaused = false;
            OnResumed?.Invoke();
        }
    }
}
