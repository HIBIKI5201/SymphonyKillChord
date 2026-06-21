using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Buff;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.Player;
using UnityEngine;

namespace KillChord.Runtime.Application.Player.SkillEffect
{
    /// <summary>
    ///     スキルの基盤クラス。
    /// </summary>
    public class SkillBase : ISkillEffect
    {
        public SkillBase(IBuff buff)
        {
            _buff = buff;
        }
        public virtual void Execute(SkillEffectContext context){}

        protected IBuff _buff;
    }
}