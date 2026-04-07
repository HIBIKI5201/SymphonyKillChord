using KillChord.Runtime.Adaptor.InGame.Skill;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.InfraStructure.Player;
using UnityEngine;

namespace KillChord.Runtime.Composition
{
    public class SkillInitializer : MonoBehaviour
    {
        [SerializeField] private SkillRepository _skillRepository;
        private SkillController _skillController;

        public void Initialize(IMusicSyncService musicSyncService)
        {
            _skillController = new SkillController(_skillRepository, musicSyncService);
        }
    }
}