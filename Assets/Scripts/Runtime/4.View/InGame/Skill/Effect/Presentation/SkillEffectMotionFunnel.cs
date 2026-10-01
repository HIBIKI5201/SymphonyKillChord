using KillChord.Runtime.Adaptor.InGame.Skill.Effect;
using KillChord.Runtime.View.InGame.Skill.Effect.Presentation;
using LitMotion;
using Unity.Transforms;
using UnityEngine;

namespace KillChord.Runtime.View
{
    /// <summary>
    ///     エフェクトを螺旋状に回しながら到達点へ移動させるスキル演出。
    /// </summary>
    public class SkillEffectMotionFunnel : MotionSkillEffectPresentationBase
    {
        [SerializeField, Tooltip("到達点の高さです。エフェクト原点からのローカル値で固定されます。")]
        private float _ringHeight = 1f;
        [SerializeField, Tooltip("螺旋の回転角に足すオフセット（度）。")]
        private float _rotOffset;

        [SerializeField, Tooltip("移動させる対象のTransformです。未設定時は自身を使用します。")]
        private Transform _travelTarget;
        private Vector3 _playerCenter;
        private Vector3 _gunMoveStartPos;
        private float _gunGoalRotX;
        private float _gunGoalRotY;
        private MotionHandle _handle;
        /// <summary>
        ///     エフェクトを螺旋状に移動させるモーションを生成する。
        /// </summary>
        protected override MotionHandle CreateMotion(in SkillEffectContext context)
        {
            _handle.TryComplete();

            if (_travelTarget == null) _travelTarget = transform;

            Transform parent = _travelTarget.parent;

            // プレイヤーの周囲のランダムな方向を開始位置にする。
            Vector3 startWorldPosition = context.PlayerTransform != null
                ? context.PlayerTransform.position
                : context.WorldPosition;

            Quaternion startRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            startWorldPosition += startRotation * Vector3.forward;

            Vector3 startLocalPosition = parent != null
               ? parent.InverseTransformPoint(startWorldPosition)
               : startWorldPosition;


            // 銃の最終的な向きをランダムに決める。
            _playerCenter = context.PlayerTransform.position;
            _gunGoalRotX = Random.Range(-10f, 45f);
            _gunGoalRotY = Random.Range(0f, 360f);
            // 螺旋状に上昇し、外へ飛び出し、プレイヤーの背後へ回り込み、最後に向きを整える4段階のモーション。
            _handle = LSequence.Create()
                .Append(LMotion.Create(0f,1f,0.3f / context.PlaybackSpeed)
                    .WithEase(Ease.InQuad)
                    .Bind(this, static (value,state) =>
                    {
                        Vector3 pos = state._playerCenter + Quaternion.Euler(0f, value * 360f + state._rotOffset, 0f) * Vector3.forward + Vector3.up * state._ringHeight * value;
                        Quaternion rot = Quaternion.Euler(
                            Mathf.Lerp(-90f,0f,Mathf.Clamp01(value * 1.1f)),
                            value * 360f + state._rotOffset, 
                            0f);
                        state._travelTarget.SetPositionAndRotation(pos, rot);

                    }))
                .Append(LMotion.Create(0f,1f,0.05f / context.PlaybackSpeed)
                    .WithEase(Ease.OutCirc)
                    .Bind(this,static (value,state) =>
                    {
                        Vector3 pos = state._playerCenter + state._travelTarget.rotation * Vector3.forward * Mathf.Lerp(1f, 4f,value);
                        state._travelTarget.position = pos;
                        state._gunMoveStartPos = pos;
                    }))
                .Append(LMotion.Create(0f,1f,0.1f / context.PlaybackSpeed)
                    .Bind(this, static (value, state) =>
                    {
                        Vector3 goalPos = state._travelTarget.parent.position + Quaternion.Euler(0f, state._gunGoalRotY, 0f) * Vector3.forward * -4f;
                        Vector3 pos = Vector3.Lerp(state._gunMoveStartPos, goalPos, value) + Vector3.up * state._ringHeight;
                        Quaternion rot = Quaternion.LookRotation(state._travelTarget.parent.position - pos, Vector3.up);
                        state._travelTarget.SetPositionAndRotation(pos, rot);
                    }))
                .Append(LMotion.Create(0f, 1f, 0.3f / context.PlaybackSpeed)
                    .WithEase(Ease.OutCirc)
                    .Bind(this, static (value, state) =>
                    {
                        Quaternion rot = Quaternion.Euler(Mathf.Lerp(0f, state._gunGoalRotX, value), state._gunGoalRotY + value * 45f, 0f);
                        Vector3 pos = state._travelTarget.parent.position + rot * Vector3.forward * -4f + Vector3.up * state._ringHeight;
                        state._travelTarget.SetPositionAndRotation(pos, rot);
                    }))
                .Run();
            return _handle;
        }

        /// <summary>
        ///     再生中のモーションを止める。
        /// </summary>
        private void OnDestroy()
        {
            _handle.TryCancel();
        }
    }
}
