using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
namespace DevelopProducts.TicketSystem
{
    public class MasterTicketWindow : EditorWindow
    {
        // --- 設定項目 ---
        private const string GAS_URL =
            "https://script.google.com/macros/s/AKfycbxbz52y03Es5s6tzkHP7CUUFEPQR4KWhoaV0i_IbCfkwhSISjgzgSJ6izuAF-GRBJxy/exec";

        private const string API_KEY = "1234";

        // --- 内部データ構造 ---
        [Serializable]
        public class TicketData
        {
            public string id;
            public string sceneName;
            public bool isInUse;
            public string userName;
            public string masterPath;
            public string timestamp;
        }

        [Serializable]
        public class TicketListWrapper
        {
            public List<TicketData> items;
        }

        private string savedUserName = "";
        private string inputNameBuffer = "";
        private int currentTab;
        private List<TicketData> ticketList = new();
        private bool isLoading;
        private Vector2 scrollPos;
        
        private readonly Color emptyColor = new(0.2f, 0.6f, 0.2f, 0.3f);
        private readonly Color occupiedByOtherColor = new(0.6f, 0.2f, 0.2f, 0.3f);
        private readonly Color occupiedBySelfColor = new(0.2f, 0.4f, 0.6f, 0.3f);
        private readonly Vector2 minWindowSize = new(800f, 200f);

        [MenuItem("Window/MasterTicketWindow")]
        public static void ShowWindow()
        {
            GetWindow<MasterTicketWindow>("Master Ticket Window");
        }

        private void OnEnable()
        {
            minSize = minWindowSize;
            savedUserName = EditorPrefs.GetString("TicketSystem_UserName", "");
            inputNameBuffer = savedUserName;
            if (!string.IsNullOrEmpty(savedUserName)) RefreshList();
        }

        private void OnGUI()
        {
            // savedUserNameが空の間だけ、入力用UIを表示し続ける
            if (string.IsNullOrEmpty(savedUserName))
            {
                DrawSetupUI();
            }
            else
            {
                DrawMainUI();
            }
        }

        /// <summary>
        /// ユーザー名の入力と保存のUIを描画する。ユーザー名が保存されるまでは、他のUIは表示しないようにする。
        /// </summary>
        private void DrawSetupUI()
        {
            EditorGUILayout.HelpBox("ユーザー名を設定してください。設定するまでメイン機能は使えません。", MessageType.Info);
            inputNameBuffer = EditorGUILayout.TextField("ユーザー名", inputNameBuffer);

            if (GUILayout.Button("保存して開始", GUILayout.Height(50)))
            {
                if (!string.IsNullOrEmpty(inputNameBuffer))
                {
                    savedUserName = inputNameBuffer;
                    EditorPrefs.SetString("TicketSystem_UserName", savedUserName);
                    RefreshList();
                }
                else
                {
                    EditorUtility.DisplayDialog("エラー", "名前を入力してください。", "OK");
                }
            }
        }

        /// <summary>
        /// ユーザー名の表示と変更ボタン、タブ切り替えのUIを描画する。通信中は通信中のメッセージを表示して、タブの内容は描画しない。
        /// </summary>
        private void DrawMainUI()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label($"ユーザー: {savedUserName}");
            if (GUILayout.Button("名前変更", EditorStyles.toolbarButton))
            {
                savedUserName = "";
            }

            EditorGUILayout.EndHorizontal();

            currentTab = GUILayout.Toolbar(currentTab, new[] { "一覧表示 (List)", "新規作成 (Create)" });
            
            if (isLoading)
            {
                EditorGUILayout.HelpBox("通信中...", MessageType.Info);
                return;
            }

            switch (currentTab)
            {
                case 0: DrawListTab(); break;
                case 1: DrawCreateTab(); break;
            }
        }

        /// <summary>
        /// チケットの一覧表示用UI。GASから取得したticketListをループして、シーン名や状態、担当者、最終更新時刻などを表示する。
        /// チケットの状態に応じて行の背景色を変える。各チケットに対して、使用開始・解放の切り替えボタンと、そのシーンの位置まで移動するボタンを置く。
        /// </summary>
        private void DrawListTab()
        {
            if (GUILayout.Button("更新", GUILayout.Height(35))) RefreshList();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Label("シーン名", GUILayout.Width(100));
            GUILayout.Label("状態", GUILayout.Width(60));
            GUILayout.Label("担当者", GUILayout.Width(100));
            GUILayout.Label("最終更新時刻", GUILayout.Width(200));
            GUILayout.Label("操作", GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            foreach (var ticket in ticketList)
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
                    UpdateTicketStatus(ticket);
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

        /// <summary>
        /// チケットの新規作成用UI。現在アクティブなシーンの情報を表示して、そのシーンに対するチケットを発行するボタンを置く。
        /// </summary>
        private void DrawCreateTab()
        {
            var activeScene = SceneManager.GetActiveScene();
            EditorGUILayout.LabelField("対象シーン", activeScene.name);
            EditorGUILayout.LabelField("パス", activeScene.path);

            if (GUILayout.Button("チケットを発行して使用開始", GUILayout.Height(40)))
            {
                CreateTicket(activeScene.name, activeScene.path);
            }
        }

        /// <summary>
        /// アセットのパスを受け取って、そのアセットをプロジェクトウィンドウで選択状態にする。
        /// </summary>
        /// <param name="assetPath"></param>
        public static void JumpToAsset(string assetPath)
        {
            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);

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

        // --- 通信処理 ---

        /// <summary>
        /// GASにリクエストを送ってチケットの最新一覧を取得し、ローカルのticketListを更新する。通信中はisLoadingをtrueにしてUIに反映させる。
        /// </summary>
        private void RefreshList()
        {
            isLoading = true;
            var request = UnityWebRequest.Get(GAS_URL);
            var operation = request.SendWebRequest();
            operation.completed += (_) =>
            {
                if (request.result == UnityWebRequest.Result.Success)
                {
                    var json = "{\"items\":" + request.downloadHandler.text + "}";
                    ticketList = JsonUtility.FromJson<TicketListWrapper>(json).items;
                }

                isLoading = false;
                Repaint();
            };
        }

        /// <summary>
        /// チケットを新規作成して使用開始する。GASにリクエストを送る前に、ローカルのticketオブジェクトの状態を使用中にしておく。
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="path"></param>
        private void CreateTicket(string sceneName, string path)
        {
            isLoading = true;
            var ticket = new TicketData
            {
                id = Guid.NewGuid().ToString(),
                sceneName = sceneName,
                isInUse = true,
                userName = savedUserName,
                masterPath = path
            };

            SendPost("create", ticket);
        }

        /// <summary>
        /// チケットの使用開始・解放を切り替える。GASに更新リクエストを送る前に、ローカルのticketオブジェクトの状態を切り替えておく。
        /// </summary>
        /// <param name="ticket"></param>
        private void UpdateTicketStatus(TicketData ticket)
        {
            isLoading = true;
            ticket.userName = ticket.isInUse ? "" : savedUserName;
            ticket.isInUse = !ticket.isInUse;
            SendPost("update", ticket);
        }

        /// <summary>
        /// GASに対してチケットの作成・更新リクエストを送る共通関数。レスポンスの内容に応じてダイアログを出したり、一覧を更新したりする。
        /// </summary>
        /// <param name="action"></param>
        /// <param name="data"></param>
        private void SendPost(string action, TicketData data)
        {
            var json = JsonUtility.ToJson(data);
            json = json.Insert(1, $"\"action\":\"{action}\",\"key\":\"{API_KEY}\",");

            var request = new UnityWebRequest(GAS_URL, "POST");
            var bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            var operation = request.SendWebRequest();
            operation.completed += (_) =>
            {
                isLoading = false;
                var response = request.downloadHandler.text;

                if (response.StartsWith("ERROR_ALREADY_IN_USE"))
                {
                    var occupant = response.Replace("ERROR_ALREADY_IN_USE:", "");
                    EditorUtility.DisplayDialog("発行失敗",
                        $"このシーンは現在 {occupant} さんが使用中です。\n作業を始める前に本人に確認してください。", "了解");
                }
                else if (response == "SUCCESS")
                {
                    EditorUtility.DisplayDialog("完了", "チケットの更新が完了しました。", "OK");

                    // 発行後は一覧タブに切り替える
                    currentTab = 0;
                }
                else
                {
                    Debug.Log(response);
                }

                RefreshList();
            };
        }
    }
}

#endif