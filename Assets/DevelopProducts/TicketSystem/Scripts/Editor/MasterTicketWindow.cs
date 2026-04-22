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
        // --- 設定項目 (GASのデプロイ後に書き換えてください) ---
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

        [MenuItem("Window/MasterTicketWindow")]
        public static void ShowWindow()
        {
            GetWindow<MasterTicketWindow>("Master Ticket Window");
        }

        private void OnEnable()
        {
            this.minSize = new Vector2(800f, 200f);
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
                                                                                    
        private void DrawMainUI()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label($"ユーザー: {savedUserName}");
            if (GUILayout.Button("名前変更", EditorStyles.toolbarButton))
            {
                savedUserName = ""; // これでOnGUIの条件により入力画面に戻る
            }

            EditorGUILayout.EndHorizontal();

            currentTab = GUILayout.Toolbar(currentTab, new[] { "一覧表示", "新規作成" });

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
                
                Color rectColor = new Color(0.2f, 0.6f, 0.2f, 0.3f);
                if (ticket.isInUse)
                {
                    rectColor = ticket.userName == savedUserName ? new Color(0.2f, 0.4f, 0.6f, 0.3f) : new Color(0.6f, 0.2f, 0.2f, 0.3f);
                }
                EditorGUI.DrawRect(rowRect, rectColor);
                GUILayout.Label(ticket.sceneName, GUILayout.Width(100));
                GUILayout.Label(ticket.isInUse ? "使用中" : "空き", GUILayout.Width(60));
                GUILayout.Label(ticket.userName, GUILayout.Width(100));
                GUILayout.Label(ticket.timestamp, GUILayout.Width(200));
                
                bool isMyTicket = string.IsNullOrEmpty(ticket.userName) || (ticket.userName == savedUserName);
                
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

        // --- 通信処理 ---

        private void RefreshList()
        {
            isLoading = true;
            var request = UnityWebRequest.Get(GAS_URL);
            var operation = request.SendWebRequest();
            operation.completed += (_) =>
            {
                if (request.result == UnityWebRequest.Result.Success)
                {
                    string json = "{\"items\":" + request.downloadHandler.text + "}";
                    ticketList = JsonUtility.FromJson<TicketListWrapper>(json).items;
                }

                isLoading = false;
                Repaint();
            };
        }

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
            
            SendPost("create", ticket); // 発行時はダイアログを出す
        }

        private void UpdateTicketStatus(TicketData ticket)
        {
            isLoading = true;
            ticket.userName = ticket.isInUse ? "" : savedUserName;
            ticket.isInUse = !ticket.isInUse;
            SendPost("update", ticket);
        }

        private void SendPost(string action, TicketData data)
        {
            string json = JsonUtility.ToJson(data);
            json = json.Insert(1, $"\"action\":\"{action}\",\"key\":\"{API_KEY}\",");

            var request = new UnityWebRequest(GAS_URL, "POST");
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            var operation = request.SendWebRequest();
            operation.completed += (_) =>
            {
                isLoading = false;
                string response = request.downloadHandler.text;

                if (response.StartsWith("ERROR_ALREADY_IN_USE"))
                {
                    string occupant = response.Replace("ERROR_ALREADY_IN_USE:", "");
                    EditorUtility.DisplayDialog("発行失敗", 
                        $"このシーンは現在 {occupant} さんが使用中です。\n作業を始める前に本人に確認してください。", "了解");
                }
                else if (response == "SUCCESS")
                {
                    EditorUtility.DisplayDialog("完了", "チケットの更新が完了しました。", "OK");
                    currentTab = 0; // 発行後は一覧タブに切り替える
                }
                else
                {
                    Debug.Log(response);
                }
    
                RefreshList();
            };
        }
        
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
    }
}

#endif