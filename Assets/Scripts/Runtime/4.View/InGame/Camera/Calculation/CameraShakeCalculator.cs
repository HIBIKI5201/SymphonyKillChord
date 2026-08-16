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
        /// <param name="parameter"> 要求するシェイクのパラメータ。</param>
        public void RequestShake(in CameraShakeParameter parameter)
        {
            // 継続時間か強さが無い設定は演出として成立しないため無視する。
            if (parameter.Duration <= 0f || parameter.MaxPower <= 0f)
            {
                return;
            }

            // 抽選結果ではなく設定上の最大値で比較し、演出の強弱が抽選で入れ替わらないようにする。
            if (parameter.MaxPower < GetCurrentPower())
            {
                return;
            }

            // 発生中のモーションを破棄してから、新しいシェイクへ差し替える。
            Reset();
            _duration = parameter.Duration;
            _maxPower = parameter.MaxPower;

            _positionHandle = LSequence.Create()
                .Join(LMotion.Punch.Create(0f, Random.Range(parameter.StrengthRangeX.x, parameter.StrengthRangeX.y), parameter.Duration)
                    .WithFrequency(Mathf.RoundToInt(Random.Range( parameter.Frequency.x, parameter.Frequency.y)))
                    .WithDampingRatio(parameter.DampingRatio)
                    .Bind(this, static (value, state) => state._positionOffset.x = value))

                .Join(LMotion.Punch.Create(0f, Random.Range(parameter.StrengthRangeY.x, parameter.StrengthRangeY.y), parameter.Duration)
                    .WithFrequency(Mathf.RoundToInt(Random.Range(parameter.Frequency.x, parameter.Frequency.y)))
                    .WithDampingRatio(parameter.DampingRatio)
                    .Bind(this, static (value, state) => state._positionOffset.y = value))

                .Join(LMotion.Punch.Create(0f, Random.Range(parameter.StrengthRangeZ.x, parameter.StrengthRangeZ.y), parameter.Duration)
                    .WithFrequency(Mathf.RoundToInt(Random.Range(parameter.Frequency.x, parameter.Frequency.y)))
                    .WithDampingRatio(parameter.DampingRatio)
                    .Bind(this, static (value, state) => state._positionOffset.z = value))
                .Run();
        }

        /// <summary>
        ///     発生中のシェイクを即座に停止し、揺れ量を初期化する。
        /// </summary>
        public void Reset()
        {
            _positionHandle.TryCancel();

            _positionOffset = Vector3.zero;
        }

        private MotionHandle _positionHandle;
        private Vector3 _positionOffset;
        private float _duration;
        private float _maxPower;

        /// <summary>
        ///     発生中のシェイクの現在の強さを返す。
        /// </summary>
        /// <returns> 減衰を反映した現在の強さ。シェイクしていない場合は0。</returns>
        private float GetCurrentPower()
        {
            if (!_positionHandle.IsActive())
            {
                return 0f;
            }

            // 経過時間の比率から、減衰後のおおよその強さを求める。
            float remainingRatio = 1f - Mathf.Clamp01((float)(_positionHandle.Time / _duration));
            return _maxPower * remainingRatio;
        }
    }
}
