using KillChord.Runtime.Adaptor.InGame.Skill;
using KillChord.Runtime.Adaptor.InGame.Skill.Effect;
using KillChord.Runtime.Utility.Identity;
using KillChord.Runtime.View.InGame.Skill.Effect;
using KillChord.Runtime.View.Persistent.Music;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Skill
{
    /// <summary>
    ///     スキル発動時のエフェクトと効果音を再生するビュー。
    /// </summary>
    public class SkillView : MonoBehaviour, ISkillVisual
    {
        /// <summary> スキル ID。 </summary>
        public int Id => _id.Id;

        /// <summary>
        ///     スキルエフェクトの再生に必要な依存を注入する。
        /// </summary>
        /// <param name="skillEffectPlayer"> スキルエフェクト再生用のPlayerです。 </param>
        /// <param name="contextFactory"> エフェクトContextの生成器です。 </param>
        public void Initialize(ISkillEffectPlayer skillEffectPlayer, SkillEffectContextFactory contextFactory)
        {
            _skillEffectPlayer = skillEffectPlayer;
            _contextFactory = contextFactory;
        }

        /// <summary>
        ///     スキルのエフェクトと効果音を再生する。
        /// </summary>
        public void Execute()
        {
            PlaySkillEffect();
            PlaySoundEffect();
        }

        [SerializeField, SourceDataCollection("Skill"), Tooltip("表示するスキルのIDです。")]
        private DataID _id;

        [SerializeField, Tooltip("SkillSE再生用SoundEffectSource")] private SoundEffectSource _source;

        [SerializeField, Tooltip("再生するCueの名前")] private string _cueName;

        /// <summary>
        ///     スキルIDに紐づくエフェクトを再生する。
        /// </summary>
        private void PlaySkillEffect()
        {
            if (_skillEffectPlayer == null || _contextFactory == null)
            {
                return;
            }

            _skillEffectPlayer.PlaySkillEffect(Id, _contextFactory.Create());
        }

        /// <summary>
        ///     スキルのSEを再生する。
        /// </summary>
        private void PlaySoundEffect()
        {
            if (_source == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(_cueName))
            {
                _source.Play();
                return;
            }

            _source.Play(_cueName);
        }

        private ISkillEffectPlayer _skillEffectPlayer;
        private SkillEffectContextFactory _contextFactory;
    }
}
