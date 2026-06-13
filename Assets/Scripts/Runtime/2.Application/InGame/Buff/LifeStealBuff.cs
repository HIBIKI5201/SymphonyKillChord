using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KillChord.Runtime.Domain;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Skill;
using UnityEngine.Assertions.Must;

namespace KillChord.Runtime.Application.InGame.Buff
{
    public class LifeStealBuff : IBuff
    {
        public BuffContext Execute(BuffContext context)
        {
            throw new System.NotImplementedException();
        }

        public ValueTask<BuffContext> ExecuteAsync(BuffContext context)
        {
            throw new System.NotImplementedException();
        }

        public BuffMetaData GetState()
        {
            throw new System.NotImplementedException();
        }
    }
}
