using KillChord.Runtime.Adaptor.InGame.Skill;
using KillChord.Runtime.Utility.Identity;
using KillChord.Runtime.View.Persistent.Music;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Skill
{
    public class SkillView : MonoBehaviour, ISkillVisual
    {
        public int Id => _id.Id;
        public void Execute()
        {
            Debug.Log($"SkillView Execute: {Id}");
            //実際のViewで起こる演出など

            if (_source == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(_cueName))
            {
                _source.Play();
                return;
            }

            Debug.Log($"Playing sound: {_cueName}");
            _source.Play(_cueName);
        }

        [SerializeField, SourceDataCollection("Skill"), Tooltip("表示するスキルのIDです。")]
        private DataID _id;

        [SerializeField, Tooltip("SkillSE再生用SoundEffectSource")] private SoundEffectSource _source;

        [SerializeField, Tooltip("再生するCueの名前")] private string _cueName;
    }
}
