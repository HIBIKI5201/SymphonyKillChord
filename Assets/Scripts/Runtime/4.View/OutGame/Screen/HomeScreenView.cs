using KillChord.Runtime.View.OutGame.Common;
using KillChord.Runtime.View.OutGame.Navigation;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Screen
{
    /// <summary>
    ///     ホーム画面 View。
    /// </summary>
    public sealed class HomeScreenView : ScreenViewBase
    {

        /// <summary>
        ///     View を初期化します。
        /// </summary>
        public HomeScreenView(VisualElement rootElement, OutGameUIEvent outGameUIEvent)
            : base(rootElement, outGameUIEvent)
        {
            _stageSelectButton = RootElement.Q<Button>(STAGE_SELECT_BUTTON_NAME)
                ?? throw new System.InvalidOperationException(
                    $"{STAGE_SELECT_BUTTON_NAME} が見つかりません。");

            _skillTreeButton = RootElement.Q<Button>(SKILL_TREE_BUTTON_NAME)
                ?? throw new System.InvalidOperationException(
                    $"{SKILL_TREE_BUTTON_NAME} が見つかりません。");

            _skillBuildButton = RootElement.Q<Button>(SKILL_BUILD_BUTTON_NAME)
                ?? throw new System.InvalidOperationException(
                    $"{SKILL_BUILD_BUTTON_NAME} が見つかりません。");

            _settingButton = RootElement.Q<Button>(SETTING_BUTTON_NAME)
                ?? throw new System.InvalidOperationException(
                    $"{SETTING_BUTTON_NAME} が見つかりません。");

            _rebuildPointsLabel = RootElement.Q<Label>(REBUILD_POINTS_LABEL_NAME)
                ?? throw new System.InvalidOperationException(
                    $"{REBUILD_POINTS_LABEL_NAME} が見つかりません。");

            _unlockPointsLabel = RootElement.Q<Label>(UNLOCK_POINTS_LABEL_NAME)
                ?? throw new System.InvalidOperationException(
                    $"{UNLOCK_POINTS_LABEL_NAME} が見つかりません。");

            _characterImage = RootElement.Q<Image>(CHARACTER_IMAGE_NAME)
                ?? throw new System.InvalidOperationException(
                    $"{CHARACTER_IMAGE_NAME} が見つかりません。");

            RegisterButtonCallbacks();
        }

        /// <summary>
        ///     トップバーに表示するポイントを更新します。
        /// </summary>
        /// <param name="rebuildPoints"> 改造ポイント。 </param>
        /// <param name="unlockPoints"> 解放ポイント。 </param>
        public void SetPoints(int rebuildPoints, int unlockPoints)
        {
            _rebuildPointsLabel.text = rebuildPoints.ToString();
            _unlockPointsLabel.text = unlockPoints.ToString();
        }

        /// <summary>
        ///     キャラクター表示領域に描画するテクスチャを設定します。
        /// </summary>
        /// <param name="texture"> 3Dキャラクターをレンダリングしたテクスチャ。 </param>
        public void SetCharacterTexture(RenderTexture texture)
        {
            _characterImage.style.backgroundImage = Background.FromRenderTexture(texture);
        }

        /// <summary>
        ///     リソースを解放します。
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            UnregisterButtonCallbacks();
        }

        /// <summary>
        ///     各ボタンのコールバックを登録します。
        /// </summary>
        private void RegisterButtonCallbacks()
        {
            _stageSelectButton.MakeNavigable();
            _skillTreeButton.MakeNavigable();
            _skillBuildButton.MakeNavigable();
            _settingButton.MakeNavigable();

            _stageSelectActivation = _stageSelectButton.RegisterActivation(HandleStageSelectActivationHandler);
            _skillTreeActivation = _skillTreeButton.RegisterActivation(HandleSkillTreeActivationHandler);
            _skillBuildActivation = _skillBuildButton.RegisterActivation(HandleSkillBuildActivationHandler);
            _settingActivation = _settingButton.RegisterActivation(HandleSettingActivationHandler);

            // マウスホバーと同じ見た目(白パネルのon画像)をコントローラー選択時にも反映する。
            _stageSelectFocusSync = _stageSelectButton.SyncFocusClassToParent(FOCUSED_CLASS_NAME);
            _skillTreeFocusSync = _skillTreeButton.SyncFocusClassToParent(FOCUSED_CLASS_NAME);
            _skillBuildFocusSync = _skillBuildButton.SyncFocusClassToParent(FOCUSED_CLASS_NAME);

            // hover/focus中のスケール演出は .btn-scale-feedback (Button.uss) + パルスManipulatorで行う。
            // ボタン単体ではなく、背景パネルも含むラッパー(親要素)に付与し、両方をまとめて拡縮させる。
            _stageSelectPulse = _stageSelectButton.parent.EnableButtonPulseAnimation();
            _skillTreePulse = _skillTreeButton.parent.EnableButtonPulseAnimation();
            _skillBuildPulse = _skillBuildButton.parent.EnableButtonPulseAnimation();
        }

        /// <summary>
        ///     各ボタンのコールバックを登録解除します。
        /// </summary>
        private void UnregisterButtonCallbacks()
        {
            _stageSelectActivation?.Dispose();
            _skillTreeActivation?.Dispose();
            _skillBuildActivation?.Dispose();
            _settingActivation?.Dispose();

            _stageSelectFocusSync?.Dispose();
            _skillTreeFocusSync?.Dispose();
            _skillBuildFocusSync?.Dispose();

            if (_stageSelectPulse != null)
            {
                _stageSelectButton.parent.RemoveManipulator(_stageSelectPulse);
            }

            if (_skillTreePulse != null)
            {
                _skillTreeButton.parent.RemoveManipulator(_skillTreePulse);
            }

            if (_skillBuildPulse != null)
            {
                _skillBuildButton.parent.RemoveManipulator(_skillBuildPulse);
            }
        }

        /// <summary>
        ///     作戦ボタンが作動したときのコールバックです。
        ///     作戦画面を表示するイベントを発行します。
        /// </summary>
        private void HandleStageSelectActivationHandler()
        {
            OutGameUIEvent.OnShownStageSelectionScreen?.Invoke();
        }

        /// <summary>
        ///     研究ボタンが作動したときのコールバックです。
        ///     研究画面を表示するイベントを発行します。
        /// </summary>
        private void HandleSkillTreeActivationHandler()
        {
            OutGameUIEvent.OnShownSkillTreeScreen?.Invoke();
        }

        /// <summary>
        ///     改造ボタンが作動したときのコールバックです。
        ///     改造画面を表示するイベントを発行します。
        /// </summary>
        private void HandleSkillBuildActivationHandler()
        {
            OutGameUIEvent.OnShownSkillBuildScreen?.Invoke();
        }

        /// <summary>
        ///     設定ボタンが作動したときのコールバックです。
        ///     設定画面を表示するイベントを発行します。
        /// </summary>
        private void HandleSettingActivationHandler()
        {
            OutGameUIEvent.OnShownSettingScreen?.Invoke();
        }


        /// <inheritdoc />
        protected override VisualElement InitialFocusElement => _stageSelectButton;

        private const string STAGE_SELECT_BUTTON_NAME = "StageSelect";
        private const string SKILL_TREE_BUTTON_NAME = "SkillTree";
        private const string SKILL_BUILD_BUTTON_NAME = "SkillBuild";
        private const string SETTING_BUTTON_NAME = "OptionIcon";
        private const string REBUILD_POINTS_LABEL_NAME = "RebuildPointsValueLabel";
        private const string UNLOCK_POINTS_LABEL_NAME = "UnlockPointsValueLabel";
        private const string CHARACTER_IMAGE_NAME = "CharacterImage";
        private const string FOCUSED_CLASS_NAME = "is-focused";

        private readonly Button _stageSelectButton;
        private readonly Button _skillTreeButton;
        private readonly Button _skillBuildButton;
        private readonly Button _settingButton;
        private readonly Label _rebuildPointsLabel;
        private readonly Label _unlockPointsLabel;
        private readonly Image _characterImage;
        private IDisposable _stageSelectActivation;
        private IDisposable _skillTreeActivation;
        private IDisposable _skillBuildActivation;
        private IDisposable _settingActivation;
        private IDisposable _stageSelectFocusSync;
        private IDisposable _skillTreeFocusSync;
        private IDisposable _skillBuildFocusSync;
        private IManipulator _stageSelectPulse;
        private IManipulator _skillTreePulse;
        private IManipulator _skillBuildPulse;
    }
}
