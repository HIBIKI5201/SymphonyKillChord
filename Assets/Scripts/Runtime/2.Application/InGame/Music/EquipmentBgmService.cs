using KillChord.Runtime.Domain.InGame.Music;
using System;

namespace KillChord.Runtime.Application.InGame.Music
{
    /// <summary>
    ///     装備スキル構成から構築したシーケンスを保持し、拍の進行に応じて
    ///     切り替えるべきBGMセレクターラベルを判定するサービス。
    ///     CRIへの反映は行わず、切り替え判定のみを担う純粋なロジックとする。
    /// </summary>
    public sealed class EquipmentBgmService
    {
        /// <summary>
        ///     シーケンスと切り替え間隔からサービスを生成する。
        /// </summary>
        /// <param name="sequence"> 小節ごとに切り替えるラベルのシーケンス。 </param>
        /// <param name="measuresPerStep"> シーケンスを1ステップ進める間隔（小節数）。 </param>
        public EquipmentBgmService(BgmSelectorSequence sequence, int measuresPerStep)
        {
            _sequence = sequence;
            _measuresPerStep = Math.Max(MIN_MEASURES_PER_STEP, measuresPerStep);
            _currentIndex = 0;
            _previousStep = 0;
        }

        /// <summary> 有効なシーケンスを保持しているかどうかを取得する。 </summary>
        public bool HasSequence => _sequence.HasLabels;

        /// <summary> 開始時に再生する初期ラベル（シーケンス先頭＝原曲）を取得する。 </summary>
        public string InitialLabel => _sequence.ResolveLabel(0);

        /// <summary>
        ///     現在の拍から小節ステップを判定し、切り替えの変わり目であれば次のラベルを返す。
        /// </summary>
        /// <param name="currentBeat"> 現在の拍（MusicSyncState.CurrentBeat）。 </param>
        /// <param name="label"> 切り替え先のセレクターラベル。変わり目でない場合は空文字。 </param>
        /// <returns> 変わり目で切り替えが必要な場合はtrue。 </returns>
        public bool TryAdvance(int currentBeat, out string label)
        {
            label = string.Empty;
            if (!_sequence.HasLabels)
            {
                return false;
            }

            int currentStep = currentBeat / (BEATS_PER_MEASURE * _measuresPerStep);
            if (currentStep == _previousStep)
            {
                return false;
            }

            _previousStep = currentStep;
            _currentIndex = (_currentIndex + 1) % _sequence.Length;
            label = _sequence.ResolveLabel(_currentIndex);
            return true;
        }

        private const int BEATS_PER_MEASURE = 4;
        private const int MIN_MEASURES_PER_STEP = 1;

        private readonly BgmSelectorSequence _sequence;
        private readonly int _measuresPerStep;
        private int _currentIndex;
        private int _previousStep;
    }
}
