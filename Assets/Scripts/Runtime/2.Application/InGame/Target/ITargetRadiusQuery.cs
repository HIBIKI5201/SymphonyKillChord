using KillChord.Runtime.Domain.InGame.Character;
using System.Collections.Generic;

namespace KillChord.Runtime.Application.InGame.Target
{
    /// <summary>
    ///     指定した対象を中心とした範囲内のターゲットを検索するためのインターフェース。
    /// </summary>
    public interface ITargetRadiusQuery
    {
        /// <summary>
        ///     指定した中心のキャラクターを中心に、指定した範囲内のターゲットを検索し、結果を result リストに格納します。
        /// </summary>
        /// <param name="center"> 中心となるキャラクターです。 </param>
        /// <param name="range"> 検索範囲の半径です。 </param>
        /// <param name="results"> 検索結果を格納するリストです。 </param>
        void Query(CharacterEntity center, float range, List<CharacterEntity> results);
    }
}
