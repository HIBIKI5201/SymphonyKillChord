using KillChord.Runtime.Adaptor.Persistent.Environment;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Utility.Constant;
using R3;
using System;

namespace KillChord.Runtime.Adaptor.InGame.Music
{
    /// <summary>
    ///     音楽との同期タイミングの更新を制御するコントローラークラス。
    ///     曲の再生位置をゲーム側の音楽時間へ換算し、同期状態と判定処理へ渡す。
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
        ///     再生中の曲のループの長さを設定する。小節の長さは同期状態のBPMから求める。
        ///     ゲーム側の拍のグリッドは、ループの頭が来るたびに小節の頭へそろえ直す。
        /// </summary>
        /// <param name="loopLengthSeconds"> ループの長さ（秒）。0以下ならループしない曲として扱う。 </param>
        public void SetLoopLength(double loopLengthSeconds)
        {
            _timeline = new MusicLoopTimeline(loopLengthSeconds, GetBarLength());
        }

        /// <summary>
        ///     毎フレームの更新処理を行う。
        ///     曲の再生位置をゲーム側の音楽時間へ換算し、ユーザー設定のオフセットを加えてから同期状態・判定処理へ渡す。
        ///     ポーズ解除のカウントダウン中は、ゲーム側の時間が再開する時刻へ届くまで同期を止める。
        /// </summary>
        /// <param name="songPositionSeconds"> 曲の頭からの通しの再生位置（秒）。 </param>
        /// <param name="unscaledDeltaSeconds"> 前フレームからの実時間の経過秒数。 </param>
        public void Tick(double songPositionSeconds, double unscaledDeltaSeconds = 0d)
        {
            double adjustedPlayTime = ToAdjustedPlayTime(songPositionSeconds);

            if (_hold.IsHolding)
            {
                if (!_hold.TryRelease(adjustedPlayTime, unscaledDeltaSeconds))
                {
                    _resumeRemainingBeatCount.Value = _hold.CalculateRemainingBeatCount(
                        adjustedPlayTime,
                        _musicSyncState.BeatLength,
                        _musicSyncState.BeatOffsetSeconds);
                    return;
                }

                PublishHoldState();
            }

            // ループの継ぎ目や再生し直しの誤差でゲーム側の時間が戻らないよう、前回の時刻より前へは進めない。
            if (_hasTicked && adjustedPlayTime < _lastAdjustedPlayTime)
            {
                adjustedPlayTime = _lastAdjustedPlayTime;
            }

            _lastAdjustedPlayTime = adjustedPlayTime;
            _hasTicked = true;

            _musicSyncState.UpdatePlayTime(adjustedPlayTime);
            _musicSyncService.Update(adjustedPlayTime);
        }

        /// <summary>
        ///     ポーズ解除のカウントダウンのために、ゲーム側の同期を止める。
        ///     再開はポーズした時刻のままとし、その1小節前からBGMを鳴らし直す。
        /// </summary>
        /// <param name="restartSongPositionSeconds"> BGMを鳴らし直す曲の再生位置（秒）。 </param>
        /// <returns> 同期を止めた場合はtrue。拍を求められない、またはすでに止めている場合はfalse。 </returns>
        public bool TryBeginResumeHold(out double restartSongPositionSeconds)
        {
            restartSongPositionSeconds = 0d;
            double barLength = GetBarLength();
            if (_hold.IsHolding || !_hasTicked || barLength <= 0d)
            {
                return false;
            }

            double resumePlayTime = _lastAdjustedPlayTime;
            restartSongPositionSeconds = ToSongPosition(resumePlayTime - barLength);
            _hold.Begin(resumePlayTime, barLength * HOLD_TIMEOUT_BAR_COUNT + HOLD_TIMEOUT_MARGIN_SECONDS);
            PublishHoldState();
            return true;
        }

        /// <summary>
        ///     いまのゲーム側の時刻に当たる曲の再生位置を求める。
        ///     カウントダウンを取りやめたあと、カウントダウンなしで再開するときに使う。
        /// </summary>
        /// <param name="songPositionSeconds"> 曲の再生位置（秒）。 </param>
        /// <returns> 一度でも同期を進めていればtrue。 </returns>
        public bool TryGetCurrentSongPosition(out double songPositionSeconds)
        {
            songPositionSeconds = _hasTicked ? ToSongPosition(_lastAdjustedPlayTime) : 0d;
            return _hasTicked;
        }

        /// <summary>
        ///     カウントダウンを取りやめ、同期の停止を解除する。ゲーム側の時刻はポーズした時刻のまま残す。
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
            _lastAdjustedPlayTime = 0d;
            PublishHoldState();
        }

        /// <summary> 実時間でこの小節数を過ぎても同期が戻らないときは、そのまま再開する。 </summary>
        private const double HOLD_TIMEOUT_BAR_COUNT = 2d;

        /// <summary> 同期の停止を打ち切るまでの時間へ加える余裕（秒）。 </summary>
        private const double HOLD_TIMEOUT_MARGIN_SECONDS = 1d;

        private readonly MusicSyncState _musicSyncState;
        private readonly IMusicSyncService _musicSyncService;
        private readonly IEnvironmentSettingsViewModel _environmentSettingsViewModel;
        private readonly MusicPlaybackHold _hold = new();
        private readonly ReactiveProperty<bool> _isResumeHolding = new(false);
        private readonly ReactiveProperty<int> _resumeRemainingBeatCount = new(0);
        private MusicLoopTimeline _timeline;
        private double _lastAdjustedPlayTime;
        private bool _hasTicked;

        /// <summary>
        ///     曲の再生位置を、ループと判定オフセットを反映したゲーム側の時間へ換算する。
        /// </summary>
        /// <param name="songPositionSeconds"> 曲の頭からの通しの再生位置（秒）。 </param>
        /// <returns> ゲーム側の音楽時間。 </returns>
        private double ToAdjustedPlayTime(double songPositionSeconds)
        {
            return _timeline.ToGameTime(songPositionSeconds) + GetRhythmOffsetSeconds();
        }

        /// <summary>
        ///     ゲーム側の時間を、曲の再生位置へ換算する。曲の頭より前は頭として扱う。
        /// </summary>
        /// <param name="adjustedPlayTime"> ゲーム側の音楽時間。 </param>
        /// <returns> 曲の頭からの通しの再生位置（秒）。 </returns>
        private double ToSongPosition(double adjustedPlayTime)
        {
            double songPosition = _timeline.ToSongPosition(adjustedPlayTime - GetRhythmOffsetSeconds());
            return Math.Max(0d, songPosition);
        }

        /// <summary>
        ///     ユーザー設定のリズム判定オフセットを取得する。BGMとゲーム全体の時間の差として使う。
        /// </summary>
        /// <returns> オフセット（秒）。 </returns>
        private double GetRhythmOffsetSeconds()
        {
            return _environmentSettingsViewModel?.RhythmOffsetSeconds.CurrentValue ?? 0d;
        }

        /// <summary>
        ///     同期状態のBPMから1小節の長さを求める。
        /// </summary>
        /// <returns> 1小節の長さ（秒）。BPMが未設定なら0。 </returns>
        private double GetBarLength()
        {
            return _musicSyncState.BeatLength * MusicConstants.STANDARD_BEATS_PER_BAR;
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
