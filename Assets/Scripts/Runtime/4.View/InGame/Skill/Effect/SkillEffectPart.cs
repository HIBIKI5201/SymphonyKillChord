using KillChord.Runtime.Adaptor.InGame.Skill.Effect;
using KillChord.Runtime.View.InGame.Skill.Effect.Placement;
using KillChord.Runtime.View.InGame.Skill.Effect.Presentation;
using System;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Skill.Effect
{
    /// <summary>
    ///     1つのスキルエフェクト内で、独立した配置を持つ構成要素。
    ///     プレイヤー追従の演出と対象追従の演出を、同じエフェクトへ同居させるために使用する。
    /// </summary>
    [Serializable]
    public sealed class SkillEffectPart
    {
        /// <summary> この構成要素のルートTransformです。 </summary>
        public Transform PartRoot => _partRoot;

        /// <summary> この構成要素が束ねる再生ストラテジーです。 </summary>
        public SkillEffectPresentationBase[] Presentations => _presentations;

        /// <summary> 解決済みの配置ストラテジーです。 </summary>
        public ISkillEffectPlacement Placement => _placement;

        /// <summary> 毎フレームの追従更新が必要かどうかです。 </summary>
        public bool IsFollow => _placement != null && _placement.IsFollow;

        /// <summary>
        ///     配置ストラテジーを解決してキャッシュする。
        /// </summary>
        public void ResolvePlacement()
        {
            _placement = SkillEffectPlacementResolver.Resolve(_attachMode, _betweenRatio);
        }

        /// <summary>
        ///     再生ストラテジーの参照を必要時に収集する。
        /// </summary>
        public void CachePresentations()
        {
            if (_presentations != null && _presentations.Length > 0)
            {
                return;
            }

            // インスペクタ未設定時のみ、ルート配下から収集する。
            _presentations = _partRoot != null
                ? _partRoot.GetComponentsInChildren<SkillEffectPresentationBase>(true)
                : Array.Empty<SkillEffectPresentationBase>();
        }

        /// <summary>
        ///     配置結果へオフセットを加味してルートへ適用する。
        /// </summary>
        /// <param name="pose"> 適用する配置結果です。 </param>
        public void ApplyPose(in SkillEffectPose pose)
        {
            if (_partRoot == null)
            {
                return;
            }

            Quaternion appliedRotation = _followsRotation
                ? pose.Rotation * Quaternion.Euler(_rotationOffset)
                : Quaternion.Euler(_rotationOffset);
            _partRoot.SetPositionAndRotation(
                pose.Position + (pose.Rotation * _positionOffset),
                appliedRotation);
        }

        [SerializeField, Tooltip("この構成要素のルートTransformです。配置はこのTransformへ適用されます。")]
        private Transform _partRoot;

        [SerializeField, Tooltip("この構成要素の配置方式です。")]
        private SkillEffectAttachMode _attachMode = SkillEffectAttachMode.PlayerPoint;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("2点間配置で使用する補間比率です。プレイヤーが0、対象が1です。")]
        private float _betweenRatio = 0.5f;

        [SerializeField, Tooltip("配置位置に加算するローカルオフセットです。")]
        private Vector3 _positionOffset;

        [SerializeField, Tooltip("配置回転に加算するオイラー角オフセットです。")]
        private Vector3 _rotationOffset;

        [SerializeField, Tooltip("配置先の回転にも追従するかです。")]
        private bool _followsRotation = true;

        [SerializeField, Tooltip("この構成要素が束ねる再生ストラテジーです。未設定時は子階層から収集します。")]
        private SkillEffectPresentationBase[] _presentations;

        private ISkillEffectPlacement _placement;
    }
}
