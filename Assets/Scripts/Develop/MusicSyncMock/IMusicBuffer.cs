using UnityEngine;

public interface IMusicBuffer
{
    public float CurrentBpm { get; }
    public float BeatLength { get; }
    public float CurrentBeat { get; }
}
