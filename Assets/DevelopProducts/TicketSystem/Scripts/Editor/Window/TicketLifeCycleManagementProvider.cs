using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace DevelopProducts.TicketSystem
{
    public class TicketLifeCycleManagementProvider : SettingsProvider
    {
        private const string SETTINGS_PATH = "Project/TicketSystem/Management";
        private bool isLoading;
        private int currentTab;
        private string currentUserName;

        private TicketLifeCycleManagementProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) :
            base(path, scopes, keywords)
        {
        }

        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            return new TicketLifeCycleManagementProvider(SETTINGS_PATH, SettingsScope.Project);
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            currentUserName = TicketSystemSettings.instance.userName;
            base.OnActivate(searchContext, rootElement);
        }

        public override void OnGUI(string searchContext)
        {
            if (isLoading)
            {
                EditorGUILayout.LabelField("通信中...");
                return;
            }

            currentTab = GUILayout.Toolbar(currentTab, new[] { "チケットの発行", "チケットの返却" });

            switch (currentTab)
            {
                case 0: DrawCreateTab(); break;
                case 1: DrawDisposeTab(); break;
            }
        }

        private void DrawDisposeTab()
        {
            return;
        }

        /// <summary>
        /// チケットの新規作成用UI。現在アクティブなシーンの情報を表示して、そのシーンに対するチケットを発行するボタンを置く。
        /// </summary>
        private void DrawCreateTab()
        {
            var activeScene = SceneManager.GetActiveScene();
            EditorGUILayout.LabelField("対象シーン", activeScene.name);
            EditorGUILayout.LabelField("パス", activeScene.path);
            EditorGUILayout.LabelField("ユーザー名", currentUserName);

            if (GUILayout.Button("チケットを発行して使用開始", GUILayout.Height(40)))
            {
                isLoading = true;
                TicketSystemWebClient.CreateTicket(activeScene.name, activeScene.path, currentUserName)
                    .ContinueWith(_ => isLoading = false);
            }
        }
    }
}