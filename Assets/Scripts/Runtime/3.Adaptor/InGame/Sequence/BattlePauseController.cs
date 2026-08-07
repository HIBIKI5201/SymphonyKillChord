using System;

namespace KillChord.Runtime.Adaptor.InGame.Sequence
{
    /// <summary>
    ///     戦闘ポーズのコントローラー。
    /// </summary>
    public class BattlePauseController
    {
        public BattlePauseController(IBattlePauseModule module)
        {
            _module = module;
            _isPaused = false;
        }

        /// <summary>
        ///     ポーズ状態
        /// </summary>
        public bool IsPaused => _isPaused;

        /// <summary> ポーズした時に発火するイベント </summary>
        public event Action OnPaused;

        /// <summary> ポーズが解除された時に発火するイベント </summary>
        public event Action OnResumed;

        /// <summary> シナリオ開始時に発火するイベント </summary>
        public event Action OnScenarioPauseStarted;

        /// <summary> シナリオ終了時に発火するイベント </summary>
        public event Action OnScenarioPauseEnded;

        /// <summary>
        ///     ポーズ状態を切り替える。
        /// </summary>
        public void Toggle()
        {
            if (_isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

        /// <summary>
        ///     ポーズを実行する。
        /// </summary>
        /// <returns></returns>
        public bool Pause()
        {
            if(_isPaused || _module == null)
            {
                return false;
            }
            if (!_module.TryPause())
            {
                return false;
            }
            OnPaused?.Invoke();
            _isPaused = true;
            return true;
        }

        /// <summary>
        ///     ポーズを解除する。
        /// </summary>
        /// <returns></returns>
        public bool Resume()
        {
            if (!_isPaused || _module == null)
            {
                return false;
            }
            _module.Resume();
            OnResumed?.Invoke();
            _isPaused = false;
            return true;
        }

        /// <summary>
        ///     シナリオによるポーズを行う。
        /// </summary>
        /// <returns></returns>
        public bool BeginScenarioPause()
        {
            if (_isScenarioPauseActive)
            {
                return false;
            }

            _wasPausedBeforeScenario = IsPaused;
            if (!_wasPausedBeforeScenario && !Pause())
            {
                return false;
            }

            _isScenarioPauseActive = true;
            OnScenarioPauseStarted?.Invoke();
            return true;
        }

        /// <summary>
        ///     シナリオによるポーズを解除する。
        /// </summary>
        /// <returns></returns>
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
        private readonly IBattlePauseModule _module;
    }
}
