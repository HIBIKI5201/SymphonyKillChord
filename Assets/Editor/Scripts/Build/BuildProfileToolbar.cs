using KillChord.Editor.SourceDataProvider.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEditor.Overlays;
using UnityEditor.Toolbars;
using UnityEngine;

namespace KillChord.Editor.Build
{
    /// <summary>
    ///     メインツールバーの再生ボタン隣に、アクティブなBuild Profileの切り替えを表示します。
    ///     Profile名の区切り文字 "_" をメニュー階層として扱い、「種別/モード/プラットフォーム」で選択できます。
    /// </summary>
    internal static class BuildProfileToolbar
    {
        /// <summary>
        ///     アクティブなBuild Profile名を表示し、切り替えメニューを開く要素を生成します。
        /// </summary>
        /// <returns> ツールバーへ配置するドロップダウンです。 </returns>
        [MainToolbarElement(
            ELEMENT_PATH,
            defaultDockPosition = MainToolbarDockPosition.Middle,
            defaultDockIndex = DEFAULT_DOCK_INDEX)]
        internal static MainToolbarDropdown CreateProfileDropdown()
        {
            _dropdown = new MainToolbarDropdown(
                CreateContent(BuildProfile.GetActiveBuildProfile()),
                ShowProfileMenu);
            return _dropdown;
        }

        /// <summary> ツールバー要素の登録パスです。 </summary>
        private const string ELEMENT_PATH = "KillChord/Build Profile";

        /// <summary> 再生ボタン群の右隣へ並べるための表示順です。 </summary>
        private const int DEFAULT_DOCK_INDEX = 100;

        /// <summary> 初回表示を済ませたかを記録するEditorPrefsキーです。 </summary>
        private const string FIRST_SHOWN_PREFS_KEY = "KillChord.BuildProfileToolbar.FirstShown";

        /// <summary> Profile名の階層区切り文字です。 </summary>
        private const char PROFILE_NAME_SEPARATOR = '_';

        /// <summary> メニュー階層の区切り文字です。 </summary>
        private const char MENU_PATH_SEPARATOR = '/';

        private const string NO_PROFILE_LABEL = "Platform";
        private const string NO_PROFILE_MENU_LABEL = "Build Profileがありません";
        private const string TOOLTIP =
            "アクティブなBuild Profileです。体験版Profileを選ぶと KILLCHORD_DEMO が有効になり、Addressables Groupも切り替わります。";

        private static MainToolbarDropdown _dropdown;

        /// <summary>
        ///     追加直後のツールバー要素は非表示で登録されるため、初回だけ表示状態にします。
        ///     以降はユーザーがツールバーの右クリックメニューで切り替えた状態を尊重します。
        /// </summary>
        [InitializeOnLoadMethod]
        private static void ShowOnFirstDiscovery()
        {
            if (EditorPrefs.GetBool(FIRST_SHOWN_PREFS_KEY, false))
            {
                return;
            }

            // ツールバーの構築後でないとOverlayを取得できないため、1フレーム遅らせる。
            EditorApplication.delayCall += TryShowOnce;
        }

        /// <summary>
        ///     Build Profileを選ぶドロップダウンメニューを表示します。
        /// </summary>
        /// <param name="rect"> メニューを開く基準となる矩形です。 </param>
        private static void ShowProfileMenu(Rect rect)
        {
            BuildProfile activeProfile = BuildProfile.GetActiveBuildProfile();
            GenericMenu menu = new();

            foreach (BuildProfile profile in FindProfiles())
            {
                BuildProfile targetProfile = profile;
                menu.AddItem(
                    new GUIContent(ToMenuPath(profile.name)),
                    profile == activeProfile,
                    () => SelectProfile(targetProfile));
            }

            if (menu.GetItemCount() == 0)
            {
                menu.AddDisabledItem(new GUIContent(NO_PROFILE_MENU_LABEL));
            }

            menu.DropDown(rect);
        }

        /// <summary>
        ///     指定したBuild Profileをアクティブにし、ゲームデータ種別の設定を反映します。
        /// </summary>
        /// <param name="profile"> 切り替え先のBuild Profileです。 </param>
        private static void SelectProfile(BuildProfile profile)
        {
            BuildProfile activeProfile = BuildProfile.GetActiveBuildProfile();
            if (profile == null || profile == activeProfile)
            {
                return;
            }

            if (!ConfirmPlatformSwitch(activeProfile, profile))
            {
                return;
            }

            // Editor再生でも体験版専用シーンを読めるよう、アクティブ化の前にシーン一覧を揃える。
            GameDataVariantProfiles.SynchronizeDemoScenes(profile);
            BuildProfile.SetActiveBuildProfile(profile);

            // 同じ種別同士の切り替えではドメインリロードが起きないため、Group設定もここで反映する。
            GameDataVariantBuildSettings.ApplyActiveProfile();

            if (_dropdown != null)
            {
                _dropdown.content = CreateContent(profile);
            }
        }

        /// <summary>
        ///     プラットフォームが変わる切り替えは再インポートに時間がかかるため、実行前に確認します。
        /// </summary>
        /// <param name="currentProfile"> 現在アクティブなBuild Profileです。 </param>
        /// <param name="nextProfile"> 切り替え先のBuild Profileです。 </param>
        /// <returns> 切り替えを続行する場合はtrueです。 </returns>
        private static bool ConfirmPlatformSwitch(BuildProfile currentProfile, BuildProfile nextProfile)
        {
            string currentPlatform = GetPlatformName(currentProfile);
            string nextPlatform = GetPlatformName(nextProfile);
            if (string.Equals(currentPlatform, nextPlatform, StringComparison.Ordinal))
            {
                return true;
            }

            return EditorUtility.DisplayDialog(
                "プラットフォームの切り替え",
                $"{nextProfile.name} へ切り替えるとプラットフォームが {nextPlatform} に変わり、再インポートに時間がかかります。続行しますか？",
                "切り替える",
                "やめる");
        }

        /// <summary>
        ///     Profile名の末尾要素をプラットフォーム名として取得します。
        /// </summary>
        /// <param name="profile"> 対象のBuild Profileです。 </param>
        /// <returns> プラットフォーム名です。取得できない場合は空文字です。 </returns>
        private static string GetPlatformName(BuildProfile profile)
        {
            if (profile == null)
            {
                return string.Empty;
            }

            int separatorIndex = profile.name.LastIndexOf(PROFILE_NAME_SEPARATOR);
            return separatorIndex < 0 ? profile.name : profile.name[(separatorIndex + 1)..];
        }

        /// <summary>
        ///     プロジェクト内のBuild Profileを名前順に取得します。
        /// </summary>
        /// <returns> 見つかったBuild Profileです。 </returns>
        private static IEnumerable<BuildProfile> FindProfiles()
        {
            return AssetDatabase.FindAssets($"t:{nameof(BuildProfile)}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<BuildProfile>)
                .Where(profile => profile != null)
                .OrderBy(profile => profile.name, StringComparer.Ordinal);
        }

        /// <summary>
        ///     Profile名をメニュー階層のパスへ変換します。
        /// </summary>
        /// <param name="profileName"> Build Profile名です。 </param>
        /// <returns> メニュー階層のパスです。 </returns>
        private static string ToMenuPath(string profileName)
        {
            return profileName.Replace(PROFILE_NAME_SEPARATOR, MENU_PATH_SEPARATOR);
        }

        /// <summary>
        ///     ツールバー要素を一度だけ表示状態にします。
        /// </summary>
        private static void TryShowOnce()
        {
            if (!TryGetOverlay(out Overlay overlay))
            {
                return;
            }

            overlay.displayed = true;
            EditorPrefs.SetBool(FIRST_SHOWN_PREFS_KEY, true);
        }

        /// <summary>
        ///     ツールバー要素に対応するOverlayを取得します。
        /// </summary>
        /// <param name="overlay"> 取得したOverlayです。 </param>
        /// <returns> 取得できた場合はtrueです。 </returns>
        private static bool TryGetOverlay(out Overlay overlay)
        {
            overlay = null;

            // MainToolbar.TryGetOverlay は internal のため、表示制御にはリフレクションを使う。
            MethodInfo method = typeof(MainToolbar).GetMethod(
                "TryGetOverlay",
                BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null)
            {
                return false;
            }

            object[] arguments = { ELEMENT_PATH, null };
            if (!(bool)method.Invoke(null, arguments))
            {
                return false;
            }

            overlay = arguments[1] as Overlay;
            return overlay != null;
        }

        /// <summary>
        ///     アクティブなBuild Profileを示すツールバー表示内容を生成します。
        /// </summary>
        /// <param name="profile"> 表示するBuild Profileです。 </param>
        /// <returns> ツールバーへ表示する内容です。 </returns>
        private static MainToolbarContent CreateContent(BuildProfile profile)
        {
            return new MainToolbarContent(profile != null ? profile.name : NO_PROFILE_LABEL, TOOLTIP);
        }
    }
}
