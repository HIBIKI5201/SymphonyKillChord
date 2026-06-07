using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.Player;
using UnityEngine;

namespace KillChord.Runtime.Application.Player.SkillEffect
{
    /// <summary>
    ///     スキルの効果をテストするためのクラス。
    /// </summary>
    public class Skill_06 : ISkillEffect
    {
        public void Execute(SkillEffectContext context)
        {
            
        }

        private float _m = 5f; // ダメージ倍率。
        private float _n = 0.3f; // 体力消費量。
    }
}