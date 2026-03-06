using UnityEngine;

namespace Mock.MusicBattle.Player
{
    public interface ISpecialAttackModule
    {
        public void Execute(SpecialAttackDTO dto);
        public void Assert(Object context) { }
    }
}
