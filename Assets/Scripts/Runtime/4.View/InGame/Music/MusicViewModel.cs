using KillChord.Runtime.Adaptor.InGame.Music;
using R3;

namespace KillChord.Runtime.View.InGame.Music
{
    /// <summary>
    ///     音楽再生の状態を管理するViewModelクラス。
    /// </summary>
    public class MusicViewModel : IMusicViewModel
    {
        /// <summary> 再生中のキュー名。 </summary>
        public ReadOnlyReactiveProperty<string> CueName => _cueName;

        private ReactiveProperty<string> _cueName = new(string.Empty);

        /// <summary>
        ///     再生する音楽のキューを更新する。
        /// </summary>
        /// <param name="cueName"> 新しいキュー名。 </param>
        public void UpdateMusicCue(string cueName)
        {
            _cueName.Value = cueName;
        }
    }
}