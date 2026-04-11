using System;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition
{
    public class StageSceneInstance : MonoBehaviour, IStageSceneInstance
    {
        public Transform PlayerTransform => _playerTransform;
        public SkillInitializer SkillInitializer => _skillInitializer;

        [SerializeField] private Transform _playerTransform;
        [SerializeField] private SkillInitializer _skillInitializer;

        private void Awake()
        {
            ServiceLocator.RegisterInstance<IStageSceneInstance>(this);
        }
    }

    public interface IStageSceneInstance
    {
        Transform PlayerTransform { get; }
        SkillInitializer SkillInitializer { get; }
    }
}