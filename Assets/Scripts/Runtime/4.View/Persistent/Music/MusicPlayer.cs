using CriWare;
using KillChord.Runtime.Adaptor.Persistent.Music;
using KillChord.Runtime.View.InGame.Music;
using R3;
using UnityEngine;

namespace KillChord.Runtime.View.Persistent.Music
{
    /// <summary>
    ///     音楽再生の実装を行うViewクラス。
    /// </summary>
    [RequireComponent(typeof(CriAtomSource)), DefaultExecutionOrder(-1000)]
    public class MusicPlayer : MonoBehaviour, IVolumeManager
    {
        /// <summary> 音楽用ビューモデル。 </summary>
        public MusicViewModel MusicVM => _musicVm;

        /// <summary> 現在の曲の累計再生時間を取得。 </summary>
        public double Time => _playback.time / MILLISECONDS_PER_SECOND;

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
        /// </summary>
        public void Initialize()
        {
            _cri = GetComponent<CriAtomSource>();
        }

        public void SetVolume(float volume)
        {
            _cri.volume = volume;
        }

        public float GetVolume()
        {
            return _cri.volume;
        }

        /// <summary>
        ///     再生中BGMのセレクターラベルを設定し、再生中の音声へ即時反映する。
        /// </summary>
        /// <param name="selectorName"> CRIのセレクター名。 </param>
        /// <param name="labelName"> 設定するセレクターラベル名。 </param>
        public void SetSelectorLabel(string selectorName, string labelName)
        {
            if (_cri == null || string.IsNullOrEmpty(selectorName) || string.IsNullOrEmpty(labelName))
            {
                return;
            }

            _cri.player.SetSelectorLabel(selectorName, labelName);

            if (_isPlaying)
            {
                _cri.player.Update(_playback);
            }
        }

        private const double MILLISECONDS_PER_SECOND = 1000d;

        private CriAtomSource _cri;
        private CriAtomExPlayback _playback;
        private MusicViewModel _musicVm;
        private bool _isPlaying;

        /// <summary>
        ///     BGMを変更して再生する。
        /// </summary>
        /// <param name="cueName"> 新しいキュー名。 </param>
        private void ChangeBgm(string cueName)
        {
            string currentCueName = _cri.cueName;

            if (string.IsNullOrEmpty(cueName))
            {
                StopBgm();
                Debug.Log("BGMの再生を停止します。");
                return;
            }

            if (cueName == currentCueName)
            {
                Debug.Log("cueNameが元と同じです。");
                return;
            }

            StopBgm();

            _cri.cueName = cueName;
            _playback = _cri.Play();
            _isPlaying = true;
        }

        /// <summary>
        ///     BGMの再生を停止する。
        /// </summary>
        private void StopBgm()
        {
            _playback.Stop();
            _cri.cueName = string.Empty;
            _isPlaying = false;
        }

    }
}