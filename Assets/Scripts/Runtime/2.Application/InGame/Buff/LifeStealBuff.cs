
using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Domain;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Buff;
using KillChord.Runtime.Domain.InGame.Character;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Buff
{
    public class LifeStealBuff : BuffBase
    {
        public LifeStealBuff(BuffMetaData buffMetaData) : base(buffMetaData)
        {

        }

        public override async ValueTask ExecuteAsync(BuffContext context,CancellationToken token)
        {
            SetBuffContext(context);
            context.Attacker.OnSetDamage += LifeSteal;
            try
            {
                await Task.Delay(_duration,token);
            }
            finally
            {
                context.Attacker.OnSetDamage -= LifeSteal;
                SetBuffContext(default);
            }
        }

        private void LifeSteal(Damage damage)
        {
            Health lifeStealValue = new Health(Mathf.Min(_damage.Value * _lifeStealMultiPlier,_maxHealValue));
            _buffContext.Attacker.Heal(lifeStealValue);
        }
        private void SetBuffContext(BuffContext context)
        {
            _buffContext = context;
        }
        private Damage _damage;
        private BuffContext _buffContext;
        private readonly float _lifeStealMultiPlier = 0.1f;
        private readonly float _maxHealValue = 5f;
        private readonly int _duration = 5000;
    }
}
