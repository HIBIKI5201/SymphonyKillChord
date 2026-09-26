using System;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace KillChord.Runtime.View.Persistent.Localization
{
    /// <summary>
    ///     Localizationの初期化完了を待ってから処理を実行するクラス。
    /// </summary>
    public static class LocalizationInitializer
    {
        /// <summary>
        ///     Localizationの初期化完了後に処理を実行します。
        ///     初期化前にLocalizedStringやLocalizedAssetを参照するとSelectedLocaleが未設定で例外になるため、
        ///     購読処理はこのメソッドを介して実行します。
        /// </summary>
        /// <param name="onInitialized"> 初期化完了後に実行する処理です。引数にはLocaleを取得できたかを渡します。 </param>
        /// <exception cref="ArgumentNullException"> 実行する処理がnullの場合に発生します。 </exception>
        public static void RunWhenInitialized(Action<bool> onInitialized)
        {
            if (onInitialized == null)
            {
                throw new ArgumentNullException(nameof(onInitialized));
            }

            AsyncOperationHandle<LocalizationSettings> initializationOperation =
                LocalizationSettings.InitializationOperation;

            // 初期化済みの場合は待機せずに実行する。
            if (initializationOperation.IsDone)
            {
                onInitialized(HasSelectedLocale());
                return;
            }

            initializationOperation.Completed += _ => onInitialized(HasSelectedLocale());
        }

        /// <summary>
        ///     表示に使用するLocaleが設定済みかを返します。
        /// </summary>
        /// <returns> Localeが設定済みの場合はtrueです。 </returns>
        private static bool HasSelectedLocale()
        {
            return LocalizationSettings.SelectedLocale != null;
        }
    }
}
