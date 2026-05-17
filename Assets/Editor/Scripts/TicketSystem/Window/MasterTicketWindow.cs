using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace KillChord.Editor.TicketSystem
{
    /// <summary>
    ///     チケットシステムのメインウィンドウを管理するクラス。
    ///     ユーザー名の入力と保存、タブ切り替え、チケットの一覧表示UIを担当する。
    /// </summary>
    public class MasterTicketWindow : EditorWindow
    {
        // --- ウィンドウの描画部分 ---

        [MenuItem("Window/Master Ticket Window")]
        public static void ShowWindow()
        {
            GetWindow<MasterTicketWindow>("Master Ticket Window");
        }

        private readonly Color _emptyColor = new(0.2f, 0.6f, 0.2f, 0.3f);
        private readonly Color _occupiedByOtherColor = new(0.6f, 0.2f, 0.2f, 0.3f);
        private readonly Color _occupiedBySelfColor = new(0.2f, 0.4f, 0.6f, 0.3f);
        private readonly Vector2 _minWindowSize = new(800f, 200f);

        private string _savedUserName = "";
        private bool _isLoading;
        private Vector2 _scrollPos;

        private void OnEnable()
        {
            minSize = _minWindowSize;
            _savedUserName = TicketSystemSettings.instance.UserName;

            if (!string.IsNullOrEmpty(_savedUserName))
            {
                UpdateTickets();
            }
        }

        private void OnGUI()
        {
            var currentUserName = TicketSystemSettings.instance.UserName;
            if (currentUserName != _savedUserName)
            {
                var isUserNameEmpty = string.IsNullOrEmpty(_savedUserName);
                _savedUserName = currentUserName;
                if (isUserNameEmpty && !string.IsNullOrEmpty(_savedUserName))
                {
                    // ユーザー名が新たに設定された場合は、チケットデータを更新する。
                    UpdateTickets();
                }
            }

            if (string.IsNullOrEmpty(_savedUserName))
            {
                EditorGUILayout.HelpBox("ユーザー名を設定してください。設定するまでメイン機能は使えません。", MessageType.Warning);
                return;
            }

            DrawMainUI();
        }

        private void UpdateTickets()
        {
            _isLoading = true;
            TicketSystemWebClient.RefreshList().ContinueWith(() =>
            {
                _isLoading = false;
                EditorApplication.delayCall += Repaint;
            });
        }

        /// <summary>
        /// ユーザー名の表示と変更ボタン、タブ切り替えのUIを描画する。通信中は通信中のメッセージを表示して、タブの内容は描画しない。
        /// </summary>
        private void DrawMainUI()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.LabelField($"ユーザー: {_savedUserName}");
            EditorGUILayout.EndHorizontal();

            if (_isLoading)
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

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            foreach (var ticket in cachedTickets)
            {
                var rowRect = EditorGUILayout.BeginHorizontal(GUILayout.Height(30));

                var rectColor = ticket.isInUse switch
                {
                    false => _emptyColor,
                    true when ticket.userName == _savedUserName => _occupiedBySelfColor,
                    _ => _occupiedByOtherColor
                };

                EditorGUI.DrawRect(rowRect, rectColor);
                GUILayout.Label(ticket.sceneName, GUILayout.Width(100));
                GUILayout.Label(ticket.isInUse ? "使用中" : "空き", GUILayout.Width(60));
                GUILayout.Label(ticket.userName, GUILayout.Width(100));
                GUILayout.Label(ticket.timestamp, GUILayout.Width(200));

                var isMyTicket = string.IsNullOrEmpty(ticket.userName) || (ticket.userName == _savedUserName);

                // 他者のチケットを勝手に解放できないようにする。
                EditorGUI.BeginDisabledGroup(!isMyTicket && ticket.isInUse);

                if (GUILayout.Button(ticket.isInUse ? "解放する" : "使用する", GUILayout.Width(70)))
                {
                    _isLoading = true;
                    TicketSystemWebClient.UpdateTicketStatus(ticket, _savedUserName)
                        .ContinueWith(() =>
                        {
                            _isLoading = false;
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