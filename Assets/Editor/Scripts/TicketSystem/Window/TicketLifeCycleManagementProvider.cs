using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace KillChord.Editor.TicketSystem
{
    /// <summary>
    ///     Project Settings にチケットの作成・削除などを行う管理ページを提供する。
    /// </summary>
    public class TicketLifeCycleManagementProvider : SettingsProvider
    {
        /// <summary>
        ///     チケット管理の設定プロバイダーを生成する。
        /// </summary>
        public TicketLifeCycleManagementProvider(string path, SettingsScope scopes,
            IEnumerable<string> keywords = null)
            :
            base(path, scopes, keywords)
        {
        }

        /// <summary>
        ///     Project Settings にチケット管理ページを登録する。
        /// </summary>
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            return new TicketLifeCycleManagementProvider(SETTINGS_PATH, SettingsScope.Project);
        }

        /// <summary>
        ///     ページを開いたときに現在の利用者名を取得する。
        /// </summary>
        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            _currentUserName = TicketSystemSettings.instance.UserName;
            base.OnActivate(searchContext, rootElement);
        }

        /// <summary>
        ///     チケットの発行・破棄タブを切り替えて描画する。
        /// </summary>
        public override void OnGUI(string searchContext)
        {
            if (_isLoading)
            {
                EditorGUILayout.HelpBox("通信中...", MessageType.Info);
                return;
            }

            _currentTab = GUILayout.Toolbar(_currentTab, new[] { "チケットの発行", "チケットの破棄" });

            switch (_currentTab)
            {
                case 0: DrawCreateTab(); break;
                case 1: DrawDisposeTab(); break;
            }
        }

        private const string SETTINGS_PATH = TicketSystemConst.TICKET_SYSTEM_PROJECT_PATH + "Management";
        private bool _isLoading;
        private int _currentTab;
        private string _currentUserName;
        private Vector2 _scrollPos;

        /// <summary>
        /// チケットの破棄用UI。現在キャッシュされているチケットの一覧を表示し、各チケットに対して破棄ボタンを置く。
        /// </summary>
        private void DrawDisposeTab()
        {
            EditorGUILayout.HelpBox("削除するチケットのシーンを選択", MessageType.Warning);

            // チケット一覧を再取得するボタン。
            if (GUILayout.Button("更新", GUILayout.Height(35)))
            {
                _isLoading = true;
                TicketSystemWebClient.RefreshList().ContinueWith(() => _isLoading = false);
            }

            EditorGUILayout.Space();

            // 一覧の見出し。
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("シーン名", GUILayout.Width(100));
                GUILayout.Label("状態", GUILayout.Width(60));
                GUILayout.Label("最終更新時刻", GUILayout.Width(200));
                GUILayout.Label("操作", GUILayout.ExpandWidth(true));
            }

            if (CachedTicketDataSingleton.instance == null)
            {
                EditorGUILayout.HelpBox("チケットデータが利用できません。", MessageType.Warning);
                return;
            }

            // チケットごとに状態と破棄ボタンを表示する。
            IReadOnlyList<TicketData> cachedTickets = CachedTicketDataSingleton.instance.GetAll();
            using (var scrollViewScope = new EditorGUILayout.ScrollViewScope(_scrollPos))
            {
                _scrollPos = scrollViewScope.scrollPosition;

                foreach (var ticket in cachedTickets)
                {
                    bool isDispose;
                    using (new EditorGUILayout.HorizontalScope(GUILayout.Height(20)))
                    {
                        GUILayout.Label(ticket.sceneName, GUILayout.Width(100));
                        GUILayout.Label(ticket.isInUse ? "使用中" : "空き", GUILayout.Width(60));
                        GUILayout.Label(ticket.timestamp, GUILayout.Width(200));
                        isDispose = GUILayout.Button("破棄", GUILayout.Width(200));
                    }

                    // 破棄ボタンが押されたら確認してからサーバーへ破棄を要求する。
                    if (!isDispose) continue;
                    var result = EditorDialog.DisplayDecisionDialog(
                        "チケット破棄の確認",
                        $"シーン: [{ticket.sceneName}] のチケットを破棄しますか？\nこの操作は元に戻せません。",
                        "破棄する",
                        "キャンセル");

                    if (result)
                    {
                        _isLoading = true;
                        TicketSystemWebClient.DisposeTicket(ticket.sceneName)
                            .ContinueWith(() =>
                            {
                                _isLoading = false;
                                EditorApplication.delayCall += Repaint;
                            });
                    }

                    break;
                }
            }
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
            EditorGUILayout.LabelField("ユーザー名", _currentUserName);

            if (GUILayout.Button("チケットを発行して使用開始", GUILayout.Height(40)))
            {
                _isLoading = true;
                TicketSystemWebClient.CreateTicket(activeScene.name, activeScene.path, _currentUserName)
                    .ContinueWith(() =>
                    {
                        _isLoading = false;
                        EditorApplication.delayCall += Repaint;
                    });
            }
        }
    }
}