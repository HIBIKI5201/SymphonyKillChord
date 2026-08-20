using KillChord.Runtime.Adaptor.InGame.Skill.Effect;
using KillChord.Runtime.View.InGame.Skill.Effect.Presentation;
using LitMotion;
using Unity.Transforms;
using UnityEngine;

namespace KillChord.Runtime.View
{
    public class SkillEffectMotionFunnel : MotionSkillEffectPresentationBase
    {
        [SerializeField]
        private float _rotOffset;

        [SerializeField, Tooltip("移動させる対象のTransformです。未設定時は自身を使用します。")]
        private Transform _travelTarget;
        private Vector3 _playerCenter;
        private MotionHandle _handle;
        protected override MotionHandle CreateMotion(in SkillEffectContext context)
        {
            _handle.TryComplete();


            Transform parent = _travelTarget.parent;

            Vector3 startWorldPosition = context.PlayerTransform != null
                ? context.PlayerTransform.position
                : context.WorldPosition;

            Quaternion startRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            startWorldPosition += startRotation * Vector3.forward;

            Vector3 startLocalPosition = parent != null
               ? parent.InverseTransformPoint(startWorldPosition)
               : startWorldPosition;


            _playerCenter = context.PlayerTransform.position;

            _handle = LSequence.Create()
                .Join(LMotion.Create(0f,1f,0.5f)
                    .Bind(this, static (value,state) =>
                    {
                        Vector3 pos = state._playerCenter + Quaternion.Euler(0f, value * 360f * 5f + state._rotOffset, 0f) * Vector3.forward;
                        Quaternion rot = Quaternion.Euler(value * -90f, value * 360f * 5f + state._rotOffset, 0f);
                        state._travelTarget.SetPositionAndRotation(pos, rot);
                    }))
                .Run();
            return _handle;
        }
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
        private void OnDestroy()
        {
            _handle.TryCancel();
        }
    }
}
