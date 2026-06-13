using System.Threading.Tasks;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Character;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace KillChord.Runtime.Domain
{
    public interface IBuff
    {  
        /// <summary>
        ///     即時発動バフ
        /// </summary>
        /// <param name="context"> バフ対象 </param>
        /// <returns></returns>
        BuffContext Execute(BuffContext context);
        /// <summary>
        ///     継続発動バフ
        /// </summary>
        /// <param name="context"> バフ対象 </param>
        /// <returns></returns>
        ValueTask<BuffContext> ExecuteAsync(BuffContext context);
        /// <summary>
        ///     
        /// </summary>
        /// <returns></returns>
        BuffMetaData GetState();
    }

    public readonly struct BuffContext
    {
        public BuffContext(CharacterEntity attaker, CharacterEntity target, AttackResult result = default)
        {
            _attacker = attaker;
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
        Before,
        /// <summary>
        ///     攻撃計算後に発動。
        /// </summary>
        After,
    }
    /// <summary>
    /// バフのタイプ
    /// </summary>
    public enum BuffActivationType
    {
        /// <summary>
        ///     継続発動。
        /// </summary>
        Duration,
        /// <summary>
        ///     即時発動。
        /// </summary>
        Instance,
    }
    /// <summary>
    ///     バフのタイプクラスをまとめたデータ。
    /// </summary>
    public readonly struct BuffMetaData
    {
        public BuffMetaData(BuffExecuteTiming timing, BuffActivationType activation)
        {
            _executeTimingType = timing;
            _activationType = activation;
        }

        public BuffActivationType GetTimingType() => _activationType;
        public BuffExecuteTiming GetActivationType() => _executeTimingType;
        private readonly BuffExecuteTiming _executeTimingType;
        private readonly BuffActivationType _activationType;
    }
}
