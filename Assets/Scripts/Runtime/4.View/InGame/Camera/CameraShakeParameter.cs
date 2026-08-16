using System;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Camera
{
    /// <summary>
    ///     カメラシェイク1回分の揺れ方を定義するパラメータ。
    /// </summary>
    [Serializable]
    public struct CameraShakeParameter
    {
        /// <summary>
        ///     各パラメータを指定してカメラシェイク設定を生成するコンストラクタ。
        /// </summary>
        /// <param name="duration"> 揺れの継続時間(秒)。</param>
        /// <param name="positionAmplitude"> 位置の揺れ幅(メートル)。</param>
        /// <param name="rotationAmplitude"> 回転の揺れ幅(度)。</param>
        /// <param name="frequency"> 揺れの周波数。</param>
        public CameraShakeParameter(float duration, float positionAmplitude, float rotationAmplitude, float frequency)
        {
            _duration = duration;
            _positionAmplitude = positionAmplitude;
            _rotationAmplitude = rotationAmplitude;
            _frequency = frequency;
        }

        /// <summary> 揺れの継続時間(秒)。 </summary>
        public readonly float Duration => _duration;

        /// <summary> 位置の揺れ幅(メートル)。 </summary>
        public readonly float PositionAmplitude => _positionAmplitude;

        /// <summary> 回転の揺れ幅(度)。 </summary>
        public readonly float RotationAmplitude => _rotationAmplitude;

        /// <summary> 揺れの周波数。 </summary>
        public readonly float Frequency => _frequency;

        /// <summary> 位置と回転の揺れ幅を比較可能な1つの強さへ換算した値。 </summary>
        public readonly float Power => _positionAmplitude + (_rotationAmplitude * ROTATION_POWER_WEIGHT);

        /// <summary> 回転の揺れ幅(度)を位置の揺れ幅(メートル)相当へ換算する重み。 </summary>
        private const float ROTATION_POWER_WEIGHT = 0.02f;

        [Min(0f)]
        [SerializeField, Tooltip("揺れの継続時間(秒)。0以下の場合はシェイクしません。")]
        private float _duration;

        [Min(0f)]
        [SerializeField, Tooltip("位置の揺れ幅(メートル)。")]
        private float _positionAmplitude;

        [Min(0f)]
        [SerializeField, Tooltip("回転の揺れ幅(度)。")]
        private float _rotationAmplitude;

        [Min(0f)]
        [SerializeField, Tooltip("揺れの周波数。大きいほど細かく振動します。")]
        private float _frequency;
    }
}
