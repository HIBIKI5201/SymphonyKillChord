using CriWare;
using SymphonyFrameWork.Attribute;
using System;
using UnityEngine;

namespace Mock.MusicSyncMock
{
    /// <summary>
    ///     Criを用いた音楽のバッファリングするクラス。
    /// </summary>
    public class CriMusicBuffer : MonoBehaviour, IMusicBuffer
    {
        [SerializeField, ReadOnly, Tooltip("現在のBPM")]
        private double _currentBpm;
        [SerializeField, ReadOnly, Tooltip("再生中のソース")]
        private CriAtomSource _source;
        [SerializeField, ReadOnly, Tooltip("現在のビート数")]
        private double _beat;
        [SerializeField, ReadOnly, Tooltip("BGMの固有拍子")]
        private double _propTimeSignature;
        [SerializeField, Tooltip("最初小節開始時間（ミリ秒）")]
        private long _startOffset;

        /// <summary>現在のBPM</summary>
        public double CurrentBpm => _currentBpm;
        /// <summary>1拍毎の長さ（秒）</summary>
        public double BeatLength => 60L / _currentBpm;
        /// <summary>現在の拍数</summary>
        public double CurrentBeat => _beat;
        /// <summary>BGMの固有拍子</summary>
        public double PropTimeSignature => _propTimeSignature;

        #region ライフサイクル
        private void Update()
        {
            Tick();
        }
        #endregion

        #region Publicメソッド
        /// <summary>
        /// 初期化を行う。
        /// </summary>
        /// <param name="source">再生BGM</param>
        /// <param name="bpm">BGMのBPM</param>
        /// <param propTimeSignature="bpm">BGMのの固有拍子</param>
        /// <param name="startOffset">最初小節の開始時間(ミリ秒)</param>
        public void Init(CriAtomSource source, double bpm, double propTimeSignature, long startOffset)
        {
            _source = source;
            _currentBpm = bpm;
            _propTimeSignature = propTimeSignature;
            _startOffset = startOffset;
        }

        /// <summary>
        /// 小節タイミング情報を基いて、全体拍数を算出する。
        /// </summary>
        /// <param name="barTimingInfo">小節タイミング情報</param>
        /// <returns>全体拍数</returns>
        public double ConvertBarTimingInfoToBeat(BarTimingInfo barTimingInfo)
        {
            // 現在の小節数
            double currentBar = Math.Floor(_beat / _propTimeSignature);
            // 発火小節数
            double targetBar = currentBar + (barTimingInfo.BarFlg);
            // 発火全体拍数
            double exeBeat = targetBar * _propTimeSignature + (_propTimeSignature / barTimingInfo.TimeSignature * barTimingInfo.TargetBeat);

            Debug.Log($"拍数計算　現在小節：{currentBar}, 発火小節：{targetBar}, 発火拍数：{exeBeat}");

            return exeBeat;
        }
        #endregion

        #region Privateメソッド
        private void OnGUI()
        {
            if (_source == null) return;

            GUILayout.Label($"Time: {_beat}");
        }

        /// <summary>
        /// 現在の拍数を更新する。
        /// </summary>
        private void Tick()
        {
            _beat = (_source.time - _startOffset ) / 1000d  / BeatLength;
        }
        #endregion
    }
}