using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;

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
            public int isInUse;
            public string userName;
            public string masterPath;
            public string status;
            public string timestamp;
        }

        [Serializable]
        public class TicketListWrapper
        {
            public List<TicketData> items;
        }

        private string savedUserName = "";
        private string inputNameBuffer = ""; // 入力確定前の保持用
        private int currentTab;
        private List<TicketData> ticketList = new();
        private bool isLoading;
        private string newDescription = "";

        [MenuItem("Window/MasterTicketWindow")]
        public static void ShowWindow()
        {
            GetWindow<MasterTicketWindow>("Master Ticket");
        }

        private void OnEnable()
        {
            savedUserName = EditorPrefs.GetString("TicketSystem_UserName", "");
            inputNameBuffer = savedUserName; // 初期値を同期
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

            if (GUILayout.Button("保存して開始", GUILayout.Height(30)))
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
                EditorGUILayout.HelpBox("通信中...", MessageType.None);
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
            if (GUILayout.Button("更新", GUILayout.Height(25))) RefreshList();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Label("シーン名", GUILayout.Width(150));
            GUILayout.Label("状態", GUILayout.Width(60));
            GUILayout.Label("担当者", GUILayout.Width(100));
            GUILayout.Label("操作", GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();

            foreach (var ticket in ticketList)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(ticket.sceneName, GUILayout.Width(150));

                GUI.color = ticket.isInUse == 1 ? Color.red : Color.green;
                GUILayout.Label(ticket.isInUse == 1 ? "使用中" : "空き", GUILayout.Width(60));
                GUI.color = Color.white;

                GUILayout.Label(ticket.userName, GUILayout.Width(100));

                if (GUILayout.Button(ticket.isInUse == 1 ? "解放する" : "使用する"))
                {
                    UpdateTicketStatus(ticket, ticket.isInUse == 1 ? 0 : 1);
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawCreateTab()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            EditorGUILayout.LabelField("対象シーン", activeScene.name);
            EditorGUILayout.LabelField("パス", activeScene.path);
            newDescription = EditorGUILayout.TextField("作業内容・備考", newDescription);

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
            operation.completed += (op) =>
            {
                if (request.result == UnityWebRequest.Result.Success)
                {
                    string json = "{\"items\":" + request.downloadHandler.text + "}";
                    Debug.Log(json);
                    ticketList = JsonUtility.FromJson<TicketListWrapper>(json).items;
                }

                isLoading = false;
                Repaint();
            };
        }

        private void CreateTicket(string sceneName, string path)
        {
            isLoading = true;
            var data = new TicketData
            {
                id = Guid.NewGuid().ToString(),
                sceneName = sceneName,
                isInUse = 1,
                userName = savedUserName,
                masterPath = path,
                status = "New"
            };
            
            SendPost("create", data, true); // 発行時はダイアログを出す
        }

        private void UpdateTicketStatus(TicketData ticket, int nextUseState)
        {
            isLoading = true;
            ticket.isInUse = nextUseState;
            ticket.status = nextUseState == 1 ? "In Progress" : "Done";
            SendPost("update", ticket, false);
        }

        private void SendPost(string action, TicketData data, bool showDialog)
        {
            string json = JsonUtility.ToJson(data);
            json = json.Insert(1, $"\"action\":\"{action}\",\"key\":\"{API_KEY}\",");
            Debug.Log(json);

            var request = new UnityWebRequest(GAS_URL, "POST");
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            var operation = request.SendWebRequest();
            operation.completed += (op) =>
            {
                isLoading = false;
                if (showDialog)
                {
                    EditorUtility.DisplayDialog("完了", "チケットの発行が完了しました。", "OK");
                }

                RefreshList();
            };
        }
    }
}