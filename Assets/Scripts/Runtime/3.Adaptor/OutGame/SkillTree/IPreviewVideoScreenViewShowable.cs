using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.OutGame.SkillTree
{
    /// <summary>
    ///     スキルプレイビュー画面の表示用インタフェース。
    /// </summary>
    public interface IPreviewVideoScreenViewShowable
    {
        Task Show(CancellationToken token);
    }
}
