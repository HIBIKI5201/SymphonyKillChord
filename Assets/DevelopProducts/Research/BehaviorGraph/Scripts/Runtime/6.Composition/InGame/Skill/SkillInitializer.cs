using System;
using DevelopProducts.BehaviorGraph.Runtime.Adaptor.InGame.Skill;
using DevelopProducts.BehaviorGraph.Runtime.Application.InGame.Music;
using KillChord.Runtime.InfraStructure.Player;
using SymphonyFrameWork;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace DevelopProducts.BehaviorGraph.Runtime.Composition
{
    public class SkillInitializer : MonoBehaviour, IInjectable<IMusicSyncService>
    {
        [SerializeField] private SkillRepository _skillRepository;
        private SkillController _skillController;
        private IMusicSyncService _musicSyncService;
        
        public void Initialize()
        {
            _skillController = new SkillController(_musicSyncService);
        }

        public void Inject(IMusicSyncService arg0)
        {
            _musicSyncService = arg0;
        }
    }
}