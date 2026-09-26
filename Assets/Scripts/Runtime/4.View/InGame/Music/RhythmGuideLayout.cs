using KillChord.Runtime.Adaptor.InGame.Music;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Music
{
    /// <summary>
    ///     ゲージの実描画幅、小節進捗とブロック境界を同じ座標へ変換する。
    /// </summary>
    public readonly struct RhythmGuideLayout
    {
        /// <summary>
        ///     指定幅を隙間なく分割する描画基準を生成する。
        /// </summary>
        public RhythmGuideLayout(float halfWidth, float preferredBlockWidth, float lengthInBars,
            IReadOnlyList<RhythmGuideZoneDto> zones)
        {
            HalfWidth = Mathf.Max(0f, halfWidth);
            LengthInBars = lengthInBars;
            int gridCount = HalfWidth > 0f && lengthInBars > 0f
                ? Mathf.Max(1, Mathf.CeilToInt(HalfWidth / Mathf.Max(1f, preferredBlockWidth)))
                : 0;
            var boundaries = new List<float> { 0f };
            for (int i = 1; i <= gridCount; i++)
            {
                boundaries.Add(HalfWidth * i / gridCount);
            }
            if (gridCount > 0 && zones != null)
            {
                foreach (RhythmGuideZoneDto zone in zones)
                {
                    // 判定の切り替わる位置でブロックを分割し、1ブロック内の色の食い違いを防ぐ。
                    boundaries.Add(Mathf.Clamp01(zone.StartNormalized / lengthInBars) * HalfWidth);
                    boundaries.Add(Mathf.Clamp01(zone.EndNormalized / lengthInBars) * HalfWidth);
                    boundaries.Add(Mathf.Clamp01(zone.JustStartNormalized / lengthInBars) * HalfWidth);
                    boundaries.Add(Mathf.Clamp01(zone.JustEndNormalized / lengthInBars) * HalfWidth);
                }
            }
            boundaries.Sort();
            for (int i = boundaries.Count - 1; i > 0; i--)
            {
                if (boundaries[i] == boundaries[i - 1])
                {
                    boundaries.RemoveAt(i);
                }
            }
            _boundaries = boundaries.ToArray();
            BlockCount = _boundaries.Length - 1;
        }

        /// <summary> 中心から片側の末端までの実描画幅。 </summary>
        public float HalfWidth { get; }
        /// <summary> 末端までの小節数。 </summary>
        public float LengthInBars { get; }
        /// <summary> 片側のブロック数。 </summary>
        public int BlockCount { get; }

        /// <summary>
        ///     小節進捗をゲージ全長に対する進捗へ変換する。
        /// </summary>
        public float GetNormalizedProgress(float barProgress)
        {
            return LengthInBars > 0f ? Mathf.Clamp01(barProgress / LengthInBars) : 0f;
        }

        /// <summary>
        ///     小節進捗を中心からの描画距離へ変換する。
        /// </summary>
        public float GetPosition(float barProgress)
        {
            return GetNormalizedProgress(barProgress) * HalfWidth;
        }

        /// <summary>
        ///     ブロック境界の描画距離を取得する。BlockCountは末端を表す。
        /// </summary>
        public float GetBlockBoundary(int blockIndex)
        {
            return _boundaries != null ? _boundaries[Mathf.Clamp(blockIndex, 0, BlockCount)] : 0f;
        }

        /// <summary>
        ///     ブロック内の拍を解決するため、中心位置を小節進捗へ変換する。
        /// </summary>
        public float GetBlockBarProgress(int blockIndex)
        {
            float center = (GetBlockBoundary(blockIndex) + GetBlockBoundary(blockIndex + 1)) * 0.5f;
            return HalfWidth > 0f ? center / HalfWidth * LengthInBars : 0f;
        }

        /// <summary>
        ///     進捗位置を含むブロックを取得する。区間の開始位置を含み、終了位置は次へ渡す。
        /// </summary>
        public int GetBlockIndex(float barProgress)
        {
            if (BlockCount <= 0)
            {
                return -1;
            }
            int index = Array.BinarySearch(_boundaries, GetPosition(barProgress));
            return Mathf.Clamp(index >= 0 ? index : ~index - 1, 0, BlockCount - 1);
        }

        private readonly float[] _boundaries;
    }
}
