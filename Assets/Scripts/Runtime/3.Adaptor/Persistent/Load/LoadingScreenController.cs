using Cysharp.Threading.Tasks;
using KillChord.Runtime.Application.Persistent.Load;
using KillChord.Runtime.Utility.Constant;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.Persistent.Load
{
    /// <summary>
    ///     ロード画面の表示状態と進捗を管理するコントローラー。
    /// </summary>
    public class LoadingScreenController : ILoadingSessionFactory
    {
        /// <summary>
        ///     画面の暗転を使わずに生成する。
        /// </summary>
        public LoadingScreenController() : this(0f)
        {
        }

        /// <summary>
        ///     ロード画面の前後で画面を暗転させる時間を指定して生成する。
        /// </summary>
        /// <param name="screenFadeDuration"> 暗転にかける時間（秒）。 </param>
        public LoadingScreenController(float screenFadeDuration)
        {
            if (!float.IsFinite(screenFadeDuration) || screenFadeDuration < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(screenFadeDuration),
                    screenFadeDuration,
                    "暗転の時間は非負の有限の数でなければなりません。");
            }

            _screenFadeDuration = screenFadeDuration;
        }

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
        ///     正常終了の前に、ロード画面の上を暗転させ始めるときに通知するイベント。
        /// </summary>
        public event Action OnLoadingFinishing;

        /// <summary>
        ///     ロード画面の前後で画面を暗転させる時間（秒）。
        /// </summary>
        public float ScreenFadeDuration => _screenFadeDuration;

        /// <summary>
        ///     ロード画面が使用中かどうか。
        /// </summary>
        public bool IsLoading => _activeSession != null;

        /// <summary>
        ///     ロード画面のセッションを開始する。
        ///     reuseActiveSession が true で実行中のセッションがある場合は、それを再利用する。
        /// </summary>
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
        ///     ロード開始時の暗転が終わり、画面が覆われたことを表示側から通知する。
        ///     ロード処理はこの通知を待ってから遷移元のシーンを触る。
        /// </summary>
        public void NotifyScreenCovered()
        {
            _activeSession?.MarkScreenCovered();
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

        /// <summary>
        ///     正常終了の前の暗転開始を通知する。
        /// </summary>
        /// <param name="session"> 通知元セッション。 </param>
        /// <returns> 通知元が実行中のセッションの場合はtrue。 </returns>
        private bool NotifyFinishing(LoadingSession session)
        {
            if (_activeSession != session)
            {
                return false;
            }

            OnLoadingFinishing?.Invoke();
            return true;
        }

        /// <summary>
        ///     開始時刻からの残りの暗転時間だけ待つ。
        /// </summary>
        /// <param name="startTimestamp"> 暗転を始めた時刻（Stopwatchのタイムスタンプ）。 </param>
        /// <param name="cancellationToken"> キャンセルトークン。 </param>
        /// <returns> 暗転が終わるまでを表すタスク。 </returns>
        private async Task WaitForScreenFadeAsync(long startTimestamp, CancellationToken cancellationToken)
        {
            double elapsedTime =
                (Stopwatch.GetTimestamp() - startTimestamp) / (double)Stopwatch.Frequency;
            double remainingTime = _screenFadeDuration - elapsedTime;
            if (remainingTime <= 0d)
            {
                return;
            }

            await UniTask.Delay(
                TimeSpan.FromSeconds(remainingTime),
                DelayType.Realtime,
                cancellationToken: cancellationToken);
        }

        private readonly float _screenFadeDuration;
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
            ///     ロード画面の上を暗転させてから正常終了する。
            /// </summary>
            /// <param name="cancellationToken"> キャンセルトークン。 </param>
            /// <returns> 終了までを表すタスク。 </returns>
            public async Task CompleteAsync(CancellationToken cancellationToken)
            {
                if (_isEnded)
                {
                    return;
                }

                // 暗転の時間が無い場合や、既に別のセッションに替わっている場合はそのまま終える。
                if (_owner._screenFadeDuration > 0f && _owner.NotifyFinishing(this))
                {
                    await _owner.WaitForScreenFadeAsync(Stopwatch.GetTimestamp(), cancellationToken);
                }

                End(true);
            }

            /// <summary>
            ///     ロード開始時の暗転が終わり、画面が覆われるまで待つ。
            ///     表示側からの通知を待ち、覆われた画面を1フレーム描画してから返す。
            ///     通知が来ない場合に備え、暗転時間に余裕を足した実時間で打ち切る。
            /// </summary>
            /// <param name="cancellationToken"> キャンセルトークン。 </param>
            /// <returns> 画面が覆われるまでを表すタスク。 </returns>
            public async Task WaitUntilScreenCoveredAsync(CancellationToken cancellationToken)
            {
                if (_isEnded)
                {
                    return;
                }

                if (!_screenCoveredSource.Task.IsCompleted)
                {
                    using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    double timeout = _owner._screenFadeDuration + SCREEN_COVER_TIMEOUT_MARGIN;
                    Task timeoutTask = UniTask.Delay(
                            TimeSpan.FromSeconds(timeout),
                            DelayType.Realtime,
                            cancellationToken: timeoutSource.Token)
                        .AsTask();

                    Task finishedTask = await Task.WhenAny(_screenCoveredSource.Task, timeoutTask);
                    if (finishedTask == timeoutTask)
                    {
                        // キャンセルされた場合はここで例外として伝える。
                        await timeoutTask;
                        UnityEngine.Debug.LogWarning(
                            $"[{nameof(LoadingScreenController)}] 暗転完了の通知が {timeout} 秒以内に来なかったため、ロード処理を進めます。");
                    }
                    else
                    {
                        timeoutSource.Cancel();
                    }
                }

                // 覆われた画面が描画されてから遷移元のシーンを触る。
                await UniTask.NextFrame(cancellationToken);
            }

            /// <summary>
            ///     画面が覆われたことを記録する。
            /// </summary>
            public void MarkScreenCovered()
            {
                _screenCoveredSource.TrySetResult(true);
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

            /// <summary> 暗転完了の通知を待つ時間に、暗転時間へ足す余裕（秒）。 </summary>
            private const double SCREEN_COVER_TIMEOUT_MARGIN = 1d;

            private readonly LoadingScreenController _owner;
            private readonly TaskCompletionSource<bool> _screenCoveredSource =
                new(TaskCreationOptions.RunContinuationsAsynchronously);
            private bool _isEnded;

        }
    }
}
