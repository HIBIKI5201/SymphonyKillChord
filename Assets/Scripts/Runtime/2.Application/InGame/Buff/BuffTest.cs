using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KillChord.Runtime.Domain;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Skill;
using UnityEngine.Assertions.Must;

namespace KillChord.Runtime.Application.InGame.Buff
{
    public class BuffTest : IBuff
    {
        public BuffContext Execute(BuffContext context)
        {
            context.Attacker.ChangeBaseDamage(context.Attacker.BaseDamage * _multiPiler);
             return new BuffContext(context.Attacker,context.Target,new Domain.InGame.Battle.AttackResult(context.AttackResult.FinalDamage * 2,context.AttackResult.IsCritical));
        }


        public async ValueTask<BuffContext> ExecuteAsync(BuffContext context)
        {
            context.Attacker.ChangeBaseDamage(context.Attacker.BaseDamage * _multiPiler);

            await Task.Delay(System.TimeSpan.FromSeconds(_waitTime));

            context.Attacker.ChangeBaseDamage(context.Attacker.BaseDamage / _multiPiler);
            
            return context;
        }

        public BuffMetaData GetState()
        {
            return _status;
        }

        private readonly BuffMetaData _status = new BuffMetaData(BuffExecuteTiming.Before,BuffActivationType.Duration);
        private readonly float _multiPiler = 2f;
        private readonly float _waitTime = 5f;
    }
}
