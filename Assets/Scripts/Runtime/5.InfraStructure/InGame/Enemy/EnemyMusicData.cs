using UnityEngine;

[CreateAssetMenu(fileName = nameof(EnemyMusicData), menuName = "KillChord/Enemy/" + nameof(EnemyMusicData))]
public class EnemyMusicData : ScriptableObject
{
    public byte BarFlag => _barFlag;
    public long TimeSignature => _timeSignature;
    public long TargetBeat => _targetBeat;

    [SerializeField] private byte _barFlag;
    [SerializeField] private long _timeSignature;
    [SerializeField] private long _targetBeat;
}
