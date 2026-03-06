using CriWare;
using SymphonyFrameWork.Attribute;
using System;
using UnityEngine;

namespace Mock.MusicBattle.MusicSync
{
    /// <summary>
    ///     Criを用いた音楽のバッファリングするクラス。
    /// </summary>
    public class CriMusicBuffer : IMusicBuffer
    {
        #region コンストラクタ
        /// <summary>
        ///     初期化を行う。
        /// </summary>
        /// <param name="source">再生BGM</param>
        /// <param name="bpm">BGMのBPM</param>
        /// <param name="propTimeSignature">BGMのの固有拍子</param>
        /// <param name="startOffset">最初小節の開始時間(ミリ秒)</param>
        public CriMusicBuffer(CriMusicPlayer player, double bpm, double propTimeSignature, long startOffset)
        {
            _player = player;
            _currentBpm = bpm;
            _propTimeSignature = propTimeSignature;
            _startOffset = startOffset;
        }
        #endregion

        #region パブリックプロパティ
        /// <summary> 現在のBPM。 </summary>
        public double CurrentBpm => _currentBpm;
        /// <summary> 1拍毎の長さ（秒）。 </summary>
        public double BeatLength => 60L / _currentBpm;
        /// <summary> 現在の拍数。 </summary>
        public double CurrentBeat => _beat;
        /// <summary> BGMの固有拍子。 </summary>
        public double PropTimeSignature => _propTimeSignature;
        #endregion

        #region Publicメソッド
        /// <summary>
        ///     現在の拍数を更新する。
        /// </summary>
        public void Tick()
        {
            _beat = (_player.Time - _startOffset) / 1000d / BeatLength;
        }
        #endregion

        #region パブリックインターフェースメソッド
        /// <summary>
        ///     小節タイミング情報を基いて、全体拍数を算出する。
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

        #region インスペクター表示フィールド
        /// <summary> 現在のBPM。 </summary>
        private double _currentBpm;
        /// <summary> 再生BGMソース。 </summary>
        private CriMusicPlayer _player;
        /// <summary> 現在のビート数。 </summary>
        private double _beat;
        /// <summary> BGMの固有拍子。 </summary>
        private double _propTimeSignature;
        /// <summary> 最初小節の開始時間(ミリ秒)。 </summary>
        private long _startOffset;
        #endregion

        #region デバッグ機能
        /// <summary>
        ///     デバッグ用に現在の時間を表示します。
        /// </summary>
        public void OnGUI()
        {
            if (_player == null) return;

            GUILayout.Label($"Time: {_beat}");
        }
        #endregion
    }
}
