using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.OutGame.StageSelect
{
    /// <summary>
    ///     ステージノード間の接続を表す ViewModel のインターフェース。
    /// </summary>
    public interface IStageConnectionViewModel
    {
        /// <summary>
        ///     接続線の塗りつぶしアニメーションを再生します。
        /// </summary>
        /// <param name="token"> キャンセルトークン。</param>
        /// <returns> アニメーションの再生が完了するタスク。</returns>
        Task PlayFillAnimationAsync(CancellationToken token);
    }
}
