using KillChord.Runtime.Adaptor.OutGame.SkillTree;
using KillChord.Runtime.View.OutGame.Navigation;
using KillChord.Runtime.View.OutGame.Screen;
using KillChord.Runtime.View.Persistent.Localization;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;

namespace KillChord.Runtime.View.OutGame.SkillTree
{
    /// <summary>
    ///     スキルプレビュー動画のViewクラス。
    /// </summary>
    public class PreviewVideoScreenView : ScreenViewBase, IDisposable, IPreviewVideoScreenViewModel, IPreviewVideoScreenViewShowable
    {
        /// <summary>
        ///     プレビュー動画の表示に使う要素・プレイヤー・動画を指定して生成する。
        /// </summary>
        public PreviewVideoScreenView(VisualElement root, OutGameUIEvent outGameUIEvent, VideoPlayer player, Dictionary<int, VideoClip> videoClips) : base(root, outGameUIEvent)
        {
            _root = root;
            _outGameUIEvent = outGameUIEvent;
            _closeButton = _root.Q(name: ELEMENT_NAME_CLOSE_BUTTON);
            _player = player;
            _videoClips = videoClips;

            RegisterEvents();
            _closeButtonLocalizedText = new LocalizedElementText(
                UI_COMMON_TABLE,
                "ui.skill_tree.close_preview",
                text => ((Button)_closeButton).text = text);
        }

        /// <summary>
        ///     プレイビュー動画を再生する。
        /// </summary>
        /// <param name="nodeId"></param>
        public void PlayPreviewVideo(int nodeId)
        {
            if (!_videoClips.ContainsKey(nodeId))
            {
                Debug.LogWarning("[PreviewVideoScreenView] このスキルには動画がありません。");
                return;
            }
            _player.clip = _videoClips[nodeId];
            _player.Play();
        }

        /// <summary>
        ///     プレイビュー動画の再生を停止する。
        /// </summary>
        public void StopPreviewVideo()
        {
            _player.Stop();
        }

        /// <summary>
        ///     閉じるボタンの登録とローカライズ文言の購読を解除する。
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            _closeButtonActivation?.Dispose();
            _closeButtonLocalizedText?.Dispose();
        }

        private Dictionary<int, VideoClip> _videoClips;
        private VideoPlayer _player;
        private VisualElement _root;
        /// <inheritdoc />
        protected override VisualElement CancelTargetElement => _closeButton;

        private VisualElement _closeButton;
        private OutGameUIEvent _outGameUIEvent;
        private IDisposable _closeButtonActivation;
        private LocalizedElementText _closeButtonLocalizedText;

        private const string ELEMENT_NAME_CLOSE_BUTTON = "ClosePreviewButton";
        private const string UI_COMMON_TABLE = "UICommon";

        /// <summary>
        ///     閉じるボタンの操作を登録する。フォーカス移動の対象からは外す。
        /// </summary>
        private void RegisterEvents()
        {
            // キャンセル操作で閉じられるため、フォーカス移動の対象からは外す。
            _closeButton.ExcludeFromNavigation();
            _closeButtonActivation = _closeButton.RegisterActivation(HandleCloseButtonActivationHandler);
        }

        /// <summary>
        ///     動画の閉じるボタンを押下時の処理。
        /// </summary>
        private void HandleCloseButtonActivationHandler()
        {
            StopPreviewVideo();
            _outGameUIEvent.OnSkillPreviewCloseButtonClicked?.Invoke();
        }
    }
}
