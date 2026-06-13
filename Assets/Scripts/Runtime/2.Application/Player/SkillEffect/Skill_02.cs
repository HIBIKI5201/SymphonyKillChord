using System;
using KillChord.Runtime.Domain;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.Player;


namespace KillChord.Runtime.Application.Player.SkillEffect
{
    /// <summary>
    ///   スキルID 02 のスキル効果を実装するクラス。 
    /// </summary>
    public class Skill_02 : SkillBase
    {
        
        public Skill_02(IBuff buff) : base(buff)
        {
        }
        public override void Execute(SkillEffectContext context)
        {
             _attackController.Execute((int)_shotGunBeat); //ショットガン処理を実行する。
             var targets = context.Repository.FindByRule(); //ショットガンにヒットしたキャラクタを取得する。
             foreach(var target in targets) target.BuffSystem.Add(_buff);

        }
        private float _multiplier = 0.9f; //強力な一回攻撃。
        private BeatType _shotGunBeat = BeatType.Two;
        private IAttackController _attackController;
    }
}