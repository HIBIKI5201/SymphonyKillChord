using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
namespace DevelopProducts.TicketSystem
{
    /// <summary>
    /// チケットシステムのメインウィンドウを管理するクラス。
    /// ユーザー名の入力と保存、タブ切り替え、チケットの一覧表示UIを担当する。
    /// </summary>
    public class MasterTicketWindow : EditorWindow
    {
        private string savedUserName = "";
        private bool isLoading;
        private Vector2 scrollPos;

        private readonly Color emptyColor = new(0.2f, 0.6f, 0.2f, 0.3f);
        private readonly Color occupiedByOtherColor = new(0.6f, 0.2f, 0.2f, 0.3f);
        private readonly Color occupiedBySelfColor = new(0.2f, 0.4f, 0.6f, 0.3f);
        private readonly Vector2 minWindowSize = new(800f, 200f);

        // --- ウィンドウの描画部分 ---

        [MenuItem("Window/Master Ticket Window")]
        public static void ShowWindow()
        {
            GetWindow<MasterTicketWindow>("Master Ticket Window");
        }

        private void OnEnable()
        {
            minSize = minWindowSize;
            savedUserName = TicketSystemSettings.instance.userName;

            if (!string.IsNullOrEmpty(savedUserName))
            {
                UpdateTickets();
            }
        }

        private void UpdateTickets()
        {
            isLoading = true;
            TicketSystemWebClient.RefreshList().ContinueWith(() =>
            {
                isLoading = false;
                EditorApplication.delayCall += Repaint;
            });
        }

        private void OnGUI()
        {
            if (savedUserName != null && savedUserName != TicketSystemSettings.instance.userName)
            {
                savedUserName = TicketSystemSettings.instance.userName;
            }

            if (string.IsNullOrEmpty(savedUserName))
            {
                EditorGUILayout.HelpBox("ユーザー名を設定してください。設定するまでメイン機能は使えません。", MessageType.Warning);
                return;
            }

            DrawMainUI();
        }

        /// <summary>
        /// ユーザー名の表示と変更ボタン、タブ切り替えのUIを描画する。通信中は通信中のメッセージを表示して、タブの内容は描画しない。
        /// </summary>
        private void DrawMainUI()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.LabelField($"ユーザー: {savedUserName}");
            EditorGUILayout.EndHorizontal();

            if (isLoading)
            {
                EditorGUILayout.HelpBox("通信中...", MessageType.Info);
                return;
            }

            DrawListTab();
        }

        /// <summary>
        /// チケットの一覧表示用UI。GASから取得したticketListをループして、シーン名や状態、担当者、最終更新時刻などを表示する。
        /// チケットの状態に応じて行の背景色を変える。各チケットに対して、使用開始・解放の切り替えボタンと、そのシーンの位置まで移動するボタンを置く。
        /// </summary>
        private void DrawListTab()
        {
            if (GUILayout.Button("更新", GUILayout.Height(35)))
            {
                UpdateTickets();
            }

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Label("シーン名", GUILayout.Width(100));
            GUILayout.Label("状態", GUILayout.Width(60));
            GUILayout.Label("担当者", GUILayout.Width(100));
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
                var rowRect = EditorGUILayout.BeginHorizontal(GUILayout.Height(30));

                var rectColor = ticket.isInUse switch
                {
                    false => emptyColor,
                    true when ticket.userName == savedUserName => occupiedBySelfColor,
                    _ => occupiedByOtherColor
                };

                EditorGUI.DrawRect(rowRect, rectColor);
                GUILayout.Label(ticket.sceneName, GUILayout.Width(100));
                GUILayout.Label(ticket.isInUse ? "使用中" : "空き", GUILayout.Width(60));
                GUILayout.Label(ticket.userName, GUILayout.Width(100));
                GUILayout.Label(ticket.timestamp, GUILayout.Width(200));

                var isMyTicket = string.IsNullOrEmpty(ticket.userName) || (ticket.userName == savedUserName);

                // 他者のチケットを勝手に解放できないようにする。
                EditorGUI.BeginDisabledGroup(!isMyTicket && ticket.isInUse);

                if (GUILayout.Button(ticket.isInUse ? "解放する" : "使用する", GUILayout.Width(70)))
                {
                    isLoading = true;
                    TicketSystemWebClient.UpdateTicketStatus(ticket, savedUserName)
                        .ContinueWith(() =>
                        {
                            isLoading = false;
                            EditorApplication.delayCall += Repaint;
                        });
                }

                EditorGUI.EndDisabledGroup();

                if (GUILayout.Button("シーンの位置まで移動", GUILayout.Width(150)))
                {
                    JumpToAsset(ticket.masterPath);
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        // --- その他の機能 ---

        /// <summary>
        /// アセットのパスを受け取って、そのアセットをプロジェクトウィンドウで選択状態にする。
        /// </summary>
        /// <param name="assetPath"></param>
        private static void JumpToAsset(string assetPath)
        {
            var asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);

            if (asset != null)
            {
                EditorGUIUtility.PingObject(asset);
                Selection.activeObject = asset;
            }
            else
            {
                Debug.LogError($"アセットが見つかりませんでした: {assetPath}");
            }
        }
    }
}

#endif