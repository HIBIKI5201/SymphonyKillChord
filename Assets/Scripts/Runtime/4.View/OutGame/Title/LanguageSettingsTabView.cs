using KillChord.Runtime.Adaptor.Persistent.Environment;
using KillChord.Runtime.View.OutGame.Common;
using R3;
using System;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Title
{
    /// <summary>
    ///     タイトル画面のメニュータブにある言語設定項目を共通環境設定へ接続するView。
    /// </summary>
    public sealed class LanguageSettingsTabView : IDisposable
    {
        /// <summary>
        ///     言語設定タブを初期化する。
        /// </summary>
        public LanguageSettingsTabView(
            VisualElement rootElement,
            IEnvironmentSettingsViewModel environmentSettingsViewModel,
            IEnvironmentSettingsCommand environmentSettingsCommand)
        {
            _environmentSettingsCommand = environmentSettingsCommand
                ?? throw new ArgumentNullException(nameof(environmentSettingsCommand));
            if (environmentSettingsViewModel == null)
            {
                throw new ArgumentNullException(nameof(environmentSettingsViewModel));
            }

            _languagePrevButton = Require<Button>(rootElement, LANGUAGE_PREV_BUTTON_NAME);
            _languageNextButton = Require<Button>(rootElement, LANGUAGE_NEXT_BUTTON_NAME);
            _languageValueLabel = Require<Label>(rootElement, LANGUAGE_VALUE_LABEL_NAME);
            _subscriptions = new CompositeDisposable();

            _languagePrevButtonPreset = _languagePrevButton.ApplyBasicButtonPreset(HandleLanguagePrevButtonClicked);
            _languageNextButtonPreset = _languageNextButton.ApplyBasicButtonPreset(HandleLanguageNextButtonClicked);

            environmentSettingsViewModel.LanguageLabel
                .Subscribe(HandleLanguageLabelPublished)
                .AddTo(_subscriptions);
        }

        /// <summary>
        ///     登録済みコールバックを解除する。
        /// </summary>
        public void Dispose()
        {
            _languagePrevButtonPreset.Dispose();
            _languageNextButtonPreset.Dispose();
            _subscriptions.Dispose();
        }

        private const string LANGUAGE_PREV_BUTTON_NAME = "LanguagePrevButton";
        private const string LANGUAGE_NEXT_BUTTON_NAME = "LanguageNextButton";
        private const string LANGUAGE_VALUE_LABEL_NAME = "LanguageValueLabel";
        private const int CYCLE_PREVIOUS_DIRECTION = -1;
        private const int CYCLE_NEXT_DIRECTION = 1;

        private readonly IEnvironmentSettingsCommand _environmentSettingsCommand;
        private readonly Button _languagePrevButton;
        private readonly Button _languageNextButton;
        private readonly Label _languageValueLabel;
        private readonly CompositeDisposable _subscriptions;
        private readonly IDisposable _languagePrevButtonPreset;
        private readonly IDisposable _languageNextButtonPreset;

        /// <summary>
        ///     表示言語を前へ切り替える。
        ///     <para>
        ///         タイトル画面のメニューには環境設定画面のような確定操作(保存ボタン)がないため、
        ///         BGM/効果音の音量設定と同様に切り替えと同時に確定して保存する。
        ///     </para>
        /// </summary>
        private void HandleLanguagePrevButtonClicked()
        {
            CycleLanguageAndConfirm(CYCLE_PREVIOUS_DIRECTION);
        }

        /// <summary>
        ///     表示言語を次へ切り替える。切り替えと同時に確定して保存する。
        /// </summary>
        private void HandleLanguageNextButtonClicked()
        {
            CycleLanguageAndConfirm(CYCLE_NEXT_DIRECTION);
        }

        /// <summary>
        ///     表示言語を表示へ反映する。
        /// </summary>
        private void HandleLanguageLabelPublished(string label)
        {
            _languageValueLabel.text = label;
        }

        /// <summary>
        ///     表示言語を切り替え、即座に確定して保存する。
        /// </summary>
        private void CycleLanguageAndConfirm(int direction)
        {
            _environmentSettingsCommand.CycleLanguage(direction);
            _environmentSettingsCommand.ConfirmChanges();
        }

        /// <summary>
        ///     必須UI要素を取得する。
        /// </summary>
        private static T Require<T>(VisualElement rootElement, string elementName)
            where T : VisualElement
        {
            return rootElement.Q<T>(elementName)
                ?? throw new InvalidOperationException(
                    $"[{nameof(LanguageSettingsTabView)}] {elementName} が見つかりませんでした。");
        }
    }
}
