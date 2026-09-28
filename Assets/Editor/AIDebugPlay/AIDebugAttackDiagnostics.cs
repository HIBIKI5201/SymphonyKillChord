using KillChord.Runtime.Composition.InGame.Player;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityApplication = UnityEngine.Application;

namespace KillChord.Editor.AIDebugPlay
{
    /// <summary>
    ///     攻撃キューが進まない場合の入力環境とプレイヤー状態をコピーする。
    /// </summary>
    internal static class AIDebugAttackDiagnostics
    {
        /// <summary>
        ///     設定やフォーカスを変更せずに入力停止の診断情報を取得する。
        /// </summary>
        /// <param name="player"> 入力抑制と攻撃状態を読むプレイヤー。未取得の場合はnull。 </param>
        /// <param name="pressedMouse"> キューが押下中のMouse。無い場合は現在のMouseを読む。 </param>
        /// <returns> 入力環境とプレイヤー状態の診断情報を表すJSONオブジェクト。 </returns>
        internal static object Read(PlayerModuleContainer player, Mouse pressedMouse)
        {
            var mouse = pressedMouse ?? Mouse.current;
            var focusedWindow = EditorWindow.focusedWindow;
            return AIDebugJson.Object(
                ("isPlaying", EditorApplication.isPlaying),
                ("isEditorPaused", EditorApplication.isPaused),
                ("timeScale", Time.timeScale),
                ("applicationHasFocus", UnityApplication.isFocused),
                ("focusedEditorWindow", focusedWindow != null ? focusedWindow.GetType().Name : null),
                ("inputUpdateTypeAtCapture", InputState.currentUpdateType.ToString()),
                ("inputUpdateMode", InputSystem.settings.updateMode.ToString()),
                ("backgroundBehavior", InputSystem.settings.backgroundBehavior.ToString()),
                ("editorInputBehavior", InputSystem.settings.editorInputBehaviorInPlayMode.ToString()),
                ("mouseDeviceId", mouse?.deviceId),
                ("mouseAdded", mouse != null && mouse.added),
                ("mouseEnabled", mouse != null && mouse.added && mouse.enabled),
                ("mouseButtonPressedAtCapture", mouse != null && mouse.added && mouse.leftButton.isPressed),
                ("isInputSuppressed", player?.InputSuppressionState?.IsSuppressed),
                ("isAttacking", player?.PlayerAttackController?.IsAttacking),
                ("isAttackCooldown", player?.PlayerAttackController?.IsAttackCooldown));
        }
    }
}
