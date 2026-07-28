using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Playables;

namespace KillChord.Runtime.View.InGame.Sequence
{
    /// <summary>
    ///     ステージの開始と終了演出を再生するViewクラス。
    ///     現在はTimeline専用のクラス。
    /// </summary>
    public class StageSequenceView : MonoBehaviour
    {
        /// <summary>
        ///     ステージ開始演出を再生します。
        /// </summary>
        /// <param name="onCompleted"> 演出の再生が完了した際に呼び出されるコールバック。 </param>
        public void PlayStageStart(Action onCompleted)
        {
            CancelStageStart();

            if (_stageStartDirector == null)
            {
                Debug.LogWarning("PlayableDirectorが設定されていません。");

                onCompleted?.Invoke();
                return;
            }

            _onStageStartCompleted = onCompleted;

            _stageStartDirector.stopped += HandleStageStartStopped;
            Begin(_stageStartDirector);
        }

        /// <summary>
        ///     再生中のステージ開始演出をキャンセルします。
        /// </summary>
        public void CancelStageStart()
        {
            _onStageStartCompleted = null;

            if (_stageStartDirector == null)
            {
                return;
            }

            _stageStartDirector.stopped -= HandleStageStartStopped;

            if (_stageStartDirector.state == PlayState.Playing)
            {
                _stageStartDirector.Stop();
            }
        }

        /// <summary>
        ///     ステージクリア演出を再生します。
        /// </summary>
        /// <param name="cancellationToken"> キャンセル用のトークン。 </param>
        /// <returns> 演出の再生が完了するまで待機するAwaitable。 </returns>
        public async Awaitable PlayStageClearAsync(CancellationToken cancellationToken)
        {
            await PlayAsync(_stageClearDirector, cancellationToken);
        }

        /// <summary>
        ///     ゲームオーバー演出を再生します。
        /// </summary>
        /// <param name="cancellationToken"> キャンセル用のトークン。 </param>
        /// <returns> 演出の再生が完了するまで待機するAwaitable。 </returns>
        public async Awaitable PlayGameOverAsync(CancellationToken cancellationToken)
        {
            await PlayAsync(_gameOverDirector, cancellationToken);
        }

        [SerializeField, Tooltip("ステージ開始演出のPlayableDirector")]
        private PlayableDirector _stageStartDirector;

        [SerializeField, Tooltip("ステージクリア演出のPlayableDirector")]
        private PlayableDirector _stageClearDirector;

        [SerializeField, Tooltip("ゲームオーバー時のPlayableDirector")]
        private PlayableDirector _gameOverDirector;

        private Action _onStageStartCompleted;

        private void OnDestroy()
        {
            CancelStageStart();
        }

        /// <summary>
        ///     ステージ開始Timelineの停止イベントを処理します。
        /// </summary>
        /// <param name="director"> 停止したPlayableDirector。 </param>
        private void HandleStageStartStopped(PlayableDirector director)
        {
            director.stopped -= HandleStageStartStopped;

            Action onCompleted = _onStageStartCompleted;

            _onStageStartCompleted = null;
            onCompleted?.Invoke();
        }

        /// <summary>
        ///     指定されたPlayableDirectorの演出を即時再生します。
        /// </summary>
        /// <param name="director"> 再生するPlayableDirector。 </param>
        private static void Begin(PlayableDirector director)
        {
            director.time = 0;
            director.Evaluate();
            director.Play();
        }

        /// <summary>
        ///     指定されたPlayableDirectorの演出を再生し、完了するまで待機します。
        /// </summary>
        /// <param name="director"> 再生するPlayableDirector。 </param>
        /// <param name="cancellationToken"> キャンセル用のトークン。 </param>
        /// <returns> 演出の再生が完了するまで待機するAwaitable。 </returns>
        private static async Awaitable PlayAsync(
            PlayableDirector director,
            CancellationToken cancellationToken)
        {
            if (director == null)
            {
                Debug.LogWarning("PlayableDirectorが設定されていません。");
                return;
            }

            Begin(director);

            try
            {
                // 再生が終了するまで待機。
                while (director.state == PlayState.Playing)
                {
                    await Awaitable.NextFrameAsync(cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // キャンセルされた場合は演出も停止。
                director.Stop();
                throw;
            }
        }
    }
}
