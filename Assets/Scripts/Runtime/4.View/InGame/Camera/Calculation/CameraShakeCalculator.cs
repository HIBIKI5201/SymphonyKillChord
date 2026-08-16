using LitMotion;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Camera
{
    /// <summary>
    ///     LitMotionのPunchモーションでカメラシェイクの揺れ量を生成するクラス。
    /// </summary>
    public sealed class CameraShakeCalculator
    {
        /// <summary> 現在のカメラのローカル空間での位置オフセット。 </summary>
        public Vector3 PositionOffset => _positionOffset;

        /// <summary>
        ///     シェイクの発生を要求する。
        ///     発生中のシェイクより弱い要求は、演出が弱まらないよう無視する。
        /// </summary>
        /// <param name="config"> 要求するシェイクの設定。</param>
        public void RequestShake(CameraShakeConfig config)
        {
            if (config == null)
            {
                return;
            }

            // 継続時間か強さが無い設定は演出として成立しないため無視する。
            if (config.Priority < _currentPriority)
            {
                return;
            }

            // 発生中のモーションを破棄してから、新しいシェイクへ差し替える。
            Reset();
            _positionHandle = LSequence.Create()
                .Join(LMotion.Punch.Create(0f, Random.Range(config.StrengthRangeX.x, config.StrengthRangeX.y), config.Duration)
                    .WithFrequency(Mathf.RoundToInt(Random.Range(config.Frequency.x, config.Frequency.y)))
                    .Bind(this, static (value, state) => state._positionOffset.x = value))

                .Join(LMotion.Punch.Create(0f, Random.Range(config.StrengthRangeY.x, config.StrengthRangeY.y), config.Duration)
                    .WithFrequency(Mathf.RoundToInt(Random.Range(config.Frequency.x, config.Frequency.y)))
                    .Bind(this, static (value, state) => state._positionOffset.y = value))

                .Join(LMotion.Punch.Create(0f, Random.Range(config.StrengthRangeZ.x, config.StrengthRangeZ.y), config.Duration)
                    .WithFrequency(Mathf.RoundToInt(Random.Range(config.Frequency.x, config.Frequency.y)))
                    .Bind(this, static (value, state) => state._positionOffset.z = value))
                .Run(x => x.WithOnComplete(() => _currentPriority = int.MinValue));
            _currentPriority = config.Priority;
        }

        /// <summary>
        ///     発生中のシェイクを即座に停止し、揺れ量を初期化する。
        /// </summary>
        public void Reset()
        {
            _positionHandle.TryCancel();
            _positionOffset = Vector3.zero;
            _currentPriority = int.MinValue;
        }

        private MotionHandle _positionHandle;
        private Vector3 _positionOffset;
        private int _currentPriority;
    }
}
