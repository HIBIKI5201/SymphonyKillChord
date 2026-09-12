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
        ///     発生中のシェイクより優先度が低い要求は、演出が弱まらないよう無視する。
        ///     優先度が同値の場合は、新しい要求で上書きする。
        /// </summary>
        /// <param name="config"> 要求するシェイクの設定。</param>
        /// <returns> シェイクを開始した場合は true。要求を無視した場合は false。</returns>
        public bool TryRequestShake(CameraShakeConfig config)
        {
            // 設定が未割り当ての場合は演出として成立しないため無視する。
            if (config == null)
            {
                return false;
            }

            // 継続時間が無い設定は揺れが発生しないため、モーションを確保せず無視する。
            if (config.Duration <= 0)
            {
                return false;
            }

            // 発生中のシェイクより優先度が低い要求は、強い演出を上書きしないよう無視する。
            // 優先度はシェイク発生中のみ有効。モーションが終了すると IsActive() が false になり、
            // _currentPriority の値に関わらず次の要求を受け付ける。
            if (config.Priority < _currentPriority && _positionHandle.IsActive())
            {
                return false;
            }


            // 発生中のモーションを破棄してから、新しいシェイクへ差し替える。
            Reset();
            _positionHandle = LSequence.Create()
                .Join(LMotion.Punch.Create(0f, Random.Range(config.StrengthRangeX.x, config.StrengthRangeX.y), config.Duration)
                    .WithFrequency(Random.Range(config.Frequency.x, config.Frequency.y + 1))
                    .Bind(this, static (value, state) => state._positionOffset.x = value))

                .Join(LMotion.Punch.Create(0f, Random.Range(config.StrengthRangeY.x, config.StrengthRangeY.y), config.Duration)
                    .WithFrequency(Random.Range(config.Frequency.x, config.Frequency.y + 1))
                    .Bind(this, static (value, state) => state._positionOffset.y = value))

                .Join(LMotion.Punch.Create(0f, Random.Range(config.StrengthRangeZ.x, config.StrengthRangeZ.y), config.Duration)
                    .WithFrequency(Random.Range(config.Frequency.x, config.Frequency.y + 1))
                    .Bind(this, static (value, state) => state._positionOffset.z = value))
                .Run();
            _currentPriority = config.Priority;
            return true;
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

        /// <summary> 発生中のシェイクを再生するモーションハンドル。 </summary>
        private MotionHandle _positionHandle;

        /// <summary> 現在の揺れ量。カメラのローカル空間での位置オフセット。 </summary>
        private Vector3 _positionOffset;

        /// <summary> 直近に受け付けたシェイクの優先度。モーション終了後も値は残るため、<see cref="_positionHandle"/> が有効な間のみ意味を持つ。 </summary>
        private int _currentPriority;
    }
}
