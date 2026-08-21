using KillChord.Runtime.Adaptor.InGame.Skill.Effect;
using System.Threading;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Skill.Effect.Presentation
{
    /// <summary>
    ///     GameObjectの表示切り替えのみでスキルエフェクトを表現するストラテジー。
    /// </summary>
    public sealed class GameObjectSkillEffectPresentation : SkillEffectPresentationBase
    {
        [SerializeField, Tooltip("表示を切り替える対象のGameObjectです。自身以外を指定します。")]
        private GameObject _targetObject;

        [SerializeField, Min(0f), Tooltip("表示を維持する時間です。")]
        private float _durationSeconds = 1f;

        /// <summary>
        ///     対象GameObjectを非表示状態へ整える。
        /// </summary>
        protected override void OnPrewarm()
        {
            OnStop();
        }

        /// <summary>
        ///     対象GameObjectを表示し、指定時間だけ待機する。
        /// </summary>
        /// <param name="context"> エフェクトの参照点です。 </param>
        /// <param name="cancellationToken"> 再生を中断するためのキャンセルトークンです。 </param>
        /// <returns> 再生完了を待機するAwaitableです。 </returns>
        protected override async Awaitable OnPlayAsync(SkillEffectContext context, CancellationToken cancellationToken)
        {
            if (!HasValidTarget())
            {
                return;
            }

            _targetObject.SetActive(true);
            await Awaitable.WaitForSecondsAsync(_durationSeconds / context.PlaybackSpeed, cancellationToken);
        }

        /// <summary>
        ///     対象GameObjectを非表示にする。
        /// </summary>
        protected override void OnStop()
        {
            if (!HasValidTarget())
            {
                return;
            }

            _targetObject.SetActive(false);
        }

        /// <summary>
        ///     切り替え可能な対象が設定されているか判定する。
        /// </summary>
        /// <returns> 自身以外の有効な対象が設定されている場合はtrueです。 </returns>
        private bool HasValidTarget()
        {
            // 自身を消すとプールの有効・無効制御と競合するため、対象から除外する。
            return _targetObject != null && _targetObject != gameObject;
        }
    }
}
