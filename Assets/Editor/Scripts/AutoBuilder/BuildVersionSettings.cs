using System;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace KillChord.Editor.AutoBuilder
{
    /// <summary>
    ///     自動ビルドで、環境変数で渡された版（例: 3.2.4）をプレイヤーの版番号へ一時的に適用します。
    ///     版は develop・main へのマージで付く v タグから CI が決めます。ビルドが終わったら元の設定へ戻します。
    /// </summary>
    internal static class BuildVersionSettings
    {
        /// <summary>
        ///     ビルドの直前に呼び、版番号を適用します。環境変数が無い（手元のビルド）ときは何もしません。
        /// </summary>
        /// <param name="profileName"> ログに出すビルドプロファイル名です。 </param>
        /// <returns> ビルドを続けてよい場合は true です。版の形式が正しくない場合は false です。 </returns>
        public static bool TryApply(string profileName)
        {
            string version = Environment.GetEnvironmentVariable(BUILD_VERSION_ENV);
            if (string.IsNullOrEmpty(version))
            {
                return true;
            }

            // メジャー・マイナー・パッチを取り出し、versionCode に収まるかを確かめる。
            Match match = VERSION_PATTERN.Match(version.Trim());
            if (!match.Success)
            {
                Debug.LogError($"[{nameof(BuildVersionSettings)}] {BUILD_VERSION_ENV} の値 '{version}' が <メジャー>.<マイナー>.<パッチ> の形式ではないため、{profileName} のビルドを止めます。");
                return false;
            }

            int major = int.Parse(match.Groups[1].Value);
            int minor = int.Parse(match.Groups[2].Value);
            int patch = int.Parse(match.Groups[3].Value);
            if (major > MAX_MAJOR || minor > MAX_MINOR_OR_PATCH || patch > MAX_MINOR_OR_PATCH)
            {
                Debug.LogError($"[{nameof(BuildVersionSettings)}] 版 {version} は Android の versionCode に収まらないため、{profileName} のビルドを止めます（メジャー {MAX_MAJOR}、マイナー・パッチ {MAX_MINOR_OR_PATCH} まで）。");
                return false;
            }

            // Android の versionCode と iOS・macOS のビルド番号は、版が上がるほど大きくなる数にする。
            int versionCode = major * MAJOR_FACTOR + minor * MINOR_FACTOR + patch;
            string bundleVersion = $"{major}.{minor}.{patch}";

            // 元の設定を覚えてから、版番号を入れる。
            if (!_hasSavedState)
            {
                _savedBundleVersion = PlayerSettings.bundleVersion;
                _savedAndroidVersionCode = PlayerSettings.Android.bundleVersionCode;
                _savedIosBuildNumber = PlayerSettings.iOS.buildNumber;
                _savedMacBuildNumber = PlayerSettings.macOS.buildNumber;
                _hasSavedState = true;
            }

            PlayerSettings.bundleVersion = bundleVersion;
            PlayerSettings.Android.bundleVersionCode = versionCode;
            PlayerSettings.iOS.buildNumber = versionCode.ToString();
            PlayerSettings.macOS.buildNumber = versionCode.ToString();

            Debug.Log($"[{nameof(BuildVersionSettings)}] {profileName} を版 {bundleVersion}（versionCode {versionCode}）でビルドします。");
            return true;
        }

        /// <summary>
        ///     適用した版番号を、適用前の値へ戻します。
        /// </summary>
        public static void Restore()
        {
            if (!_hasSavedState)
            {
                return;
            }

            PlayerSettings.bundleVersion = _savedBundleVersion;
            PlayerSettings.Android.bundleVersionCode = _savedAndroidVersionCode;
            PlayerSettings.iOS.buildNumber = _savedIosBuildNumber;
            PlayerSettings.macOS.buildNumber = _savedMacBuildNumber;
            _hasSavedState = false;
        }

        /// <summary> 版（例: 3.2.4）を渡す環境変数名です。 </summary>
        private const string BUILD_VERSION_ENV = "SKC_BUILD_VERSION";

        /// <summary> versionCode でのメジャーの重みです。 </summary>
        private const int MAJOR_FACTOR = 1000000;

        /// <summary> versionCode でのマイナーの重みです。 </summary>
        private const int MINOR_FACTOR = 1000;

        /// <summary> versionCode の上限（2100000000）に収まるメジャーの最大値です。 </summary>
        private const int MAX_MAJOR = 2099;

        /// <summary> 桁が隣とぶつからないマイナー・パッチの最大値です。 </summary>
        private const int MAX_MINOR_OR_PATCH = 999;

        /// <summary> 版の形式です。 </summary>
        private static readonly Regex VERSION_PATTERN = new(@"^(\d{1,4})\.(\d{1,3})\.(\d{1,3})$");

        private static bool _hasSavedState;
        private static string _savedBundleVersion;
        private static int _savedAndroidVersionCode;
        private static string _savedIosBuildNumber;
        private static string _savedMacBuildNumber;
    }
}
