using KillChord.Runtime.View;
using R3;

namespace KillChord.Runtime.View
{
    public class MusicViewModel : IMusicViewModel
    {
        public ReadOnlyReactiveProperty<string> CueName => _cueName;

        private ReactiveProperty<string> _cueName = new(string.Empty);

        public void UpdateMusicCue(string cueName)
        {
            if (string.IsNullOrEmpty(cueName)) { return; }

            _cueName.Value = cueName;
        }
    }
}