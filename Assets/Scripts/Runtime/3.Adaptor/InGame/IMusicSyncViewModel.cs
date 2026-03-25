namespace KillChord.Runtime.Adaptor
{
    public interface IMusicSyncViewModel
    {
        void SetBpm(int bpm);
        int GetNearestSignature(double seconds);
    }
}