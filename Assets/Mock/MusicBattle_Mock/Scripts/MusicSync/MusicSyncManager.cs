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
            _musicPlayer.Init(source);
            _musicBuffer.Init(source, bpm, bgmProperTime, startOffset);
            _inputHandler.Init(_timeSignatures);
            PlayBgm();
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
        public void RegisterAction(BarTimingInfo barTimingInfo, Action action)
        {
            _actionHandler.RegisterAction(barTimingInfo, action);
        }
        #endregion

        [SerializeField] private float[] _timeSignatures = { 1f, 2f, 3f, 4f, 6f, 8f, 12f, 16f };
        [SerializeField] CriMusicBuffer _musicBuffer;
        [SerializeField] CriMusicPlayer _musicPlayer;
        [SerializeField] MusicInputHandler _inputHandler;
        [SerializeField] MusicActionHandler _actionHandler;

        #region ライフサイクル
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
}
