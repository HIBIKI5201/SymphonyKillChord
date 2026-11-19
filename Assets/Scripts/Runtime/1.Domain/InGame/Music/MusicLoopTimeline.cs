using System;

namespace KillChord.Runtime.Domain.InGame.Music
{
    /// <summary>
    ///     ループする曲の再生位置と、ゲーム側の音楽時間を相互に換算する値オブジェクトです。
    ///     ゲーム側の拍のグリッドは、ループの頭が来るたびに小節の頭へそろえ直します。
    ///     ループの長さが小節の整数倍でないときは、ループの継ぎ目でゲーム側の時間を次の小節の頭まで進めます。
    /// </summary>
    public readonly struct MusicLoopTimeline
    {
        /// <summary>
        ///     ループの長さと小節の長さを指定して初期化します。
        /// </summary>
        /// <param name="loopLengthSeconds"> 曲のループの長さ（秒）。0以下ならループしない曲として扱います。 </param>
        /// <param name="barLengthSeconds"> 1小節の長さ（秒）。0以下なら換算せずにそのまま返します。 </param>
        public MusicLoopTimeline(double loopLengthSeconds, double barLengthSeconds)
        {
            bool isValid = IsFinitePositive(loopLengthSeconds) && IsFinitePositive(barLengthSeconds);
            LoopLengthSeconds = isValid ? loopLengthSeconds : 0d;
            GridLoopLengthSeconds = isValid ? CalculateGridLoopLength(loopLengthSeconds, barLengthSeconds) : 0d;
        }

        /// <summary> 曲のループの長さ（秒）。ループしない場合は0。 </summary>
        public double LoopLengthSeconds { get; }

        /// <summary> ゲーム側で1周に割り当てる長さ（秒）。小節の整数倍です。ループしない場合は0。 </summary>
        public double GridLoopLengthSeconds { get; }

        /// <summary> ループに合わせて換算するときはtrue。 </summary>
        public bool IsLooping => LoopLengthSeconds > 0d && GridLoopLengthSeconds > 0d;

        /// <summary> 小節の整数倍とみなす、ループの長さの端数の上限（秒）。 </summary>
        public const double LOOP_LENGTH_TOLERANCE_SECONDS = 0.005d;

        /// <summary>
        ///     曲の頭からの通しの再生位置を、ゲーム側の音楽時間へ換算します。
        /// </summary>
        /// <param name="songPositionSeconds"> 曲の頭からの通しの再生位置（秒）。ループしても巻き戻りません。 </param>
        /// <returns> ゲーム側の音楽時間（秒）。 </returns>
        public double ToGameTime(double songPositionSeconds)
        {
            if (!IsLooping || songPositionSeconds <= 0d)
            {
                return songPositionSeconds;
            }

            double loopIndex = Math.Floor(songPositionSeconds / LoopLengthSeconds);
            double positionInLoop = songPositionSeconds - loopIndex * LoopLengthSeconds;
            return loopIndex * GridLoopLengthSeconds + positionInLoop;
        }

        /// <summary>
        ///     ゲーム側の音楽時間を、曲の頭からの通しの再生位置へ換算します。
        ///     ループの継ぎ目で飛ばした区間の時刻は、次のループの頭として扱います。
        /// </summary>
        /// <param name="gameTimeSeconds"> ゲーム側の音楽時間（秒）。 </param>
        /// <returns> 曲の頭からの通しの再生位置（秒）。 </returns>
        public double ToSongPosition(double gameTimeSeconds)
        {
            if (!IsLooping || gameTimeSeconds <= 0d)
            {
                return gameTimeSeconds;
            }

            double loopIndex = Math.Floor(gameTimeSeconds / GridLoopLengthSeconds);
            double positionInLoop = gameTimeSeconds - loopIndex * GridLoopLengthSeconds;
            if (positionInLoop >= LoopLengthSeconds)
            {
                // 曲に無い区間（端数を飛ばした部分）は、次のループの頭から鳴らす。
                return (loopIndex + 1d) * LoopLengthSeconds;
            }

            return loopIndex * LoopLengthSeconds + positionInLoop;
        }

        /// <summary>
        ///     ループの長さから、ゲーム側で1周に割り当てる小節の整数倍の長さを求めます。
        ///     端数がごく小さいときは切り捨て、それ以外は次の小節の頭まで切り上げます。
        /// </summary>
        /// <param name="loopLengthSeconds"> ループの長さ（秒）。 </param>
        /// <param name="barLengthSeconds"> 1小節の長さ（秒）。 </param>
        /// <returns> 小節の整数倍の長さ（秒）。 </returns>
        private static double CalculateGridLoopLength(double loopLengthSeconds, double barLengthSeconds)
        {
            double wholeBarCount = Math.Floor(loopLengthSeconds / barLengthSeconds);
            double remainderSeconds = loopLengthSeconds - wholeBarCount * barLengthSeconds;
            bool isAlmostWhole = remainderSeconds <= LOOP_LENGTH_TOLERANCE_SECONDS
                || barLengthSeconds - remainderSeconds <= LOOP_LENGTH_TOLERANCE_SECONDS;
            double barCount = isAlmostWhole
                ? Math.Round(loopLengthSeconds / barLengthSeconds)
                : wholeBarCount + 1d;
            return Math.Max(1d, barCount) * barLengthSeconds;
        }

        /// <summary>
        ///     有限の正の値か判定します。
        /// </summary>
        /// <param name="value"> 判定する値。 </param>
        /// <returns> 有限の正の値ならtrue。 </returns>
        private static bool IsFinitePositive(double value)
        {
            return value > 0d && !double.IsNaN(value) && !double.IsInfinity(value);
        }
    }
}
