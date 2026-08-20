using KillChord.Runtime.Adaptor.InGame.Skill.Effect;
using KillChord.Runtime.Adaptor.InGame.Target;
using KillChord.Runtime.View.InGame.Target;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Skill.Effect
{
    /// <summary>
    ///     プレイヤーと現在ターゲットからスキルエフェクトのContextを生成するクラス。
    /// </summary>
    public sealed class SkillEffectContextFactory
    {
        /// <summary>
        ///     Contextの生成元を受け取って初期化する。
        /// </summary>
        /// <param name="playerTransform"> プレイヤーのTransformです。 </param>
        /// <param name="targetSystemViewModel"> ターゲットシステムのViewModelです。 </param>
        /// <param name="playbackSpeed"> BPMに応じた再生速度倍率です。 </param>
        /// <param name="weaponSource"> エフェクトの取り付け先となる武器の供給元です。 </param>
        public SkillEffectContextFactory(
            Transform playerTransform,
            ITargetSystemViewModel targetSystemViewModel,
            float playbackSpeed,
            ISkillEffectWeaponSource weaponSource)
        {
            _playerTransform = playerTransform;
            _targetSystemViewModel = targetSystemViewModel;
            _playbackSpeed = playbackSpeed;
            _weaponSource = weaponSource;
        }

        /// <summary>
        ///     現在の状況からContextを生成する。
        /// </summary>
        /// <returns> 生成したContextです。 </returns>
        public SkillEffectContext Create()
        {
            Transform targetTransform = ResolveTargetTransform(out Vector3 targetPosition, out bool hasTarget);
            Vector3 worldPosition = hasTarget ? targetPosition : ResolvePlayerPosition();
            return new SkillEffectContext(
                _playerTransform,
                targetTransform,
                worldPosition,
                ResolveDirection(worldPosition, hasTarget),
                playbackSpeed: _playbackSpeed,
                weaponTransform: _weaponSource?.WeaponTransform);
        }

        /// <summary>
        ///     現在ターゲットのTransformと位置を解決する。
        /// </summary>
        /// <param name="targetPosition"> 解決したターゲット位置です。 </param>
        /// <param name="hasTarget"> ターゲットを解決できた場合はtrueです。 </param>
        /// <returns> 解決したTransformです。追従できない場合はnull。 </returns>
        private Transform ResolveTargetTransform(out Vector3 targetPosition, out bool hasTarget)
        {
            targetPosition = Vector3.zero;
            hasTarget = false;
            if (_targetSystemViewModel == null
                || !_targetSystemViewModel.TryGetCurrentTarget(out ITargetableViewModel targetable)
                || targetable == null
                || !targetable.IsAlive)
            {
                return null;
            }

            targetPosition = targetable.Position;
            hasTarget = true;

            // Transformを持つ実装のみ追従できるため、それ以外は位置のみを使用する。
            return targetable is TransformTargetable transformTargetable ? transformTargetable.TargetTransform : null;
        }

        /// <summary>
        ///     プレイヤーの現在位置を解決する。
        /// </summary>
        /// <returns> プレイヤーの現在位置です。 </returns>
        private Vector3 ResolvePlayerPosition()
        {
            return _playerTransform != null ? _playerTransform.position : Vector3.zero;
        }

        /// <summary>
        ///     エフェクトの向きを解決する。
        /// </summary>
        /// <param name="worldPosition"> エフェクトのワールド座標です。 </param>
        /// <param name="hasTarget"> ターゲットを解決できている場合はtrueです。 </param>
        /// <returns> 解決した方向ベクトルです。 </returns>
        private Vector3 ResolveDirection(Vector3 worldPosition, bool hasTarget)
        {
            if (_playerTransform == null)
            {
                return Vector3.forward;
            }

            // ターゲットがある場合はプレイヤーから対象へ向かう方向を使用する。
            if (!hasTarget)
            {
                return _playerTransform.forward;
            }

            Vector3 direction = worldPosition - _playerTransform.position;
            return direction.sqrMagnitude <= MINIMUM_SQR_MAGNITUDE ? _playerTransform.forward : direction.normalized;
        }

        private const float MINIMUM_SQR_MAGNITUDE = 0.0001f;

        private readonly Transform _playerTransform;
        private readonly ITargetSystemViewModel _targetSystemViewModel;
        private readonly float _playbackSpeed;
        private readonly ISkillEffectWeaponSource _weaponSource;
    }
}
