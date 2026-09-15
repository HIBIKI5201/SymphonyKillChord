using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using static KillChord.Editor.AIDebugPlay.AIDebugJson;

namespace KillChord.Editor.AIDebugPlay
{
    /// <summary>
    ///     LLMのQA操作に対して、Editor専用のJSON窓口を提供する。
    /// </summary>
    public static class AIDebugQaApi
    {
        /// <summary>
        ///     同一のEditor呼び出し内で状態を取得する。ゲームのロードや入力は行わない。
        /// </summary>
        public static string GetSnapshotJson(bool includeUi = true)
        {
            var scenes = new List<object>();
            for (int index = 0; index < SceneManager.sceneCount; index++)
            {
                var scene = SceneManager.GetSceneAt(index);
                scenes.Add(Object(("name", scene.name), ("path", scene.path), ("loaded", scene.isLoaded)));
            }
            var sections = Object();
            Add(sections, "player", AIDebugCombatSnapshot.ReadPlayer);
            Add(sections, "rhythm", AIDebugCombatSnapshot.ReadRhythm);
            Add(sections, "mission", AIDebugCombatSnapshot.ReadMission);
            Add(sections, "targets", AIDebugCombatSnapshot.ReadTargets);
            Add(sections, "sequence", AIDebugCombatSnapshot.ReadSequence);
            Add(sections, "save", AIDebugSaveSnapshot.Read);
            sections.Add("observedCombat", AIDebugQaMonitor.ReadCombat());
            if (includeUi) { Add(sections, "ui", AIDebugUi.Read); }
            return Serialize(Object(("schemaVersion", 1), ("success", true),
                ("environment", "UnityEditor"), ("projectPath", System.IO.Path.GetDirectoryName(Application.dataPath)),
                ("capturedAtUtc", DateTime.UtcNow.ToString("O")), ("frame", Time.frameCount),
                ("editorTimeSeconds", EditorApplication.timeSinceStartup), ("gameTimeSeconds", Time.timeAsDouble),
                ("isPlaying", EditorApplication.isPlaying), ("isEditorPaused", EditorApplication.isPaused),
                ("timeScale", Time.timeScale), ("activeScene", SceneManager.GetActiveScene().name),
                ("scenes", scenes), ("sections", sections)));
        }

        /// <summary>
        ///     UI操作を発行する。操作完了やQA合格は後続の観測で判定する。
        /// </summary>
        public static string InvokeButton(int instanceId, string expectedPath)
        {
            try { return Serialize(Object(("success", true), ("result", AIDebugUi.InvokeButton(instanceId, expectedPath)))); }
            catch (Exception exception) { return Serialize(Object(("success", false), ("message", exception.Message))); }
        }

        /// <summary>
        ///     EditModeで破棄済みのランタイムサービスを観測しないようにする。
        /// </summary>
        private static void Add(Dictionary<string, object> sections, string name, Func<object> read)
        {
            sections.Add(name, EditorApplication.isPlaying ? Observe(read) : Unavailable("Not in PlayMode"));
        }
    }
}
