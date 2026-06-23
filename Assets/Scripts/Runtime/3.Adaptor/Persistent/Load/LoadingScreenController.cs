using KillChord.Runtime.Application.Persistent.Load;
using KillChord.Runtime.Utility.Constant;
using System;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.Persistent.Load
{
    /// <summary>
    ///     ロード画面の表示状態と進捗を管理するコントローラー。
    /// </summary>
    public class LoadingScreenController : ILoadingSessionFactory
    {
        /// <summary>
        ///     ロード画面を表示するときに通知するイベント。
        /// </summary>
        public event Action LoadingStarted;

        /// <summary>
        ///     ロード進捗が変更されたときに通知するイベント。
        /// </summary>
        public event Action<float> LoadingProgressChanged;

        /// <summary>
        ///     ロード処理が終了したときに通知するイベント。
        /// </summary>
        public event Action<bool> LoadingCompleted;

        /// <summary>
        ///     ロード画面が使用中かどうか。
        /// </summary>
        public bool IsLoading => _activeSession != null;

        public ILoadingSession Begin(bool reuseActiveSession = false)
        {
            if(reuseActiveSession)
            {
                if(_activeSession == null)
                {
                    throw new InvalidOperationException(
                        "再利用するアクティブなセッションが存在しません。");
                }

                return _activeSession;
            }

            if(_activeSession != null)
            {
                throw new InvalidOperationException(
                    "既にアクティブなセッションが存在します。");
            }

            _activeSession = new LoadingSession(this);
            _currentProgress = 0f;

            LoadingStarted?.Invoke();
            LoadingProgressChanged?.Invoke(_currentProgress);

            return _activeSession;
        }

        /// <summary>
        ///     実行中のロードセッションを失敗として終了する。
        /// </summary>
        public void FailActiveSession()
        {
            _activeSession?.Fail();
        }

        /// <summary>
        ///     セッションから通知された進捗を反映する。
        /// </summary>
        /// <param name="session"> 通知元セッション。 </param>
        /// <param name="progress"> 0から1の進捗。 </param>
        private void Report(
            LoadingSession session,
            float progress)
        {
            if (_activeSession != session)
            {
                return;
            }

            float clampedProgress = Mathf.Clamp01(progress);

            // 非同期通知の順番が前後しても進捗を戻さない。
            if (clampedProgress < _currentProgress)
            {
                return;
            }

            _currentProgress = clampedProgress;
            LoadingProgressChanged?.Invoke(_currentProgress);
        }

        /// <summary>
        ///     ロードセッションを終了する。
        /// </summary>
        /// <param name="session"> 終了するセッション。 </param>
        /// <param name="success"> 正常終了した場合はtrue。 </param>
        private void End(
            LoadingSession session,
            bool success)
        {
            if (_activeSession != session)
            {
                return;
            }

            if (success)
            {
                _currentProgress = 1f;
                LoadingProgressChanged?.Invoke(1f);
            }

            _activeSession = null;
            LoadingCompleted?.Invoke(success);
        }

        private LoadingSession _activeSession;
        private float _currentProgress;

        /// <summary>
        ///     一回分のロード画面表示期間を管理するクラス。
        /// </summary>
        private class LoadingSession : ILoadingSession
        {
            /// <summary>
            ///     所有元を指定して生成する。
            /// </summary>
            /// <param name="owner"> 所有元コントローラー。 </param>
            public LoadingSession(
                LoadingScreenController owner)
            {
                _owner = owner
                    ?? throw new ArgumentNullException(nameof(owner));
            }

            /// <summary>
            ///     ロードセッションが終了済みかどうか。
            /// </summary>
            public bool IsEnded => _isEnded;

            /// <summary>
            ///     ロード進捗を通知する。
            /// </summary>
            /// <param name="value"> 0から1の進捗。 </param>
            public void Report(float value)
            {
                if (_isEnded)
                {
                    return;
                }

                _owner.Report(this, value);
            }

            /// <summary>
            ///     ロード処理を正常終了する。
            /// </summary>
            public void Complete()
            {
                End(true);
            }

            /// <summary>
            ///     ロード処理を失敗として終了する。
            /// </summary>
            public void Fail()
            {
                End(false);
            }

            /// <summary>
            ///     ロードセッションを終了する。
            /// </summary>
            /// <param name="success">正常終了した場合はtrue。</param>
            private void End(bool success)
            {
                if (_isEnded)
                {
                    return;
                }

                _isEnded = true;
                _owner.End(this, success);
            }

            private readonly LoadingScreenController _owner;
            private bool _isEnded;

        }
    }
}
