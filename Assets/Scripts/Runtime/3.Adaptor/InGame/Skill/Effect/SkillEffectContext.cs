using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Skill.Effect
{
    /// <summary>
    ///     スキルエフェクトの再生に必要な参照点をViewへ受け渡すDTO。
    /// </summary>
    public readonly struct SkillEffectContext
    {
        /// <summary>
        ///     エフェクト再生Contextを生成する。
        /// </summary>
        /// <param name="playerTransform"> プレイヤーのTransformです。 </param>
        /// <param name="targetTransform"> 対象のTransformです。対象が存在しない場合はnull。 </param>
        /// <param name="worldPosition"> ワールド設置座標、および対象未解決時のフォールバック座標です。 </param>
        /// <param name="direction"> エフェクトの向きです。ゼロベクトルの場合は既定の向きを使用します。 </param>
        /// <param name="scale"> エフェクトのスケール倍率です。 </param>
        public SkillEffectContext(
            Transform playerTransform,
            Transform targetTransform,
            Vector3 worldPosition,
            Vector3 direction,
            float scale = DEFAULT_SCALE)
        {
            _playerTransform = playerTransform;
            _targetTransform = targetTransform;
            _worldPosition = worldPosition;
            _direction = direction;
            _scale = scale;
        }

        /// <summary> プレイヤーのTransformです。 </summary>
        public Transform PlayerTransform => _playerTransform;

        /// <summary> 対象のTransformです。 </summary>
        public Transform TargetTransform => _targetTransform;

        /// <summary> ワールド設置座標です。 </summary>
        public Vector3 WorldPosition => _worldPosition;

        /// <summary> エフェクトの向きです。 </summary>
        public Vector3 Direction => _direction;

        /// <summary> エフェクトのスケール倍率です。 </summary>
        public float Scale => _scale;

        /// <summary> 対象が解決済みかどうかです。 </summary>
        public bool HasTarget => _targetTransform != null;

        /// <summary>
        ///     プレイヤーを基準にしたContextを生成する。
        /// </summary>
        /// <param name="playerTransform"> プレイヤーのTransformです。 </param>
        /// <returns> 生成したContextです。 </returns>
        public static SkillEffectContext FromPlayer(Transform playerTransform)
        {
            if (playerTransform == null)
            {
                return default;
            }

            return new SkillEffectContext(
                playerTransform,
                null,
                playerTransform.position,
                playerTransform.forward);
        }

        private const float DEFAULT_SCALE = 1f;

        private readonly Transform _playerTransform;
        private readonly Transform _targetTransform;
        private readonly Vector3 _worldPosition;
        private readonly Vector3 _direction;
        private readonly float _scale;
    }
}
