using KillChord.Runtime.Adaptor.Persistent.Environment;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Utility.Constant;
using R3;

namespace KillChord.Runtime.Adaptor.InGame.Music
{
    /// <summary>
    ///     音楽との同期タイミングの更新を制御するコントローラークラス。
    /// </summary>
    public class MusicSyncController
    {
        /// <summary>
        ///     新しいコントローラーを生成する。
        /// </summary>
        /// <param name="musicSyncState"> 音楽同期状態。 </param>
        /// <param name="musicSyncService"> 音楽同期サービス。 </param>
        /// <param name="environmentSettingsViewModel">
        ///     リズム判定オフセットを公開する環境設定ViewModel。
        ///     未指定の場合はオフセット0秒として扱う。
        /// </param>
        public MusicSyncController(
            MusicSyncState musicSyncState,
            IMusicSyncService musicSyncService,
            IEnvironmentSettingsViewModel environmentSettingsViewModel = null)
        {
            _musicSyncState = musicSyncState;
            _musicSyncService = musicSyncService;
            _environmentSettingsViewModel = environmentSettingsViewModel;
        }

        /// <summary> ポーズ解除のカウントダウンのために同期を止めている間はtrue。 </summary>
        public ReadOnlyReactiveProperty<bool> IsResumeHolding => _isResumeHolding;

        /// <summary> 同期の再開までに残っている拍の数。止めていないときは0。 </summary>
        public ReadOnlyReactiveProperty<int> ResumeRemainingBeatCount => _resumeRemainingBeatCount;

        /// <summary>
        ///     毎フレームの更新処理を行う。
        ///     ユーザー設定のリズム判定オフセットを再生時間へ加算してから同期状態・判定処理へ渡す。
        ///     ポーズ解除のカウントダウン中は、ゲーム側の時間がポーズした時刻へ戻るまで同期を止める。
        /// </summary>
        /// <param name="playTime"> 現在の再生時間。 </param>
        /// <param name="unscaledDeltaSeconds"> 前フレームからの実時間の経過秒数。 </param>
        public void Tick(double playTime, double unscaledDeltaSeconds = 0d)
        {
            if (_hold.IsHolding)
            {
                double holdPlayTime = ToAdjustedPlayTime(playTime);
                if (!_hold.TryRelease(holdPlayTime, unscaledDeltaSeconds))
                {
                    _resumeRemainingBeatCount.Value = _hold.CalculateRemainingBeatCount(
                        holdPlayTime,
                        _musicSyncState.BeatLength,
                        _musicSyncState.BeatOffsetSeconds);
                    return;
                }

                // 解除したフレームは、ポーズした時刻から続けて同期を進める。
                PublishHoldState();
            }

            double adjustedPlayTime = ToAdjustedPlayTime(playTime);
            _lastAdjustedPlayTime = adjustedPlayTime;
            _hasTicked = true;

            _musicSyncState.UpdatePlayTime(adjustedPlayTime);
            _musicSyncService.Update(adjustedPlayTime);
        }

        /// <summary>
        ///     BGMを先に再生してカウントダウンするために、ゲーム側の同期を止める。
        ///     BGMが1小節進む間に、ゲーム側の時間がポーズした時刻へ戻るよう換算をずらす。
        /// </summary>
        /// <returns> 同期を止めた場合はtrue。拍を求められない、またはすでに止めている場合はfalse。 </returns>
        public bool TryBeginResumeHold()
        {
            double beatLength = _musicSyncState.BeatLength;
            if (_hold.IsHolding || !_hasTicked || beatLength <= 0d)
            {
                return false;
            }

            // 小節単位でずらし、曲の小節頭とゲーム側の小節頭を揃えたままにする。
            double barLength = beatLength * MusicConstants.STANDARD_BEATS_PER_BAR;
            _hold.Begin(_lastAdjustedPlayTime, barLength, barLength * HOLD_TIMEOUT_BAR_COUNT + HOLD_TIMEOUT_MARGIN_SECONDS);
            PublishHoldState();
            return true;
        }

        /// <summary>
        ///     カウントダウンを取りやめ、加えたずれを取り消して同期の停止を解除する。
        /// </summary>
        public void CancelResumeHold()
        {
            if (!_hold.IsHolding)
            {
                return;
            }

            _hold.Cancel();
            PublishHoldState();
        }

        /// <summary>
        ///     音源の停止・切り替えに合わせて入力とゲージ基準を破棄する。
        /// </summary>
        public void ResetPlayback()
        {
            _musicSyncService.ResetPlayback();
            _hold.Reset();
            _hasTicked = false;
            PublishHoldState();
        }

        /// <summary> 実時間でこの小節数を過ぎても同期が戻らないときは、ずれを取り消して再開する。 </summary>
        private const double HOLD_TIMEOUT_BAR_COUNT = 2d;

        /// <summary> 同期の停止を打ち切るまでの時間へ加える余裕（秒）。 </summary>
        private const double HOLD_TIMEOUT_MARGIN_SECONDS = 1d;

        private readonly MusicSyncState _musicSyncState;
        private readonly IMusicSyncService _musicSyncService;
        private readonly IEnvironmentSettingsViewModel _environmentSettingsViewModel;
        private readonly MusicPlaybackHold _hold = new();
        private readonly ReactiveProperty<bool> _isResumeHolding = new(false);
        private readonly ReactiveProperty<int> _resumeRemainingBeatCount = new(0);
        private double _lastAdjustedPlayTime;
        private bool _hasTicked;

        /// <summary>
        ///     音源の再生時間を、ずれと判定オフセットを反映したゲーム側の時間へ換算する。
        /// </summary>
        /// <param name="playTime"> 音源の再生時間。 </param>
        /// <returns> ゲーム側の再生時間。 </returns>
        private double ToAdjustedPlayTime(double playTime)
        {
            double rhythmOffsetSeconds = _environmentSettingsViewModel?.RhythmOffsetSeconds.CurrentValue ?? 0d;
            return playTime - _hold.ShiftSeconds + rhythmOffsetSeconds;
        }

        /// <summary>
        ///     同期の停止状態と残りの拍数を購読者へ通知する。
        /// </summary>
        private void PublishHoldState()
        {
            if (!_hold.IsHolding)
            {
                _resumeRemainingBeatCount.Value = 0;
            }

            _isResumeHolding.Value = _hold.IsHolding;
        }
    }
}
