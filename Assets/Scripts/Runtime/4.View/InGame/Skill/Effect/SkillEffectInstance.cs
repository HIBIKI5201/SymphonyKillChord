using KillChord.Runtime.Adaptor.InGame.Skill.Effect;
using KillChord.Runtime.View.InGame.Skill.Effect.Placement;
using KillChord.Runtime.View.InGame.Skill.Effect.Presentation;
using System;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Skill.Effect
{
    /// <summary>
    ///     プールで再利用されるスキルエフェクト1体分のルートView。
    ///     配置ストラテジーと再生ストラテジーを束ね、完了時にプールへの返却を通知する。
    /// </summary>
    public sealed class SkillEffectInstance : MonoBehaviour, ISkillEffectHandle
    {
        /// <summary> 再生中かどうかです。 </summary>
        public bool IsPlaying => _isPlaying;

        /// <summary>
        ///     プール生成時の事前準備を行う。
        /// </summary>
        public void Prewarm()
        {
            CachePresentations();
            for (int i = 0; i < _presentations.Length; i++)
            {
                _presentations[i]?.Prewarm();
            }
        }

        /// <summary>
        ///     配置ストラテジーに従ってエフェクトを再生する。
        /// </summary>
        /// <param name="placement"> 使用する配置ストラテジーです。 </param>
        /// <param name="context"> エフェクトの参照点です。 </param>
        /// <param name="onFinished"> 再生完了時に呼ばれるコールバックです。 </param>
        /// <returns> 再生に成功した場合はtrue。 </returns>
        public bool Play(ISkillEffectPlacement placement, in SkillEffectContext context, Action<SkillEffectInstance> onFinished)
        {
            if (placement == null || !placement.TryResolve(context, out SkillEffectPose pose))
            {
                return false;
            }

            CachePresentations();
            if (_presentations.Length == 0)
            {
                Debug.LogError($"[{nameof(SkillEffectInstance)}] 再生ストラテジーが1つも存在しません。", this);
                return false;
            }

            _followTransform = placement.IsFollow ? pose.FollowTransform : null;
            _onFinished = onFinished;
            _elapsedSeconds = 0f;
            _isPlaying = true;
            ApplyPose(pose.Position, pose.Rotation);

            for (int i = 0; i < _presentations.Length; i++)
            {
                _presentations[i]?.Play(context);
            }

            return true;
        }

        /// <summary>
        ///     エフェクトを停止し、プールへの返却を通知する。
        /// </summary>
        public void Stop()
        {
            if (!_isPlaying)
            {
                return;
            }

            _isPlaying = false;
            for (int i = 0; i < _presentations.Length; i++)
            {
                _presentations[i]?.Stop();
            }

            NotifyFinished();
        }

        [SerializeField, Tooltip("このエフェクトが束ねる再生ストラテジーです。未設定時は子階層から収集します。")]
        private SkillEffectPresentationBase[] _presentations;

        [SerializeField, Tooltip("配置位置に加算するローカルオフセットです。")]
        private Vector3 _positionOffset;

        [SerializeField, Tooltip("配置回転に加算するオイラー角オフセットです。")]
        private Vector3 _rotationOffset;

        [SerializeField, Tooltip("追従型のとき、対象の回転にも追従するかです。")]
        private bool _followsRotation = true;

        [SerializeField, Min(0f), Tooltip("再生完了を検出できなかった場合に強制返却するまでの時間です。0なら無効です。")]
        private float _maxLifetimeSeconds = 30f;

        /// <summary>
        ///     再生ストラテジーの参照を収集する。
        /// </summary>
        private void Awake()
        {
            CachePresentations();
        }

        /// <summary>
        ///     追従更新と完了判定を行う。
        /// </summary>
        private void LateUpdate()
        {
            if (!_isPlaying)
            {
                return;
            }

            float deltaTime = Time.deltaTime;
            _elapsedSeconds += deltaTime;

            UpdateFollow();

            bool anyPlaying = false;
            for (int i = 0; i < _presentations.Length; i++)
            {
                SkillEffectPresentationBase presentation = _presentations[i];
                if (presentation == null)
                {
                    continue;
                }

                anyPlaying |= presentation.UpdatePlayback(deltaTime);
            }

            bool isExpired = _maxLifetimeSeconds > 0f && _elapsedSeconds >= _maxLifetimeSeconds;
            if (anyPlaying && !isExpired)
            {
                return;
            }

            if (isExpired)
            {
                Debug.LogWarning($"[{nameof(SkillEffectInstance)}] 最大再生時間を超えたため強制的に返却します。 Effect: {name}", this);
            }

            Stop();
        }

        /// <summary>
        ///     再生ストラテジーの参照をキャッシュする。
        /// </summary>
        private void CachePresentations()
        {
            if (_presentations != null && _presentations.Length > 0)
            {
                return;
            }

            // インスペクタ未設定時のみ、生成時に一度だけ子階層から収集する。
            _presentations = GetComponentsInChildren<SkillEffectPresentationBase>(true);
        }

        /// <summary>
        ///     追従対象へ位置と回転を追従させる。
        /// </summary>
        private void UpdateFollow()
        {
            if (_followTransform == null)
            {
                return;
            }

            ApplyPose(_followTransform.position, _followTransform.rotation);
        }

        /// <summary>
        ///     オフセットを加味した姿勢を適用する。
        /// </summary>
        /// <param name="position"> 基準となるワールド座標です。 </param>
        /// <param name="rotation"> 基準となるワールド回転です。 </param>
        private void ApplyPose(Vector3 position, Quaternion rotation)
        {
            Quaternion appliedRotation = _followsRotation || _followTransform == null
                ? rotation * Quaternion.Euler(_rotationOffset)
                : transform.rotation;
            transform.SetPositionAndRotation(position + rotation * _positionOffset, appliedRotation);
        }

        /// <summary>
        ///     再生完了を通知する。
        /// </summary>
        private void NotifyFinished()
        {
            Action<SkillEffectInstance> onFinished = _onFinished;
            _onFinished = null;
            _followTransform = null;
            onFinished?.Invoke(this);
        }

        private Action<SkillEffectInstance> _onFinished;
        private Transform _followTransform;
        private float _elapsedSeconds;
        private bool _isPlaying;
    }
}
