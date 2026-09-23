using KillChord.Runtime.Composition.OutGame.Bootstrap;
using KillChord.Runtime.Composition.Persistent.Music;
using KillChord.Runtime.View.OutGame.Navigation;
using KillChord.Runtime.View.OutGame.Screen;
using KillChord.Runtime.View.OutGame.Setting;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.Composition.OutGame.Setting
{
    /// <summary>
    ///     設定画面を初期化するモジュールです。
    /// </summary>
    public sealed class SettingComposition : OutGameInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(SettingComposition);

        /// <summary> 実行順です。 </summary>
        public override int Order => 140;

        [SerializeField, Tooltip("設定画面を含むUI Document")]
        private UIDocument _uiDocument;

        private AudioSettingsView _audioSettingsView;

        /// <summary>
        ///     設定画面を初期化します。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Build()
        {
            if (_uiDocument == null
                || !ServiceLocator.TryGetInstance(out AudioSettingsModuleContainer audioSettingsContainer)
                || !ServiceLocator.TryGetInstance(out _outGameUIEvent))
            {
                Debug.LogError(
                    $"[{nameof(SettingComposition)}] 設定画面の構築に必要な参照を取得できませんでした。",
                    this);
                return false;
            }

            VisualElement settingRoot = _uiDocument.rootVisualElement.Q<VisualElement>(SETTING_ROOT_NAME);
            if (settingRoot == null)
            {
                Debug.LogError(
                    $"[{nameof(SettingComposition)}] {SETTING_ROOT_NAME} が見つかりませんでした。",
                    this);
                return false;
            }

            try
            {
                HierarchicalNavigationScope settingNavigationScope = new(settingRoot);
                _settingCategoryView = new SettingCategoryView(settingRoot, settingNavigationScope);
                _audioSettingsView = new AudioSettingsView(
                    settingRoot,
                    audioSettingsContainer.ViewModel,
                    audioSettingsContainer.Command);
            }
            catch (Exception exception)
            {
                _audioSettingsView?.Dispose();
                _settingCategoryView?.Dispose();
                _audioSettingsView = null;
                _settingCategoryView = null;
                Debug.LogError(
                    $"[{nameof(SettingComposition)}] 設定画面のView構築に失敗しました。{exception}",
                    this);
                return false;
            }

            _outGameUIEvent.OnShownSettingScreen += _settingCategoryView.ShowDefaultCategory;
            return true;
        }

        /// <summary>
        ///     設定画面のコールバックを解除する。
        /// </summary>
        public override void Shutdown()
        {
            if (_outGameUIEvent != null && _settingCategoryView != null)
            {
                _outGameUIEvent.OnShownSettingScreen -= _settingCategoryView.ShowDefaultCategory;
            }

            _audioSettingsView?.Dispose();
            _settingCategoryView?.Dispose();
            _audioSettingsView = null;
            _settingCategoryView = null;
            _outGameUIEvent = null;
        }

        private const string SETTING_ROOT_NAME = "SettingContainer";

        private SettingCategoryView _settingCategoryView;
        private OutGameUIEvent _outGameUIEvent;
    }
}
