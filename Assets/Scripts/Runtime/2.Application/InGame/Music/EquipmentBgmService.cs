using KillChord.Runtime.Domain.InGame.Music;
using System;

namespace KillChord.Runtime.Application.InGame.Music
{
    /// <summary>
    ///     装備スキル構成から構築したシーケンスを保持し、小節の進行に応じて
    ///     鳴らすべきBGMセレクターラベルを決定するサービス。
    ///     CRIへの反映は行わず、決定のみを担う純粋なロジックとする。
    /// </summary>
    public sealed class EquipmentBgmService
    {
        /// <summary>
        ///     シーケンスと切り替え間隔からサービスを生成する。
        /// </summary>
        /// <param name="sequence"> 区切りごとに切り替えるラベルのシーケンス。 </param>
        /// <param name="measuresPerDivision"> シーケンスを1ステップ進める間隔（小節数）。 </param>
        public EquipmentBgmService(BgmSelectorSequence sequence, int measuresPerDivision)
        {
            _sequence = sequence;
            _measuresPerDivision = Math.Max(MIN_MEASURES_PER_DIVISION, measuresPerDivision);
        }

        /// <summary> 有効なシーケンスを保持しているかどうかを取得する。 </summary>
        public bool HasSequence => _sequence.HasLabels;

        /// <summary> 再生開始時に適用する初期ラベル（シーケンス先頭＝原曲）を取得する。 </summary>
        public string InitialLabel => _sequence.ResolveLabel(0);

        /// <summary>
        ///     現在の小節から区切りを判定し、区切りが変わった場合に鳴らすべきラベルを返す。
        ///     ラベルは区切り番号からの純粋な写像であり、通知の取りこぼしや小節の飛びが
        ///     発生してもシーケンス位置がズレたまま残らない。
        /// </summary>
        /// <param name="measureIndex"> 現在の小節番号（曲頭からの通し番号）。 </param>
        /// <param name="label"> 適用するセレクターラベル。区切りの変わり目でない場合は空文字。 </param>
        /// <returns> 区切りが変わり切り替えが必要な場合はtrue。 </returns>
        public bool TryResolveLabel(int measureIndex, out string label)
        {
            label = string.Empty;

            if (!_sequence.HasLabels)
            {
                return false;
            }

            int division = FloorDivide(measureIndex, _measuresPerDivision);
            if (_hasPreviousDivision && division == _previousDivision)
            {
                return false;
            }

            _previousDivision = division;
            _hasPreviousDivision = true;

            label = _sequence.ResolveLabel(division);
            return true;
        }

        /// <summary>
        ///     負値でも下方向に丸める除算を行う。
        ///     C#の整数除算は0方向に丸めるため、負の小節番号で区切りがズレるのを防ぐ。
        /// </summary>
        /// <param name="dividend"> 被除数。 </param>
        /// <param name="divisor"> 除数。1以上であること。 </param>
        /// <returns> 下方向に丸めた商。 </returns>
        private static int FloorDivide(int dividend, int divisor)
        {
            int quotient = dividend / divisor;
            if (dividend % divisor != 0 && dividend < 0)
            {
                quotient--;
            }

            return quotient;
        }

        /// <summary> 1区切りの最小小節数。 </summary>
        private const int MIN_MEASURES_PER_DIVISION = 1;

        private readonly BgmSelectorSequence _sequence;
        private readonly int _measuresPerDivision;
        private int _previousDivision;
        private bool _hasPreviousDivision;
    }
}
