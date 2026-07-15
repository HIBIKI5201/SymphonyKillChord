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
        ///     次のWave定義を取得する。
        /// </summary>
        /// <returns></returns>
        public EnemyWaveDefinition GetNextWave()
        {
            EnemyWaveDefinition ret;
            if (_currentIndex < _waves.Length)
            {
                ret = _waves[_currentIndex];
                _currentIndex++;
            }
            else
            {
                ret = null;
            }
            // ループ設定がある場合、ループ開始のindexに戻る
            if (_currentIndex >= _waves.Length && _loopFlg)
            {
                _currentIndex = _loopStart;
            }
            return ret;
        }

        private readonly EnemyWaveDefinition[] _waves;
        private readonly bool _loopFlg;
        private readonly int _loopStart;
        private int _currentIndex;
    }
}
