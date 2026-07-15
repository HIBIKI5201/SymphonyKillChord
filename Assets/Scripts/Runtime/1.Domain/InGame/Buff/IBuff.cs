using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Character;

namespace KillChord.Runtime.Domain.InGame.Buff
{
    public interface IBuff
    {  
        /// <summary>
        ///     即時発動バフ
        /// </summary>
        /// <param name="context"> バフ対象 </param>
        /// <returns></returns>
        BuffContext ExecuteInstance(BuffContext context);
        /// <summary>
        ///     継続発動バフ
        /// </summary>
        /// <param name="context"> バフ対象 </param>
        /// <returns></returns>
        ValueTask ExecuteAsync(BuffContext context,CancellationToken token);
        /// <summary>
        ///     
        /// </summary>
        /// <returns></returns>
        BuffMetaData GetState();
    }

    public readonly struct BuffContext
    {
        public BuffContext(CharacterEntity attacker, CharacterEntity target, AttackResult result = default)
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
    /// <summary>
    ///     バフの発動タイミングのタイプ。
    /// </summary>
    public enum BuffExecuteTiming
    {
        /// <summary>
        ///     攻撃計算前に発動。
        /// </summary>
        Attack_Logic_Before,
        /// <summary>
        ///     攻撃計算後に発動。
        /// </summary>
        Attack_Logic_After,
        /// <summary>
        ///     スキル発動時に発動。
        /// </summary>
        Skill,
    }

    /// <summary>
    ///     バフのタイプクラスをまとめたデータ。
    /// </summary>
    public readonly struct BuffMetaData
    {
        public BuffMetaData(BuffExecuteTiming timing)
        {
            _executeTimingType = timing;
        }

        public BuffExecuteTiming GetActivationType() => _executeTimingType;
        private readonly BuffExecuteTiming _executeTimingType;

    }
}
