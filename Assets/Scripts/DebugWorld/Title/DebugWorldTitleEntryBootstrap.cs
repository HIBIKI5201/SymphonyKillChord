using KillChord.DebugWorld.Battle;
using KillChord.DebugWorld.Common;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KillChord.DebugWorld.Title
{
    /// <summary>
    ///     タイトルシーンが開かれたときに、裏世界への入口を差し込むクラスです。
    ///     <para> 本番のタイトルシーンは変更せず、開発ビルドとエディタだけで入口を足します。 </para>
    /// </summary>
    public static class DebugWorldTitleEntryBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        /// <summary>
        ///     タイトルシーンが開かれたら、そのシーンに入口を置きます。
        /// </summary>
        /// <param name="scene"> 開かれたシーンです。 </param>
        /// <param name="mode"> 開き方です。 </param>
        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!string.Equals(scene.name, DebugWorldSceneNames.TITLE, StringComparison.Ordinal))
            {
                return;
            }

            // 戦闘からポーズでタイトルへ戻った場合も、裏世界の戦闘設定を外す。
            DebugBattleSession.End();

            // ハブシーンがビルドに無いProfileでは、開けない入口を出さない。
            if (!DebugWorldSceneChanger.CanOpenHub())
            {
                return;
            }

            GameObject entryObject = new(nameof(DebugWorldTitleEntry));
            SceneManager.MoveGameObjectToScene(entryObject, scene);
            entryObject.AddComponent<DebugWorldTitleEntry>();
        }
    }
}
