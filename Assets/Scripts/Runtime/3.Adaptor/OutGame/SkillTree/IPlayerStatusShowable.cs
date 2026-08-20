using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.OutGame.SkillTree
{
    /// <summary>
    ///     プレイヤーステータス画面の表示用インタフェース。
    /// </summary>
    public interface IPlayerStatusShowable
    {
        Task Show(CancellationToken token);
    }
}
