using CriWare;
using KillChord.Runtime.Adaptor.Persistent.Music;
using KillChord.Runtime.Utility.Diagnostics;
using KillChord.Runtime.View.InGame.Music;
using R3;
using System;
using UnityEngine;

namespace KillChord.Runtime.View.Persistent.Music
{
    /// <summary>
    ///     音楽再生の実装を行うViewクラス。
    ///     BGMは音声同期タイマー付きのプレーヤーで鳴らし、実際に鳴った位置を曲の再生位置として返す。
    /// </summary>
    [RequireComponent(typeof(CriAtomSource)), DefaultExecutionOrder(-1000)]
    [AddComponentMenu("KillChord/View/Music/MusicPlayer")]
    public class MusicPlayer : MonoBehaviour, IVolumeManager, IBgmCuePlayer, IBgmSelectorPlayer
    {
        /// <summary> 音楽用ビューモデル。 </summary>
        public MusicViewModel MusicVM => _musicVm;

        /// <summary> 再生中の曲のループの長さ（秒）。分からない場合は0。 </summary>
        public double LoopLengthSeconds => _loopLengthSeconds;

        /// <summary>
        ///     ビューモデルをバインドする。
        /// </summary>
        /// <param name="musicViewModel"> 音楽用ビューモデル。 </param>
        public void Bind(MusicViewModel musicViewModel)
        {
            _musicVm = musicViewModel;
            musicViewModel.CueName.Subscribe(ChangeBgm).RegisterTo(destroyCancellationToken);
        }

        /// <summary>
        ///     初期化処理を行う。
        ///     CriAtomSourceはキューシート名と元の音量の設定に使い、再生は音声同期タイマー付きのプレーヤーで行う。
        /// </summary>
        public void Initialize()
        {
            _cri = GetComponent<CriAtomSource>();
            if (_player == null)
            {
                CriAtomPlugin.InitializeLibrary();
                _player = new CriAtomExPlayer(true);
                _player.SetVoicePriority(VOICE_PRIORITY);
            }

            CaptureBaseVolume();
            ApplyVolume();
        }

        /// <summary>
        ///     再生するBGM CueをViewModelへ設定する。
        /// </summary>
        /// <param name="cueName"> 再生するCue名。空の場合は停止する。 </param>
        public void SetCue(string cueName)
        {
            _musicVm?.UpdateMusicCue(cueName);
        }

        /// <summary>
        ///     BGM全体音量の比率を適用する。
        /// </summary>
        /// <param name="volumeRatio"> 0から1の音量比率。 </param>
        public void SetVolume(float volumeRatio)
        {
            _volumeRatio = volumeRatio;
            ApplyVolume();
        }

        /// <summary>
        ///     保存済み音量とは独立したBGM演出用の音量倍率を設定します。
        /// </summary>
        /// <param name="volumeRatio"> 演出用の0から1の音量倍率です。 </param>
        public void SetPresentationVolume(float volumeRatio)
        {
            _presentationVolumeRatio = Mathf.Clamp01(volumeRatio);
            ApplyVolume();
        }

        /// <summary>
        ///     現在の音量の倍率を返す。
        /// </summary>
        public float GetVolume()
        {
            return _volumeRatio;
        }

        /// <summary>
        ///     再生中BGMのセレクターラベルを設定する。
        ///     実際の音の切り替えはCRI側の遷移の設定に従って反映される。
        /// </summary>
        /// <param name="selectorName"> セレクター名。 </param>
        /// <param name="labelName"> 設定するセレクターラベル名。 </param>
        public void SetSelectorLabel(string selectorName, string labelName)
        {
            if (_player == null || string.IsNullOrEmpty(selectorName) || string.IsNullOrEmpty(labelName))
            {
                return;
            }

            _player.SetSelectorLabel(selectorName, labelName);

            // 再生中のプレイバックへ反映する。未再生時は設定値が次の再生に引き継がれる。
            if (_isPlaying)
            {
                _player.Update(_playback);
            }
        }

        /// <summary>
        ///     曲の頭からの通しの再生位置を取得する。ループしても巻き戻らない。
        ///     実際に鳴った位置に合わせた時刻を使い、取れない場合は再生開始からの経過時間で代える。
        /// </summary>
        /// <param name="songPositionSeconds"> 曲の再生位置（秒）。 </param>
        /// <returns> BGMを再生している場合はtrue。 </returns>
        public bool TryGetSongPosition(out double songPositionSeconds)
        {
            songPositionSeconds = 0d;
            if (!_isPlaying || _playback.GetStatus() == CriAtomExPlayback.Status.Removed)
            {
                return false;
            }

            TryResolveLoopLength();

            long elapsedMilliseconds = _playback.GetTimeSyncedWithAudio();
            if (elapsedMilliseconds < 0L)
            {
                // 発音前や同期タイマーが使えないときは、経過時間で代える。準備中は0になる。
                elapsedMilliseconds = Math.Max(0L, _playback.GetTime());
            }

            // 同じ再生の中では、時刻を戻さない。
            double songPosition = _startSongPositionSeconds + elapsedMilliseconds / MILLISECONDS_PER_SECOND;
            _lastSongPositionSeconds = Math.Max(_lastSongPositionSeconds, songPosition);
            songPositionSeconds = _lastSongPositionSeconds;
            return true;
        }

        /// <summary>
        ///     BGMを指定した曲の再生位置から鳴らし直す。ポーズ解除のカウントダウンで使う。
        /// </summary>
        /// <param name="songPositionSeconds"> 曲の頭からの通しの再生位置（秒）。 </param>
        public void RestartBGMAt(double songPositionSeconds)
        {
            if (_player == null || string.IsNullOrEmpty(_currentCueName))
            {
                return;
            }

            StopPlayback();
            StartPlayback(Math.Max(0d, songPositionSeconds));
        }

        /// <summary>
        ///     BGM再生、及び再生時間を一時停止する。
        /// </summary>
        public void PauseBGM()
        {
            if (_isPlaying)
            {
                _playback.Pause();
            }
        }

        /// <summary>
        ///     BGM再生、及び再生時間の進行を再開する。
        /// </summary>
        public void ResumeBGM()
        {
            if (_isPlaying)
            {
                _playback.Resume(CriAtomEx.ResumeMode.PausedPlayback);
            }
        }

        private const double MILLISECONDS_PER_SECOND = 1000d;
        private const double MICROSECONDS_PER_SECOND = 1000000d;
        private const int VOICE_PRIORITY = 255;

        private CriAtomSource _cri;
        private CriAtomExPlayer _player;
        private CriAtomExPlayback _playback;
        private MusicViewModel _musicVm;
        private string _currentCueName = string.Empty;
        private bool _isPlaying;
        private double _loopLengthSeconds;
        private bool _isLoopResolved;
        private double _startSongPositionSeconds;
        private double _lastSongPositionSeconds;
        private float _baseVolume = 1f;
        private float _volumeRatio = 1f;
        private float _presentationVolumeRatio = 1f;
        private bool _baseVolumeCaptured;

        /// <summary>
        ///     プレーヤーを破棄する。
        /// </summary>
        private void OnDestroy()
        {
            if (_player == null)
            {
                return;
            }

            StopPlayback();
            _player.Dispose();
            _player = null;
            CriAtomPlugin.FinalizeLibrary();
        }

        /// <summary>
        ///     最新の音量設定に演出用倍率を掛けてBGMへ反映します。
        /// </summary>
        private void ApplyVolume()
        {
            if (_player == null)
            {
                return;
            }

            _player.SetVolume(_baseVolume * _volumeRatio * _presentationVolumeRatio);
            if (_isPlaying)
            {
                _player.Update(_playback);
            }
        }

        /// <summary>
        ///     BGMを変更して再生する。
        /// </summary>
        /// <param name="cueName"> 新しいキュー名。 </param>
        private void ChangeBgm(string cueName)
        {
            if (_player == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(cueName))
            {
                StopBgm();
                DevLog.Log("BGMの再生を停止します。");
                return;
            }

            if (cueName == _currentCueName)
            {
                DevLog.Log("cueNameが元と同じです。");
                return;
            }

            StopBgm();

            // キューシート名が空のときは、CRIが読み込み済みのキューシートからキュー名で探す。
            CriAtomExAcb acb = string.IsNullOrEmpty(_cri.cueSheet) ? null : CriAtom.GetAcb(_cri.cueSheet);
            _player.SetCue(acb, cueName);
            _currentCueName = cueName;
            _loopLengthSeconds = 0d;
            _isLoopResolved = false;
            StartPlayback(0d);
        }

        /// <summary>
        ///     BGMの再生を停止する。
        /// </summary>
        private void StopBgm()
        {
            StopPlayback();
            _currentCueName = string.Empty;
            _loopLengthSeconds = 0d;
            _isLoopResolved = false;
        }

        /// <summary>
        ///     設定済みのキューを、指定した曲の再生位置から再生する。
        /// </summary>
        /// <param name="songPositionSeconds"> 曲の頭からの通しの再生位置（秒）。 </param>
        private void StartPlayback(double songPositionSeconds)
        {
            // ループする曲は、ループの中の位置へ折り返してから開始位置にする。
            double positionInLoop = _loopLengthSeconds > 0d
                ? songPositionSeconds % _loopLengthSeconds
                : songPositionSeconds;
            _player.SetStartTimeMicro((long)Math.Round(positionInLoop * MICROSECONDS_PER_SECOND));
            _playback = _player.Start();
            _player.SetStartTimeMicro(0L);

            _startSongPositionSeconds = songPositionSeconds;
            _lastSongPositionSeconds = songPositionSeconds;
            _isPlaying = true;
        }

        /// <summary>
        ///     再生中のプレイバックを止める。
        /// </summary>
        private void StopPlayback()
        {
            if (!_isPlaying)
            {
                return;
            }

            _playback.Stop(true);
            _isPlaying = false;
        }

        /// <summary>
        ///     再生中の波形のループ情報から、ループの長さを求める。発音前は取れないので、取れるまで毎回試す。
        ///     頭からループする波形だけを対象にし、途中からループする波形はループを考えずに同期する。
        /// </summary>
        private void TryResolveLoopLength()
        {
            if (_isLoopResolved || !_playback.GetFormatInfo(out CriAtomEx.FormatInfo info))
            {
                return;
            }

            _isLoopResolved = true;
            if (info.samplingRate <= 0 || info.loopLength <= 0L)
            {
                return;
            }

            if (info.loopOffset != 0L)
            {
                Debug.LogWarning($"[{nameof(MusicPlayer)}] BGM「{_currentCueName}」は途中からループするため、ループを考えずに同期します。", this);
                return;
            }

            _loopLengthSeconds = (double)info.loopLength / info.samplingRate;
        }

        /// <summary>
        ///     CriAtomSourceに設定されている元の音量を保持します。
        /// </summary>
        private void CaptureBaseVolume()
        {
            if (_baseVolumeCaptured)
            {
                return;
            }

            _baseVolume = _cri.volume;
            _baseVolumeCaptured = true;
        }
    }
}
