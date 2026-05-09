using CriWare;
using KillChord.Runtime.Adaptor.InGame.Skill;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Skill
{
    public class SkillView : MonoBehaviour, ISkillVisual
    {
        public int Id => _id;
        public void Execute()
        {
            Debug.Log($"SkillView Execute: {Id}");
            //実際のViewで起こる演出など
            if (_source == null)
            {
                _source = this.gameObject.GetComponent<CriAtomSource>();
            }

            if (_source != null && !string.IsNullOrEmpty(_cueName))
            {
                Debug.Log($"Playing sound: {_cueName}");
                _source.Play(_cueName);
            }
        }

        [SerializeField] private int _id;

        [SerializeField, Tooltip("SkillSE再生用CriAtomSource")] private CriAtomSource _source;

        [SerializeField, Tooltip("再生するCueの名前")] private string _cueName;
    }
}
