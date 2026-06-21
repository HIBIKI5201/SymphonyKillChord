using System.Threading.Tasks;
using KillChord.Runtime.Domain.InGame.Character;
using UnityEngine;

namespace KillChord.Runtime.Domain.Player
{
    /// <summary>
    ///  条件を満たすものを返す。
    /// </summary>
    public interface IViewRepository
    {
        public CharacterEntity[] FindByRule();
    }

}
