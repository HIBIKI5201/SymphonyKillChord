using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Playables;

namespace KillChord.Demo.End
{
    /// <summary>
    ///     体験版終了時のデモムービーTimelineを再生するViewです。
    /// </summary>
    public sealed class DemoEndMovieView : MonoBehaviour
    {
        /// <summary>
        ///     デモムービーを先頭から再生し、停止するまで待機します。
        /// </summary>
        /// <param name="cancellationToken"> キャンセル用のトークンです。 </param>
        /// <returns> 再生完了まで待機するAwaitableです。 </returns>
        public async Awaitable PlayAsync(CancellationToken cancellationToken)
        {
            if (_director == null)
            {
                Debug.LogWarning(
                    $"[{nameof(DemoEndMovieView)}] {nameof(_director)} が設定されていません。",
                    this);
                return;
            }

            // 途中のフレームから始まらないよう、先頭で一度評価してから再生する。
            _director.time = 0.0;
            _director.Evaluate();
            _director.Play();

            try
            {
                while (_director.state == PlayState.Playing)
                {
                    await Awaitable.NextFrameAsync(cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // キャンセルされた場合は演出も止める。
                _director.Stop();
                throw;
            }
        }

        /// <summary>
        ///     再生中のデモムービーを停止します。
        /// </summary>
        public void Stop()
        {
            if (_director == null || _director.state != PlayState.Playing)
            {
                return;
            }

            _director.Stop();
        }

        [SerializeField, Tooltip("デモムービーTimelineのPlayableDirectorです。")]
        private PlayableDirector _director;

        /// <summary>
        ///     破棄時に再生中の演出を停止します。
        /// </summary>
        private void OnDestroy()
        {
            Stop();
        }
    }
}
