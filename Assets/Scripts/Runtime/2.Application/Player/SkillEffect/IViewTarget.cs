namespace KillChord.Runtime.Application.Player.SkillEffect
{
    
    public interface IViewTarget
    {
        /// <summary>
        ///     Viewのアクション。
        /// </summary>
        /// <param name="baseDamage">スキル側の基礎攻撃力</param>
        public void Execute(float baseDamage);
    }
}