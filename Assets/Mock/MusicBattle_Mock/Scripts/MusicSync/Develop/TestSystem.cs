using CriWare;
using UnityEngine;

namespace Mock.MusicBattle.MusicSync
{
    public class TestSystem : MonoBehaviour
    {
        [Header("音楽同期システム初期化パラメータ")]
        [SerializeField] private MusicSyncManager _musicSyncManager;
        [SerializeField] private CriAtomSource _source;
        [SerializeField] private double _bpm;
        [SerializeField] private double _bgmProperTime;
        [SerializeField] private long _startOffset;

        void Start()
        {
            _musicSyncManager.Init(_source, _bpm, _bgmProperTime, _startOffset);
        }
    }
}
