using CriWare;
using KillChord.Runtime.View.InGame.Music;
using R3;
using UnityEngine;

namespace KillChord.Runtime.View.Persistent.Music
{
    /// <summary>
    ///     音楽再生の実装を行うViewクラス。
    /// </summary>
    [RequireComponent(typeof(CriAtomSource)), DefaultExecutionOrder(-1000)]
    public class MusicPlayer : MonoBehaviour
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

        private const double MILLISECONDS_PER_SECOND = 1000d;

        private CriAtomSource _cri;
        private CriAtomExPlayback _playback;
        private MusicViewModel _musicVm;

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
        }

        /// <summary>
        ///     BGMの再生を停止する。
        /// </summary>
        private void StopBgm()
        {
            _playback.Stop();
            _cri.cueName = string.Empty;
        }
    }
}