using UnityEngine;

namespace KillChord.Runtime.Domain.Persistent.Savedata
{
    public class SavedataEntity
    {
        public SavedataSkillUnlock SkillUnlock => _skillUnlock;

        private SavedataSkillUnlock _skillUnlock;
    }
}
