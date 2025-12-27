using CriWare;
using System;
using UnityEngine;

namespace Mock.MusicBattle.MusicSync
{
    /// <summary>
    ///     音楽同期システムの全体を管理し、外部へのインターフェースを持つクラス。
    /// </summary>
    [DisallowMultipleComponent]
    public class MusicSyncManager : MonoBehaviour
    {
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
            _musicPlayer.Init(source);
            _musicBuffer.Init(source, bpm, timeSignature, startOffset);
            _inputHandler.Init(_timeSignatures);
            PlayBgm();
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
        public void RegisterAction(BarTimingInfo barTimingInfo, Action action)
        {
            _actionHandler.RegisterAction(barTimingInfo, action);
        }

        /// <summary>
        ///     入力された拍子の履歴を取得します。
        /// </summary>
        /// <returns>入力された拍子の履歴。</returns>
        public bool IsMatchInputTimeSignature(float[] pattern)
        {
            ReadOnlySpan<float> signatureHistory = _inputHandler.GetSignatureHistory();

            // パターンより履歴が短ければ非マッチ。
            if (signatureHistory.Length < pattern.Length) { return false; }

            bool match = true;
            for (int i = 0; i < pattern.Length; i++)
            {
                // 履歴の最新部分とパターンを比較。
                if (signatureHistory[signatureHistory.Length - pattern.Length + i] != pattern[i])
                {
                    match = false;
                    break;
                }
            }

            return match;
        }
        #endregion

        #region インスペクター表示フィールド
        /// <summary> 入力によって検出されうる拍子の配列。 </summary>
        [SerializeField, Tooltip("入力によって検出されうる拍子の配列。")]
        private float[] _timeSignatures = { 1f, 2f, 3f, 4f, 6f, 8f, 12f, 16f };
        /// <summary> 音楽バッファの参照。 </summary>
        [SerializeField, Tooltip("音楽バッファの参照。")]
        private CriMusicBuffer _musicBuffer;
        /// <summary> 音楽プレイヤーの参照。 </summary>
        [SerializeField, Tooltip("音楽プレイヤーの参照。")]
        private CriMusicPlayer _musicPlayer;
        /// <summary> 音楽入力ハンドラの参照。 </summary>
        [SerializeField, Tooltip("音楽入力ハンドラの参照。")]
        private MusicInputHandler _inputHandler;
        /// <summary> 音楽アクションハンドラの参照。 </summary>
        [SerializeField, Tooltip("音楽アクションハンドラの参照。")]
        private MusicActionHandler _actionHandler;
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
    }
}

