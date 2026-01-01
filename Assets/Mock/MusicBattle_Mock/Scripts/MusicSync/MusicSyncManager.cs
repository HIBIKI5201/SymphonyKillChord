using CriWare;
using System;
using System.Threading;
using UnityEngine;

namespace Mock.MusicBattle.MusicSync
{
    /// <summary>
    ///     音楽同期システムの全体を管理し、外部へのインターフェースを持つクラス。
    /// </summary>
    [DisallowMultipleComponent]
    public class MusicSyncManager : MonoBehaviour
    {
        #region Publicプロパティ
        public CriMusicBuffer MusicBuffer => _musicBuffer;
        #endregion

        #region Publicメソッド
        /// <summary>
        ///     初期化処理を行います。
        /// </summary>
        /// <param name="source">再生するBGMのCriAtomSource。</param>
        /// <param name="bpm">BGMのBPM。</param>
        /// <param name="timeSignature">BGM固有の拍子。</param>
        /// <param name="startOffset">最初の小節の開始時間（ミリ秒）。</param>
        public void Init(CriAtomSource source, double bpm, double timeSignature, long startOffset)
        {
            _musicPlayer = new(source);
            _musicBuffer = new(_musicPlayer, bpm, timeSignature, startOffset);
            _inputHandler = new(_musicBuffer, _configs, _timeSignatures);
            PlayBgm();
            _actionHandler.Init(_musicBuffer);
        }

        /// <summary>
        ///     入力によって成り立つ拍子を取得します。
        /// </summary>
        /// <returns>入力によって成り立つ拍子。</returns>
        public float GetInputTimeSignature()
        {
            return _inputHandler.GetInputTimeSignature();
        }

        /// <summary>
        ///     アクション予約を行います。
        /// </summary>
        /// <param name="barTimingInfo">小節タイミング情報。</param>
        /// <param name="action">実行アクション。</param>
        public void RegisterAction(BarTimingInfo barTimingInfo, Action action, CancellationToken token)
        {
            _actionHandler.RegisterAction(barTimingInfo, action, token);
        }

        /// <summary>
        ///     入力された拍子の履歴を取得します。
        /// </summary>
        /// <returns>入力された拍子の履歴。</returns>
        public bool IsMatchInputTimeSignature(RythemPatternData pattern)
        {
            ReadOnlySpan<float> signatureHistory = _inputHandler.GetSignatureHistory();
            bool match = pattern.IsMatch(signatureHistory);

            return match;
        }
        #endregion

        #region インスペクター表示フィールド
        /// <summary> 入力によって検出されうる拍子の配列。 </summary>
        [SerializeField, Tooltip("入力によって検出されうる拍子の配列。")]
        private SignatureDatabase _timeSignatures;
        [SerializeField, Tooltip("コンフィグ")]
        private MusicSyncConfigs _configs;
        /// <summary> 音楽アクションハンドラの参照。 </summary>
        [SerializeField, Tooltip("音楽アクションハンドラの参照。")]
        private MusicActionHandler _actionHandler;
        #endregion

        #region プライベートフィールド
        /// <summary> 音楽バッファの参照。 </summary>
        private CriMusicBuffer _musicBuffer;
        /// <summary> 音楽プレイヤーの参照。 </summary>
        private CriMusicPlayer _musicPlayer;
        /// <summary> 音楽入力ハンドラの参照。 </summary>
        private MusicInputHandler _inputHandler;
        #endregion

        #region Unityイベントメソッド
        private void Update()
        {
            _musicBuffer.Tick();
        }
        #endregion

        #region Privateメソッド
        /// <summary>
        ///     BGMの再生を開始します。
        /// </summary>
        private void PlayBgm()
        {
            _musicPlayer.Play();
        }
        #endregion

        #region デバッグ機能
        private void OnGUI()
        {
            _musicBuffer?.OnGUI();
        }
        #endregion
    }
}

