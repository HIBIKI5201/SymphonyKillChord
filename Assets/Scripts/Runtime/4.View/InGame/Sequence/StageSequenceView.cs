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
            if (_stageClearCamera != null) { _stageClearCamera.Release(); }
            if (_clearFadeBackground != null) { _clearFadeBackground.SetActive(true); }
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
            if (_clearFadeBackground != null) { _clearFadeBackground.SetActive(false); }
            try
            {
                await PlayAsync(_stageClearDirector, cancellationToken);
            }
            catch
            {
                if (_stageClearCamera != null) { _stageClearCamera.Release(); }
                throw;
            }
            finally
            {
                if (_letterBox != null) { _letterBox.DeactiveAspectImmediate(); }
            }
        }

        /// <summary>
        ///     クリア演出で注視するプレイヤーを設定します。
        /// </summary>
        public void InitializeClearCamera(Transform player)
        {
            if (_stageClearCamera == null || _clearFadeBackground == null || _letterBox == null)
            {
                throw new InvalidOperationException("クリア演出のカメラ・背景・黒帯を設定してください。");
            }
            _stageClearCamera.Initialize(player);
        }

        /// <summary>
        ///     ゲームオーバー演出を再生します。
        /// </summary>
        /// <param name="cancellationToken"> キャンセル用のトークン。 </param>
        /// <returns> 演出の再生が完了するまで待機するAwaitable。 </returns>
        public async Awaitable PlayGameOverAsync(CancellationToken cancellationToken)
        {
            if (_stageClearCamera != null) { _stageClearCamera.Release(); }
            if (_clearFadeBackground != null) { _clearFadeBackground.SetActive(true); }
            await PlayAsync(_gameOverDirector, cancellationToken);
        }

        [SerializeField, Tooltip("ステージ開始演出のPlayableDirector")]
        private PlayableDirector _stageStartDirector;

        [SerializeField, Tooltip("ステージクリア演出のPlayableDirector")]
        private PlayableDirector _stageClearDirector;

        [SerializeField, Tooltip("ゲームオーバー時のPlayableDirector")]
        private PlayableDirector _gameOverDirector;

        [SerializeField, Tooltip("勝利演出中だけ非表示にする暗転背景。")]
        private GameObject _clearFadeBackground;

        [SerializeField, Tooltip("クリアTimelineから制御する正面カメラ。")]
        private StageClearCameraView _stageClearCamera;

        [SerializeField, Tooltip("イントロと共用する上下の黒帯。")]
        private UI.LetterBoxAnimationGUI _letterBox;

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
