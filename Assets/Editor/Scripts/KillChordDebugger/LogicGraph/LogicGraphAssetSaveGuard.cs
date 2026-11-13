using System;
using UnityEditor;

namespace KillChord.Editor.Debugger.LogicGraph
{
    /// <summary>
    ///     アセットの保存直前を知らせる。
    ///     <para> 記録用に包んだステップがアセットへ書き込まれないよう、ロジックパネルが保存の前に元へ戻すために使う。 </para>
    /// </summary>
    public sealed class LogicGraphAssetSaveGuard : AssetModificationProcessor
    {
        /// <summary> アセットを保存する直前に呼ばれる。 </summary>
        public static event Action OnWillSave;

        /// <summary>
        ///     保存の直前に通知を出す。保存対象は変えない。
        /// </summary>
        /// <param name="paths"> 保存されるアセットのパス。 </param>
        /// <returns> 保存するアセットのパス（そのまま返す）。 </returns>
        private static string[] OnWillSaveAssets(string[] paths)
        {
            OnWillSave?.Invoke();
            return paths;
        }
    }
}
