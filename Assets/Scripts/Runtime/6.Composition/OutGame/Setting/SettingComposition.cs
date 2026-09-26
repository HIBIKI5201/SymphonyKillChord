using KillChord.Runtime.Composition.OutGame.Bootstrap;
using KillChord.Runtime.Composition.Persistent.Environment;
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
        private EnvironmentSettingsView _environmentSettingsView;

        /// <summary>
        ///     設定画面を初期化します。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Build()
        {
            // 必要な参照と、設定画面のルート要素を取得する。
            if (_uiDocument == null
                || !ServiceLocator.TryGetInstance(out AudioSettingsModuleContainer audioSettingsContainer)
                || !ServiceLocator.TryGetInstance(out EnvironmentSettingsModuleContainer environmentSettingsContainer)
                || !ServiceLocator.TryGetInstance(out _outGameUIEvent)
                || !ServiceLocator.TryGetInstance(out _settingScreenView))
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

            // メニュー・オーディオ設定・環境設定のビューを作る。失敗した場合は作ったものを破棄する。
            try
            {
                HierarchicalNavigationScope settingNavigationScope = new(settingRoot);
                _settingMenuView = new SettingMenuView(settingRoot, settingNavigationScope);
                _audioSettingsView = new AudioSettingsView(
                    settingRoot,
                    audioSettingsContainer.ViewModel,
                    audioSettingsContainer.Command);
                _environmentSettingsView = new EnvironmentSettingsView(
                    settingRoot,
                    environmentSettingsContainer.ViewModel,
                    environmentSettingsContainer.Command);
            }
            catch (Exception exception)
            {
                _environmentSettingsView?.Dispose();
                _audioSettingsView?.Dispose();
                _settingMenuView?.Dispose();
                _environmentSettingsView = null;
                _audioSettingsView = null;
                _settingMenuView = null;
                Debug.LogError(
                    $"[{nameof(SettingComposition)}] 設定画面のView構築に失敗しました。{exception}",
                    this);
                return false;
            }

            // 設定画面の表示と戻る操作をメニューに結びつける。
            _outGameUIEvent.OnShownSettingScreen += _settingMenuView.ShowMenu;
            _settingScreenView.TryNavigateBack = _settingMenuView.TryGoBack;
            _settingMenuView.OnCancelEnvironmentChanges = _environmentSettingsView.CancelPendingChanges;
            return true;
        }

        /// <summary>
        ///     設定画面のコールバックを解除する。
        /// </summary>
        public override void Shutdown()
        {
            if (_outGameUIEvent != null && _settingMenuView != null)
            {
                _outGameUIEvent.OnShownSettingScreen -= _settingMenuView.ShowMenu;
            }

            if (_settingScreenView != null)
            {
                _settingScreenView.TryNavigateBack = null;
            }

            if (_settingMenuView != null)
            {
                _settingMenuView.OnCancelEnvironmentChanges = null;
            }

            _environmentSettingsView?.Dispose();
            _audioSettingsView?.Dispose();
            _settingMenuView?.Dispose();
            _environmentSettingsView = null;
            _audioSettingsView = null;
            _settingMenuView = null;
            _settingScreenView = null;
            _outGameUIEvent = null;
        }

        private const string SETTING_ROOT_NAME = "SettingContainer";

        private SettingMenuView _settingMenuView;
        private SettingScreenView _settingScreenView;
        private OutGameUIEvent _outGameUIEvent;
    }
}
