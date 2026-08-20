using KillChord.Runtime.Domain.InGame.Character;

namespace KillChord.Runtime.Domain.InGame.Battle
{
    public interface IAttackController
    {
        /// <summary>
        ///     Applicationから攻撃処理を呼び出すためのもの。
        /// </summary>
        void Execute(int beatType);
        /// <summary>
        ///     攻撃対象を渡す。
        /// </summary>
        /// <param name="beatType"></param>
        /// <param name="target"></param>
        void Execute(int beatType,CharacterEntity target);
    }
}