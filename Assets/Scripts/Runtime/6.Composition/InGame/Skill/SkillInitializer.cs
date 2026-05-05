using KillChord.Runtime.Adaptor.InGame.Skill;
using KillChord.Runtime.Application;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Application.InGame.Skill;
using KillChord.Runtime.InfraStructure.Player;
using KillChord.Runtime.View;
using SymphonyFrameWork;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using UnityEngine;

namespace KillChord.Runtime.Composition
{
    public class SkillInitializer : MonoBehaviour, IInjectable<IMusicSyncService>
    {
        [SerializeField] private SkillRepository _skillRepository;
        [SerializeField] private SkillView[] _skillVisuals;
        private SkillController _skillController;
        private IMusicSyncService _musicSyncService;

        public void Initialize()
        {
            SkillCheckService skillCheckService = new SkillCheckService();
            ISkillVisual[] visuals = _skillVisuals;
            _skillController = new SkillController(_skillRepository, visuals, null, null);
            SkillUsecase skillUsecase = new SkillUsecase(_musicSyncService, skillCheckService, _skillController);
            _skillController.SetUsecase(skillUsecase);
        }

        public void Inject(IMusicSyncService arg0)
        {
            _musicSyncService = arg0;
        }
    }
}