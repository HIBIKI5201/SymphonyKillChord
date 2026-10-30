using KillChord.Runtime.Adaptor.Persistent.SceneManagement;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace KillChord.DebugWorld.Common
{
    /// <summary>
    ///     本番のシーン遷移を使って、裏世界のシーンを出入りするクラスです。
    /// </summary>
    public static class DebugWorldSceneChanger
    {
        /// <summary>
        ///     ハブシーンをビルドに含めていて開ける状態かを返します。
        /// </summary>
        /// <returns> 開ける場合はtrueです。 </returns>
        public static bool CanOpenHub()
        {
            return UnityEngine.Application.CanStreamedLevelBeLoaded(DebugWorldSceneNames.HUB);
        }

        /// <summary>
        ///     遷移元のシーンを閉じて遷移先のシーンを開きます。
        ///     <para> 常駐シーンの寿命で動かし、遷移元のシーンが閉じても遷移を中断しません。 </para>
        /// </summary>
        /// <param name="fromSceneName"> 遷移元のシーン名です。 </param>
        /// <param name="toSceneName"> 遷移先のシーン名です。 </param>
        /// <returns> 遷移に成功した場合はtrueです。 </returns>
        public static async Task<bool> ChangeSceneAsync(string fromSceneName, string toSceneName)
        {
            if (!ServiceLocator.TryGetInstance(out SceneTransitionController controller))
            {
                Debug.LogError(
                    $"[{nameof(DebugWorldSceneChanger)}] {nameof(SceneTransitionController)}が取得できません。"
                    + "常駐シーンから起動してください。");
                return false;
            }

            try
            {
                bool isSuccess = await controller.ChangeSceneWithPersistentLifetimeAsync(fromSceneName, toSceneName);
                if (!isSuccess)
                {
                    Debug.LogError(
                        $"[{nameof(DebugWorldSceneChanger)}] シーン遷移に失敗しました。{fromSceneName} -> {toSceneName}");
                }

                return isSuccess;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }
    }
}
