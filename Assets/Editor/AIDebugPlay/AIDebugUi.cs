using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static KillChord.Editor.AIDebugPlay.AIDebugJson;

namespace KillChord.Editor.AIDebugPlay
{
    /// <summary>
    ///     シーン上のUIを列挙し、明示されたボタンの操作を仲介する。
    /// </summary>
    internal static class AIDebugUi
    {
        /// <summary>
        ///     有効なボタンとテキストを取得する。表示の正しさは画像と併せて確認する。
        /// </summary>
        internal static object Read()
        {
            var buttons = new List<object>();
            var texts = new List<object>();
            int buttonCount = 0;
            int textCount = 0;
            foreach (var button in UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.InstanceID))
            {
                if (!IsSceneComponent(button)) { continue; }
                buttonCount++;
                if (buttons.Count >= MAX_ELEMENTS) { continue; }
                buttons.Add(Object(("instanceId", button.GetInstanceID()), ("path", Path(button.transform)),
                    ("interactable", button.IsInteractable()), ("active", button.isActiveAndEnabled)));
            }
            foreach (var label in UnityEngine.Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.InstanceID))
            {
                if (!IsSceneComponent(label) || !label.isActiveAndEnabled) { continue; }
                textCount++;
                if (texts.Count < MAX_ELEMENTS)
                {
                    texts.Add(Object(("path", Path(label.transform)), ("text", Truncate(label.text))));
                }
            }
            foreach (var label in UnityEngine.Object.FindObjectsByType<Text>(FindObjectsSortMode.InstanceID))
            {
                if (!IsSceneComponent(label) || !label.isActiveAndEnabled) { continue; }
                textCount++;
                if (texts.Count < MAX_ELEMENTS)
                {
                    texts.Add(Object(("path", Path(label.transform)), ("text", Truncate(label.text))));
                }
            }
            return Object(("buttons", buttons), ("texts", texts), ("buttonCount", buttonCount),
                ("textCount", textCount), ("truncated", buttonCount > MAX_ELEMENTS || textCount > MAX_ELEMENTS),
                ("visibility", "activeHierarchyOnly; occlusion and canvas alpha require screenshot"));
        }

        /// <summary>
        ///     取得済みIDと階層パスが一致するボタンのイベントを一度呼ぶ。
        /// </summary>
        internal static object InvokeButton(int instanceId, string expectedPath)
        {
            if (!EditorApplication.isPlaying || EditorApplication.isPaused)
            {
                throw new InvalidOperationException("An unpaused PlayMode session is required");
            }
            var button = EditorUtility.InstanceIDToObject(instanceId) as Button;
            if (button == null || !IsSceneComponent(button) || !button.isActiveAndEnabled
                || !button.IsInteractable() || Path(button.transform) != expectedPath)
            {
                throw new InvalidOperationException("Button changed or is not interactable; obtain a fresh snapshot");
            }
            button.onClick.Invoke();
            return Object(("dispatched", true), ("method", "Button.onClick"),
                ("inputDeviceVerified", false), ("completionVerified", false));
        }

        /// <summary>
        ///     アセットや未ロードのオブジェクトを対象から除外する。
        /// </summary>
        private static bool IsSceneComponent(Component component)
        {
            return component.gameObject.scene.IsValid() && component.gameObject.scene.isLoaded;
        }

        /// <summary>
        ///     シーン名と兄弟番号を含めてボタンの識別情報を作る。
        /// </summary>
        private static string Path(Transform transform)
        {
            string path = transform.name + "[" + transform.GetSiblingIndex() + "]";
            for (Transform parent = transform.parent; parent != null; parent = parent.parent)
            {
                path = parent.name + "[" + parent.GetSiblingIndex() + "]/" + path;
            }
            return transform.gameObject.scene.path + ":" + path;
        }

        /// <summary>
        ///     長いテキストによる応答の肥大化を防ぐ。
        /// </summary>
        private static string Truncate(string value)
        {
            return value != null && value.Length > MAX_TEXT_LENGTH
                ? value.Substring(0, MAX_TEXT_LENGTH) + "[truncated]" : value;
        }

        private const int MAX_ELEMENTS = 256;
        private const int MAX_TEXT_LENGTH = 2048;
    }
}
