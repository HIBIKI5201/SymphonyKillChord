using System;

namespace Research.SaveSystem
{
    public interface ILoadService
    {
        public void Load(Action<KillChordGameData> callback);
    }
}