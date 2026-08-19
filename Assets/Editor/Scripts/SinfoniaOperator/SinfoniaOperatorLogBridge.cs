using SinfoniaStudio.SinfoniaOperator;
using UnityEditor;
using UnityEngine;

namespace KillChord.Editor.SinfoniaOperator
{
    /// <summary>
    ///     SinfoniaOperator.CoreのログをUnityのコンソールへ転送するクラス。
    /// </summary>
    [InitializeOnLoad]
    internal static class SinfoniaOperatorLogBridge
    {
        static SinfoniaOperatorLogBridge()
        {
            OperatorLog.SetWriter(static message => Debug.Log($"[SinfoniaOperator] {message}"));
        }
    }
}
