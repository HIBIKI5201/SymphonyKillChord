using KillChord.Runtime.Adaptor.InGame.Skill.Effect;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Skill.Effect.Presentation
{
    /// <summary>
    ///     GameObjectの表示切り替えのみでスキルエフェクトを表現するストラテジー。
    /// </summary>
    public sealed class GameObjectSkillEffectPresentation : SkillEffectPresentationBase
    {
        [SerializeField, Tooltip("表示を切り替える対象のGameObjectです。未設定時は自身を使用します。")]
        private GameObject _targetObject;

        [SerializeField, Min(0f), Tooltip("表示を維持する時間です。")]
        private float _durationSeconds = 1f;

        /// <summary>
        ///     対象GameObjectを解決して非表示にする。
        /// </summary>
        private void Awake()
        {
            if (_targetObject == null)
            {
                _targetObject = gameObject;
            }
        }

        /// <summary>
        ///     対象GameObjectを非表示状態へ整える。
        /// </summary>
        protected override void OnPrewarm()
        {
            if (_targetObject == null)
            {
                _targetObject = gameObject;
            }

            // 自身を含むルートを消すとプール制御と競合するため、別オブジェクトの場合のみ非表示にする。
            if (_targetObject != gameObject)
            {
                _targetObject.SetActive(false);
            }
        }

        /// <summary>
        ///     対象GameObjectを表示する。
        /// </summary>
        /// <param name="context"> エフェクトの参照点です。 </param>
        protected override void OnPlay(in SkillEffectContext context)
        {
            if (_targetObject == null || _targetObject == gameObject)
            {
                return;
            }

            _targetObject.SetActive(true);
        }

        /// <summary>
        ///     対象GameObjectを非表示にする。
        /// </summary>
        protected override void OnStop()
        {
            if (_targetObject == null || _targetObject == gameObject)
            {
                return;
            }

            _targetObject.SetActive(false);
        }

        /// <summary>
        ///     表示時間の経過で再生継続を判定する。
        /// </summary>
        /// <param name="elapsedSeconds"> 再生開始からの経過時間です。 </param>
        /// <returns> 再生が継続している場合はtrue。 </returns>
        protected override bool OnCheckPlaying(float elapsedSeconds)
        {
            return elapsedSeconds < _durationSeconds;
        }
    }
}
