using CriWare;
using System;
using System.Threading;
using UnityEngine;

namespace Mock.MusicSyncMock
{
    /// <summary>
    /// 音楽同期システムの全体を管理し、外部へのインターフェースを持つクラス。
    /// </summary>
    public class MusicSyncManager : MonoBehaviour
    {
        [SerializeField] private float[] _timeSignatures = { 1f, 2f, 3f, 4f, 6f, 8f, 12f, 16f };
        [SerializeField] CriMusicBuffer _musicBuffer;
        [SerializeField] CriMusicPlayer _musicPlayer;
        [SerializeField] MusicInputHandler _inputHandler;
        [SerializeField] MusicActionHandler _actionHandler;

        [Header("デバッグ用")]
        [SerializeField] private CriAtomSource _source;
        [SerializeField] private double _bpm;
        [SerializeField] private double _bgmProperTime;
        [SerializeField] private long _startOffset;

        #region ライフサイクル
        private void Start()
        {
            // デバッグ用初期化。多分本番ではシステムからInit、PlayBgmを呼ぶ？
            Init(_source, _bpm, _bgmProperTime, _startOffset);
            PlayBgm();
        }
        #endregion

        #region Publicメソッド
        /// <summary>
        /// 初期化処理を行う。
        /// </summary>
        /// <param name="source">再生するBGM</param>
        /// <param name="bpm">BGMのBPM</param>
        /// <param name="bgmProperTime">BGM固有の拍子</param>
        /// <param name="startOffset">最初小節の開始時間（ミリ秒）</param>
        public void Init(CriAtomSource source, double bpm, double bgmProperTime, long startOffset)
        {
            _source = source;
            _bpm = bpm;
            _bgmProperTime = bgmProperTime;
            _startOffset = startOffset;

            _musicPlayer.Init(source);
            _musicBuffer.Init(source, bpm, bgmProperTime, startOffset);
            _inputHandler.Init(_timeSignatures);
        }

        /// <summary>
        /// 入力によって成り立つ拍子を取得する。
        /// </summary>
        /// <returns></returns>
        public float GetInputTimeSignature()
        {
            return _inputHandler.GetInputTimeSignature();
        }

        /// <summary>
        /// アクション予約を行う。
        /// </summary>
        /// <param name="barTimingInfo">小節タイミング情報</param>
        /// <param name="action">実行アクション</param>
        /// <param name="token">キャンセルトークン</param>
        public void RegisterAction(BarTimingInfo barTimingInfo, Action action, CancellationToken token)
        {
            _actionHandler.RegisterAction(barTimingInfo, action, token);
        }
        #endregion

        #region Privateメソッド

        /// <summary>
        /// BGMを再生する。
        /// </summary>
        private void PlayBgm()
        {
            _musicPlayer.Play();
        }
        #endregion
    }
    /// <summary>
    /// 予約内容
    /// </summary>
    public class ScheduledAction
    {
        /// <summary>発火タイミング</summary>
        public double ExecuteBeat { get; private set; }
        /// <summary>実行アクション</summary>
        public Action Action { get; private set; }
        /// <summary>取消トークン</summary>
        public CancellationToken cancelToken;
        public ScheduledAction(double executeBeat, Action action, CancellationToken token)
        {
            ExecuteBeat = executeBeat;
            Action = action;
            cancelToken = token;
        }
    }
    
    /// <summary>
    /// 小節タイミング
    /// </summary>
    public readonly struct BarTimingInfo
    {
        public long BarFlg { get; }
        public long TimeSignature { get; }
        public long TargetBeat { get; }
        public BarTimingInfo(long barFlg, long timeSignature, long targetBeat)
        {
            BarFlg = barFlg;
            TimeSignature = timeSignature;
            TargetBeat = targetBeat;
        }
    }
}
