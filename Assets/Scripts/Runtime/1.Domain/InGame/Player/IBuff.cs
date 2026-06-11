using System.Threading.Tasks;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Character;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace KillChord.Runtime.Domain
{
    public interface IBuff
    {
        BuffContext Execute(BuffContext context);
        ValueTask<BuffContext> ExecuteAsync(BuffContext context);
        BuffStateData GetState();
    }

    public readonly struct BuffContext
    {
        public BuffContext(CharacterEntity attaker, CharacterEntity target, AttackResult result = default)
        {
            _attacker = attaker;
            _target = target;
            _result = result;
        }

        public BuffContext(in BuffContext context)
        {
            _attacker = context._attacker;
            _target = context._target;
            _result = context._result;
        }

        public CharacterEntity Attacker => _attacker;
        public CharacterEntity Target => _target;
        public AttackResult AttackResult => _result;

        private readonly CharacterEntity _attacker;
        private readonly CharacterEntity _target;
        private readonly AttackResult _result;
    }
    public enum BuffExecuteType
    {
        Pre,
        Post,
    }
    public enum BuffType
    {
        Wait,
        Now,
    }
    public readonly struct BuffStateData
    {
        public BuffStateData(BuffExecuteType executeType, BuffType type)
        {
            _buffExecuteType = executeType;
            _buffType = type;
        }

        public BuffType GetBuffType() => _buffType;
        public BuffExecuteType GetBuffExecuteType() => _buffExecuteType;
        private readonly BuffExecuteType _buffExecuteType;
        private readonly BuffType _buffType;
    }
}
