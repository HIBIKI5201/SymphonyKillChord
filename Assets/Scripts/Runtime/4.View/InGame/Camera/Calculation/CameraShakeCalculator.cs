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
            if (parameter.Priority <= _currentPriority)
            {
                return;
            }

            // 発生中のモーションを破棄してから、新しいシェイクへ差し替える。
            Reset();
            float duration = parameter.Duration;
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
            _currentPriority = parameter.Priority;
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
        private float _currentPriority;
    }
}
