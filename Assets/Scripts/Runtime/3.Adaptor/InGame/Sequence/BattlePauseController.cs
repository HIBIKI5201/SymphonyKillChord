using System;

namespace KillChord.Runtime.Adaptor.InGame.Sequence
{
    /// <summary>
    ///     戦闘ポーズのコントローラー。
    /// </summary>
    public class BattlePauseController : IScenarioBattlePauseController
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
            if (_isScenarioPauseActive)
            {
                return;
            }

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
        ///     ポーズを解除する。
        /// </summary>
        /// <returns></returns>
        public bool Resume()
        {
            if (_isScenarioPauseActive || !_isPaused || _module == null)
            {
                return false;
            }
            _isPaused = false;
            _module.Resume();
            OnResumed?.Invoke();
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
