
using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Domain;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Buff;
using KillChord.Runtime.Domain.InGame.Character;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Buff
{
    public class DamageDownBuff : BuffBase
    {
        public DamageDownBuff(BuffMetaData buffMetaData) : base(buffMetaData)
        {

        }

        public override BuffContext ExecuteInstance(BuffContext context)
        {
            Damage downDamageValue = context.Target.BaseDamage * _downMultiPlier;
            downDamageValue = new(Mathf.Min(downDamageValue.Value,_downMaxValue));
            context.Target.ChangeBaseDamage(downDamageValue);
            return context;
        }
        public override async ValueTask ExecuteAsync(BuffContext context,CancellationToken token)
        {
            
        }

        private float _downMaxValue = 10f;
        private float _downMultiPlier = 0.1f;
    }
}
