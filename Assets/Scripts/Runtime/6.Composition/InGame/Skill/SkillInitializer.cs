using System;
using KillChord.Runtime.Adaptor.InGame.Skill;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Application.InGame.Skill;
using KillChord.Runtime.InfraStructure.Player;
using SymphonyFrameWork;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition
{
    public class SkillInitializer : MonoBehaviour, IInjectable<IMusicSyncService>
    {
        [SerializeField] private SkillRepository _skillRepository;
        private SkillController _skillController;
        private IMusicSyncService _musicSyncService;

        public void Initialize()
        {
            SkillCheckService skillCheckService = new SkillCheckService();
            _skillController = new SkillController(_skillRepository, _musicSyncService, skillCheckService);
        }

        public void Inject(IMusicSyncService arg0)
        {
            _musicSyncService = arg0;
        }
    }
}