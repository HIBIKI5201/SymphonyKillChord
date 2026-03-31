using KillChord.Runtime.View;
using R3;

namespace KillChord.Runtime.View.Persistent.Music
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