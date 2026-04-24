using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DevelopProducts.TicketSystem
{
    /// <summary>
    /// シーンが保存される直前に、現在のシーンに関連するチケットの状態をチェックし、必要に応じて警告ダイアログを表示するクラス。
    /// </summary>
    [InitializeOnLoad]
    public class SceneSaveObserver
    {
        static SceneSaveObserver()
        {
            // シーンが保存される直前に呼ばれるイベントを登録する。
            EditorSceneManager.sceneSaving += OnSceneSaving;

            // エディタ起動時にチケットデータの初期ロードを行う。
            TicketSystemWebClient.RefreshList().ContinueWith(_ => { Debug.Log("チケットデータの初期ロードが完了しました。"); });
        }

        /// <summary>
        /// シーンが保存される直前に呼ばれるイベントハンドラー。
        /// 現在のシーンに関連するチケットの状態をチェックし、必要に応じて警告ダイアログを表示する。
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="path"></param>
        private static void OnSceneSaving(Scene scene, string path)
        {
            var currentUserName = TicketSystemSettings.instance.userName;
            if (string.IsNullOrEmpty(currentUserName))
            {
                Debug.LogWarning($"ユーザー名が設定されていません。{nameof(SceneSaveObserver)}は現在利用できません。");
                return;
            }

            var cachedTickets = CachedTicketDataSingleton.instance.GetAll();
            if (cachedTickets.Count == 0)
            {
                Debug.LogWarning("キャッシュされたチケットがありませんでした。");
                return;
            }

            foreach (var ticketData in cachedTickets)
            {
                if (ticketData.sceneName != scene.name) continue;

                // 自身が使用中のチケットは無視する。
                if (ticketData.userName == currentUserName) continue;

                // 見つかったチケットの使用状況に応じて、警告ダイアログを表示する。
                var dialogMessage = ticketData.isInUse
                    ? $"編集中のシーン: [{scene.name}] は現在 {ticketData.userName} さんによって使用中です。保存した内容はSourceTreeから破棄することを推奨します。"
                    : $"編集中のシーン: [{scene.name}] は現在チケットとして登録されていますが、使用中になっていません。編集する場合、[Window > Master Ticket Window] からチケット登録をしてください。";

                EditorUtility.DisplayDialog("シーン保存の警告", dialogMessage, "OK");
            }
        }
    }
}