using KillChord.Runtime.View;
using R3;

namespace KillChord.Runtime.View
{
    public class MusicViewModel : IMusicViewModel
    {
        public ReadOnlyReactiveProperty<string> CueName => _cueName;
        public ReadOnlyReactiveProperty<int> Bpm => _bpm;

        private ReactiveProperty<string> _cueName = new(string.Empty);
        private ReactiveProperty<int> _bpm = new(-1);

        public void UpdateMusicCue(string cueName, int bpm)
        {
            _cueName.Value = cueName;
            _bpm.Value = bpm;
        }
    }
}