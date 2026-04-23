using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace DevelopProducts.TicketSystem
{
    [InitializeOnLoad]
    public class SceneSaveObserver
    {
        static SceneSaveObserver()
        {
            // シーンが保存される直前に呼ばれるイベントに登録
            EditorSceneManager.sceneSaving += OnSceneSaving;
        }

        private static void OnSceneSaving(Scene scene, string path)
        {
            var currentUserName = EditorPrefs.GetString("TicketSystem_UserName", "");
            foreach (var ticketData in CachedTicketDataSingleton.instance.GetAll())
            {
                if (ticketData.sceneName != scene.name
                    || !ticketData.isInUse
                    || ticketData.userName == currentUserName) continue;
                
                // すでに使用中のチケットがある場合は、保存を許可しない
                EditorUtility.DisplayDialog(
                    "シーン保存の警告",
                    $"シーン '{scene.name}' は現在 '{ticketData.userName}' によって使用中です。保存した内容はSourceTreeから破棄することを推奨します。",
                    "OK"
                );

                throw new System.OperationCanceledException("チケットシステムによってシーンの保存がキャンセルされました。");
            }
        }
    }
}