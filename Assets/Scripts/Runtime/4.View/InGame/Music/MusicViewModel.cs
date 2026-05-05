using KillChord.Runtime.Adaptor.InGame.Music;
using R3;

namespace KillChord.Runtime.View.InGame.Music
{
    /// <summary>
    ///     音楽再生の状態を管理するViewModelクラス。
    /// </summary>
    public class MusicViewModel : IMusicViewModel
    {
        public ReadOnlyReactiveProperty<string> CueName => _cueName;

        private ReactiveProperty<string> _cueName = new(string.Empty);

        public void UpdateMusicCue(string cueName)
        {
            _cueName.Value = cueName;
        }
    }
}