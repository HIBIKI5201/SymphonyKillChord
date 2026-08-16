using UnityEngine;

namespace KillChord.Runtime.View.InGame.Camera
{
    /// <summary>
    ///     カメラシェイクの揺れ量の算出を担当するクラス。
    /// </summary>
    public sealed class CameraShakeCalculator
    {
        /// <summary>
        ///     シェイクの発生を要求する。
        ///     発生中のシェイクより弱い要求は、演出が弱まらないよう無視する。
        /// </summary>
        /// <param name="parameter"> 要求するシェイクのパラメータ。</param>
        public void RequestShake(in CameraShakeParameter parameter)
        {
            // 継続時間か揺れ幅が無い設定は演出として成立しないため無視する。
            if (parameter.Duration <= 0f || parameter.Power <= 0f)
            {
                return;
            }

            if (parameter.Power < GetCurrentPower())
            {
                return;
            }

            _parameter = parameter;
            _remainingTime = parameter.Duration;

            // 毎回同じ揺れ方にならないよう、ノイズの参照位置をずらす。
            _noiseSeed = Random.value * NOISE_SEED_RANGE;
        }

        /// <summary>
        ///     経過時間を進め、今フレームのシェイク量を算出する。
        /// </summary>
        /// <param name="deltaTime"> 前フレームからの経過時間。</param>
        /// <param name="positionOffset"> カメラのローカル空間での位置オフセット。</param>
        /// <param name="rotationOffset"> カメラのローカル空間での回転オフセット。</param>
        public void Update(float deltaTime, out Vector3 positionOffset, out Quaternion rotationOffset)
        {
            positionOffset = Vector3.zero;
            rotationOffset = Quaternion.identity;

            if (_remainingTime <= 0f)
            {
                return;
            }

            _remainingTime = Mathf.Max(0f, _remainingTime - deltaTime);

            // 残り時間の比率を二乗し、終盤ほど滑らかに収束させる。
            float progressRatio = _remainingTime / _parameter.Duration;
            float decay = progressRatio * progressRatio;

            float noiseTime = (_parameter.Duration - _remainingTime) * _parameter.Frequency;
            float noiseX = SampleNoise(NOISE_OFFSET_X, noiseTime);
            float noiseY = SampleNoise(NOISE_OFFSET_Y, noiseTime);
            float noiseZ = SampleNoise(NOISE_OFFSET_Z, noiseTime);

            positionOffset = new Vector3(noiseX, noiseY, 0f) * (_parameter.PositionAmplitude * decay);

            float rotationAmplitude = _parameter.RotationAmplitude * decay;
            rotationOffset = Quaternion.Euler(
                -noiseY * rotationAmplitude,
                noiseX * rotationAmplitude,
                noiseZ * rotationAmplitude);
        }

        /// <summary>
        ///     発生中のシェイクを即座に停止する。
        /// </summary>
        public void Reset()
        {
            _remainingTime = 0f;
            _parameter = default;
        }

        /// <summary> ノイズの参照位置をずらす乱数の範囲。 </summary>
        private const float NOISE_SEED_RANGE = 100f;

        /// <summary> X方向のノイズ参照位置のオフセット。 </summary>
        private const float NOISE_OFFSET_X = 0f;

        /// <summary> Y方向のノイズ参照位置のオフセット。 </summary>
        private const float NOISE_OFFSET_Y = 37.7f;

        /// <summary> Z方向のノイズ参照位置のオフセット。 </summary>
        private const float NOISE_OFFSET_Z = 73.3f;

        private CameraShakeParameter _parameter;
        private float _remainingTime;
        private float _noiseSeed;

        /// <summary>
        ///     発生中のシェイクの現在の強さを返す。
        /// </summary>
        /// <returns> 減衰を反映した現在の強さ。シェイクしていない場合は0。</returns>
        private float GetCurrentPower()
        {
            if (_remainingTime <= 0f)
            {
                return 0f;
            }

            float progressRatio = _remainingTime / _parameter.Duration;
            return _parameter.Power * progressRatio * progressRatio;
        }

        /// <summary>
        ///     -1～1の範囲へ変換したパーリンノイズを取得する。
        /// </summary>
        /// <param name="offset"> ノイズの参照位置のオフセット。</param>
        /// <param name="noiseTime"> ノイズの参照位置となる時間。</param>
        /// <returns> -1～1の範囲のノイズ値。</returns>
        private float SampleNoise(float offset, float noiseTime)
        {
            return (Mathf.PerlinNoise(_noiseSeed + offset, noiseTime) - 0.5f) * 2f;
        }
    }
}
