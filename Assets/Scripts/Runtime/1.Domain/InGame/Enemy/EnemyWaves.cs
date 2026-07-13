using System;

namespace KillChord.Runtime.Domain.InGame.Enemy
{
    /// <summary>
    ///     1ステージ分の敵Wave定義を保持するエンティティ。
    /// </summary>
    public class EnemyWaves
    {
        public EnemyWaves(EnemyWaveDefinition[] waves, bool loopFlg, int loopStart)
        {
            if (waves == null) throw new ArgumentNullException(nameof(waves));
            if (loopFlg && (waves.Length == 0 || loopStart < 0 || loopStart >= waves.Length))
            {
                throw new ArgumentOutOfRangeException(nameof(loopStart));
            }
            _waves = waves;
            _loopFlg = loopFlg;
            _loopStart = loopStart;
            _currentIndex = 0;
        }

        public bool IsLastWave => (!_loopFlg) && (_currentIndex >= _waves.Length);

        /// <summary>
        ///     次のWave定義とインデックスを取得します。
        /// </summary>
        /// <param name="waveIndex"> 取得したWaveインデックスです。 </param>
        /// <param name="waveDefinition"> 取得したWave定義です。 </param>
        /// <returns> 次のWaveが存在する場合はtrueです。 </returns>
        public bool TryGetNextWave(
            out int waveIndex,
            out EnemyWaveDefinition waveDefinition)
        {
            if (_currentIndex >= _waves.Length)
            {
                waveIndex = -1;
                waveDefinition = null;
                return false;
            }

            waveIndex = _currentIndex;
            waveDefinition = _waves[_currentIndex];
            _currentIndex++;

            // ループ設定がある場合、ループ開始のindexに戻る
            if (_currentIndex >= _waves.Length && _loopFlg)
            {
                _currentIndex = _loopStart;
            }

            return true;
        }

        private readonly EnemyWaveDefinition[] _waves;
        private readonly bool _loopFlg;
        private readonly int _loopStart;
        private int _currentIndex;
    }
}
