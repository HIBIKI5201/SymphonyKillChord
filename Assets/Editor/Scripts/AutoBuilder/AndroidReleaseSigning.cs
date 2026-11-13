using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace KillChord.Editor.AutoBuilder
{
    /// <summary>
    ///     自動ビルドで Android のプレイヤーをアップロード鍵で署名するための設定を、環境変数から一時的に適用します。
    ///     鍵のパスとパスワードは ProjectSettings に保存せず、ビルドが終わったら元の設定へ戻します。
    /// </summary>
    internal static class AndroidReleaseSigning
    {
        /// <summary>
        ///     Android のビルドの直前に呼び、署名の設定を適用します。
        /// </summary>
        /// <param name="target"> これからビルドするプラットフォームです。 </param>
        /// <param name="profileName"> ログに出すビルドプロファイル名です。 </param>
        /// <returns> ビルドを続けてよい場合は true です。署名が必須なのに設定が足りない場合は false です。 </returns>
        public static bool TryApply(BuildTarget target, string profileName)
        {
            if (target != BuildTarget.Android)
            {
                return true;
            }

            // 環境変数から鍵の場所・別名・パスワードを読む。
            string keystorePath = Environment.GetEnvironmentVariable(KEYSTORE_PATH_ENV);
            string keystorePass = Environment.GetEnvironmentVariable(KEYSTORE_PASS_ENV);
            string keyaliasName = Environment.GetEnvironmentVariable(KEYALIAS_NAME_ENV);
            string keyaliasPass = Environment.GetEnvironmentVariable(KEYALIAS_PASS_ENV);
            bool isRequired = string.Equals(
                Environment.GetEnvironmentVariable(SIGNING_REQUIRED_ENV),
                "true",
                StringComparison.OrdinalIgnoreCase);

            // 空の環境変数の名前を集める。値はログに出さない。
            string missingNames = string.Join("・", new[]
            {
                string.IsNullOrEmpty(keystorePath) ? KEYSTORE_PATH_ENV : null,
                string.IsNullOrEmpty(keystorePass) ? KEYSTORE_PASS_ENV : null,
                string.IsNullOrEmpty(keyaliasName) ? KEYALIAS_NAME_ENV : null,
                string.IsNullOrEmpty(keyaliasPass) ? KEYALIAS_PASS_ENV : null,
            }.Where(name => name != null));

            if (missingNames.Length > 0)
            {
                if (isRequired)
                {
                    Debug.LogError($"[{nameof(AndroidReleaseSigning)}] {profileName} はリリース署名が必須ですが、次の設定が空です: {missingNames}。デバッグ署名の APK/AAB は Google Play に提出できないため、ビルドを止めます。");
                    return false;
                }

                Debug.LogWarning($"[{nameof(AndroidReleaseSigning)}] {profileName} は署名の設定が空のため（{missingNames}）、デバッグ署名でビルドします。");
                return true;
            }

            if (!File.Exists(keystorePath))
            {
                if (isRequired)
                {
                    Debug.LogError(
                        $"[{nameof(AndroidReleaseSigning)}] {profileName} の署名に使う鍵ファイルが見つかりません: {keystorePath}");
                    return false;
                }

                Debug.LogWarning($"[{nameof(AndroidReleaseSigning)}] {profileName} の署名に使う鍵ファイルが見つからないため、デバッグ署名でビルドします: {keystorePath}");
                return true;
            }

            // 元の設定を覚えてから、署名の設定を入れる。
            if (!_hasSavedState)
            {
                _savedUseCustomKeystore = PlayerSettings.Android.useCustomKeystore;
                _savedKeystoreName = PlayerSettings.Android.keystoreName;
                _savedKeyaliasName = PlayerSettings.Android.keyaliasName;
                _hasSavedState = true;
            }

            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = keystorePath;
            PlayerSettings.Android.keystorePass = keystorePass;
            PlayerSettings.Android.keyaliasName = keyaliasName;
            PlayerSettings.Android.keyaliasPass = keyaliasPass;

            Debug.Log($"[{nameof(AndroidReleaseSigning)}] {profileName} をアップロード鍵（別名 {keyaliasName}）で署名します。");
            return true;
        }

        /// <summary>
        ///     適用した署名の設定を、適用前の値へ戻します。パスワードは空にします。
        /// </summary>
        public static void Restore()
        {
            if (!_hasSavedState)
            {
                return;
            }

            PlayerSettings.Android.useCustomKeystore = _savedUseCustomKeystore;
            PlayerSettings.Android.keystoreName = _savedKeystoreName;
            PlayerSettings.Android.keyaliasName = _savedKeyaliasName;
            PlayerSettings.Android.keystorePass = string.Empty;
            PlayerSettings.Android.keyaliasPass = string.Empty;
            _hasSavedState = false;
        }

        /// <summary> 鍵ファイル（.keystore / .jks）の絶対パスを渡す環境変数名です。 </summary>
        private const string KEYSTORE_PATH_ENV = "SKC_ANDROID_KEYSTORE_PATH";

        /// <summary> 鍵ファイルのパスワードを渡す環境変数名です。 </summary>
        private const string KEYSTORE_PASS_ENV = "SKC_ANDROID_KEYSTORE_PASS";

        /// <summary> 鍵の別名を渡す環境変数名です。 </summary>
        private const string KEYALIAS_NAME_ENV = "SKC_ANDROID_KEYALIAS_NAME";

        /// <summary> 鍵の別名のパスワードを渡す環境変数名です。 </summary>
        private const string KEYALIAS_PASS_ENV = "SKC_ANDROID_KEYALIAS_PASS";

        /// <summary> true のとき、署名の設定が無ければビルドを止めることを表す環境変数名です。 </summary>
        private const string SIGNING_REQUIRED_ENV = "SKC_ANDROID_SIGNING_REQUIRED";

        private static bool _hasSavedState;
        private static bool _savedUseCustomKeystore;
        private static string _savedKeystoreName;
        private static string _savedKeyaliasName;
    }
}
