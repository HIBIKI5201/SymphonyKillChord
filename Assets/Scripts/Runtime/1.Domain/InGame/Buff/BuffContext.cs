using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Character;

namespace KillChord.Runtime.Domain.InGame.Buff
{
    public readonly struct BuffContext
    {
        public BuffContext(
            CharacterEntity attacker,
            CharacterEntity target,
            AttackResult result = default)
        {
            _attacker = attacker;
            _target = target;
            _result = result;
        }

        public BuffContext(BuffContext context)
        {
            _attacker = context.Attacker;
            _target = context.Target;
            _result = context._result;
        }

        public CharacterEntity Attacker => _attacker;
        public CharacterEntity Target => _target;
        public AttackResult AttackResult => _result;

        private readonly CharacterEntity _attacker;
        private readonly CharacterEntity _target;
        private readonly AttackResult _result;
    }
}
