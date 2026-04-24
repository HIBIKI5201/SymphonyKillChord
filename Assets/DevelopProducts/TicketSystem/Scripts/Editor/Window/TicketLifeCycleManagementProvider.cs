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
        private Vector2 scrollPos;

        private TicketLifeCycleManagementProvider(string path, SettingsScope scopes,
            IEnumerable<string> keywords = null)
            :
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
                EditorGUILayout.HelpBox("通信中...", MessageType.Info);
                return;
            }

            currentTab = GUILayout.Toolbar(currentTab, new[] { "チケットの発行", "チケットの破棄" });

            switch (currentTab)
            {
                case 0: DrawCreateTab(); break;
                case 1: DrawDisposeTab(); break;
            }
        }

        /// <summary>
        /// チケットの破棄用UI。現在キャッシュされているチケットの一覧を表示し、各チケットに対して破棄ボタンを置く。
        /// </summary>
        private void DrawDisposeTab()
        {
            EditorGUILayout.HelpBox("削除するチケットのシーンを選択", MessageType.Warning);

            if (GUILayout.Button("更新", GUILayout.Height(35)))
            {
                isLoading = true;
                TicketSystemWebClient.RefreshList().ContinueWith(_ => isLoading = false);
            }

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Label("シーン名", GUILayout.Width(100));
            GUILayout.Label("状態", GUILayout.Width(60));
            GUILayout.Label("最終更新時刻", GUILayout.Width(200));
            GUILayout.Label("操作", GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();

            if (CachedTicketDataSingleton.instance == null)
            {
                EditorGUILayout.HelpBox("チケットデータが利用できません。", MessageType.Warning);
                return;
            }

            var cachedTickets = CachedTicketDataSingleton.instance.GetAll();
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            foreach (var ticket in cachedTickets)
            {
                EditorGUILayout.BeginHorizontal(GUILayout.Height(20));
                GUILayout.Label(ticket.sceneName, GUILayout.Width(100));
                GUILayout.Label(ticket.isInUse ? "使用中" : "空き", GUILayout.Width(60));
                GUILayout.Label(ticket.timestamp, GUILayout.Width(200));
                if (GUILayout.Button("破棄", GUILayout.Width(200)))
                {
                    var result = EditorDialog.DisplayDecisionDialog(
                        "チケット破棄の確認",
                        $"シーン: [{ticket.sceneName}] のチケットを破棄しますか？\nこの操作は元に戻せません。",
                        "破棄する",
                        "キャンセル");

                    if (!result) return;
                    isLoading = true;
                    TicketSystemWebClient.DisposeTicket(ticket.sceneName)
                        .ContinueWith(_ => isLoading = false);

                    break;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// チケットの新規作成用UI。現在アクティブなシーンの情報を表示して、そのシーンに対するチケットを発行するボタンを置く。
        /// </summary>
        private void DrawCreateTab()
        {
            EditorGUILayout.LabelField("新しく作成するチケットの情報", EditorStyles.boldLabel);
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