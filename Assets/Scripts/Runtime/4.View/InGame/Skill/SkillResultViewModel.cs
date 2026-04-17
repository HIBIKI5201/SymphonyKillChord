using KillChord.Runtime.Adaptor;
using KillChord.Runtime.Domain.InGame.Music;
using System;

namespace KillChord.Runtime.View
{
    public class SkillResultViewModel : ISkillResultViewModel
    {
        public event Action<int, ReadOnlyMemory<BeatType>> OnChanged;

        public int SkillId { get; private set; }

        public ReadOnlyMemory<BeatType> SkillPattern { get; private set; }

        public void Push(in SkillResultDTO dto)
        {
            SkillId = dto.SkillId;
            SkillPattern = dto.SkillPattern;
            OnChanged?.Invoke(SkillId, SkillPattern);
        }
    }
}
