using KillChord.Runtime.Adaptor.InGame.Skill;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.InfraStructure.Player;
using SymphonyFrameWork;
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
            _skillController = new SkillController(_skillRepository, _musicSyncService);
        }

        public void Inject(IMusicSyncService arg0)
        {
        }
    }
}