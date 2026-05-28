using SymphonyFrameWork.Utility;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace KillChord.Editor.AutoBuilder
{
    public static class AutoBuildExecuter
    {
        [Serializable]
        private struct BuildSession
        {
            public string OutputPath;

            public string[] ProfileGuids;

            public int CurrentIndex;

            public bool Running;

            public static BuildSession LoadSession()
            {
                string json = SessionState.GetString(SESSION_KEY, string.Empty);

                if (string.IsNullOrEmpty(json))
                {
                    return new BuildSession();
                }

                return JsonUtility.FromJson<BuildSession>(json);
            }

            public static void SaveSession(BuildSession session)
            {
                string json = JsonUtility.ToJson(session);

                SessionState.SetString(SESSION_KEY, json);
            }

            public static void ClearSession()
            {
                SessionState.EraseString(SESSION_KEY);
            }

            private const string SESSION_KEY = "AUTO_BUILD_SESSION";
        }

        public static void Run(string path, params BuildProfile[] profiles)
        {
            if (profiles == null || profiles.Length == 0)
            {
                Debug.LogError(
                    "Build Profiles are not set.");

                return;
            }

            BuildSession session = new()
            {
                OutputPath = path,

                ProfileGuids = profiles
                    .Select(p =>
                    {
                        string assetPath = AssetDatabase.GetAssetPath(p);

                        return AssetDatabase.AssetPathToGUID(assetPath);
                    })
                    .ToArray(),

                CurrentIndex = 0,

                Running = true
            };

            BuildSession.SaveSession(session);

            EditorApplication.delayCall += ResumeBuild;
        }

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            EditorApplication.delayCall += Resume;
        }

        private static async void Resume()
        {
            EditorApplication.delayCall -= Resume;

            await Awaitable.NextFrameAsync();

            BuildSession session = BuildSession.LoadSession();

            if (!session.Running)
            {
                return;
            }

            EditorApplication.delayCall += ResumeBuild;
        }

        private static void ResumeBuild()
        {
            EditorApplication.delayCall -= ResumeBuild;

            BuildSession session = BuildSession.LoadSession();

            if (!session.Running)
            {
                return;
            }

            // 完了していたら終了。
            if (session.CurrentIndex >= session.ProfileGuids.Length)
            {
                Debug.Log("All Build Complete");

                BuildSession.ClearSession();

                return;
            }

            EditorApplication.delayCall += ExecuteBuild;
        }

        private static async void ExecuteBuild()
        {
            EditorApplication.delayCall -= ExecuteBuild;

            await WaitForEditorReady();

            BuildSession session = BuildSession.LoadSession();

            if (!session.Running)
            {
                return;
            }

            string guid = session.ProfileGuids[session.CurrentIndex];

            string assetPath = AssetDatabase.GUIDToAssetPath(guid);

            BuildProfile profile = AssetDatabase.LoadAssetAtPath<BuildProfile>(assetPath);

            if (profile == null)
            {
                Debug.LogError($"Profile Missing : {guid}");

                NextSession();

                return;
            }

            Debug.Log($"Start Build : {profile.name}");

            // Profile切替。
            BuildProfile.SetActiveBuildProfile(profile);

            await WaitForEditorReady();

            string[] scenes = profile.GetScenesForBuild()
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();

            // プロファイルに指定がなければグローバルを使用。
            if (scenes.Length == 0)
            {
                scenes = EditorBuildSettings.scenes
                    .Where(s => s.enabled)
                    .Select(s => s.path)
                    .ToArray();
            }

            if (scenes.Length == 0)
            {
                Debug.LogWarning($"No scenes : {profile.name}");

                NextSession();

                return;
            }

            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;

            string buildDir = Path.Combine(session.OutputPath, profile.name);
            string fileName = Application.productName + GetExtension(target);
            string locationPath = Path.Combine(buildDir, fileName);

            BuildPlayerOptions options = new()
            {
                scenes = scenes,

                target = target,

                locationPathName = locationPath
            };

            // フォルダ生成。
            if (Directory.Exists(buildDir))
            {
                Directory.Delete(buildDir, true);
            }

            Directory.CreateDirectory(buildDir);

            AssetDatabase.Refresh(ImportAssetOptions.DontDownloadFromCacheServer);

            await WaitForEditorReady();

            // BuildはdelayCallから実行する。
            EditorApplication.delayCall += ExecutePlayerBuild;

            void ExecutePlayerBuild()
            {
                EditorApplication.delayCall -= ExecutePlayerBuild;

                BuildReport report = BuildPipeline.BuildPlayer(options);

                if (report.summary.result != BuildResult.Succeeded)
                {
                    Debug.LogError(
                        $"Build Failed : {profile.name}");
                }
                else
                {
                    Debug.Log(
                        $"Build Succeeded : {profile.name}");
                }

                NextSession();
            }

            void NextSession()
            {
                session.CurrentIndex++;

                BuildSession.SaveSession(session);

                EditorApplication.delayCall += ResumeBuild;
            }
        }

        private static async ValueTask WaitForEditorReady()
        {
            await SymphonyTask.WaitUntil(() =>
            {
                if (EditorApplication.isCompiling)
                {
                    return false;
                }

                if (EditorApplication.isUpdating)
                {
                    return false;
                }

                if (BuildPipeline.isBuildingPlayer)
                {
                    return false;
                }

                return true;
            });

            // 念のためさらに1フレーム待つ。
            await Awaitable.NextFrameAsync();
        }

        private static string GetExtension(BuildTarget target)
        {
            return target switch
            {
                BuildTarget.StandaloneWindows => ".exe",
                BuildTarget.StandaloneWindows64 => ".exe",
                BuildTarget.Android => ".apk",
                BuildTarget.StandaloneOSX => ".app",
                _ => string.Empty
            };
        }
    }
}