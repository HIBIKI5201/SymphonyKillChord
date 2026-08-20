using KillChord.Runtime.Domain.InGame.Character;

namespace KillChord.Runtime.Domain.InGame.Battle
{
    public interface IAttackController
    {
        /// <summary>
        ///     Applicationから攻撃処理を呼び出すためのもの。
        /// </summary>
        void Execute(int beatType, bool isJustHit);
        /// <summary>
        ///     攻撃対象を渡す。
        /// </summary>
        /// <param name="beatType"> </param>
        /// <param name="target"> 攻撃対象のキャラクターエンティティ </param>
        /// <param name="isJustHit"> ジャストヒットかどうか </param>
        void Execute(int beatType, CharacterEntity target, bool isJustHit);
    }
}