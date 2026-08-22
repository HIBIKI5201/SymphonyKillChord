using KillChord.Runtime.Adaptor.OutGame.Audio;
using KillChord.Runtime.Adaptor.OutGame.SkillBuild;
using System;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.SkillBuild
{
    /// <summary>
    ///     スキル1件分の一覧・スロット表示を管理する View。
    /// </summary>
    public sealed class SkillElementView : IDisposable
    {
        /// <summary>
        ///     View を初期化する。
        /// </summary>
        /// <param name="rootElement"> スキル要素のルート。 </param>
        /// <param name="soundEffectCommand"> UI操作音の再生コマンド。 </param>
        /// <exception cref="ArgumentNullException"></exception>
        public SkillElementView(
            VisualElement rootElement,
            IUISoundEffectCommand soundEffectCommand)
        {
            RootElement = rootElement ?? throw new ArgumentNullException(nameof(rootElement));
            _soundEffectCommand = soundEffectCommand;
            _icon = RootElement.Q<Image>(ICON_NAME)
                ?? throw new InvalidOperationException(
                    $"[{nameof(SkillElementView)}] {ICON_NAME} が見つかりませんでした。");
            _nameLabel = RootElement.Q<Label>(NAME_LABEL_NAME)
                ?? throw new InvalidOperationException(
                    $"[{nameof(SkillElementView)}] {NAME_LABEL_NAME} が見つかりませんでした。");

            RootElement.AddToClassList(DRAGGABLE_CLASS_NAME);
            RootElement.RegisterCallback<ClickEvent>(HandleClickHandler);
        }

        /// <summary> スキルが選択された時にスキル ID を通知する。 </summary>
        public event Action<int> OnSelected;

        /// <summary> スキル要素のルート。 </summary>
        public VisualElement RootElement { get; }

        /// <summary> バインド中の表示データ。 </summary>
        public SkillViewData? CurrentData { get; private set; }

        /// <summary>
        ///     表示データを反映する。
        /// </summary>
        /// <param name="data"> 表示データ。 </param>
        public void Bind(in SkillViewData data)
        {
            CurrentData = data;
            RootElement.name = $"{SKILL_ELEMENT_NAME_PREFIX}{data.SkillId}";
            RootElement.userData = data.SkillId;
            _icon.sprite = data.Icon;
            _nameLabel.text = data.DisplayName;
        }

        /// <summary>
        ///     選択表示を更新する。
        /// </summary>
        /// <param name="isSelected"> 選択中の場合は true。 </param>
        public void SetSelected(bool isSelected)
        {
            RootElement.EnableInClassList(SELECTED_CLASS_NAME, isSelected);
        }

        /// <summary>
        ///     イベント購読を解除する。
        /// </summary>
        public void Dispose()
        {
            RootElement.UnregisterCallback<ClickEvent>(HandleClickHandler);
            OnSelected = null;
            CurrentData = null;
        }

        private const string DRAGGABLE_CLASS_NAME = "draggable";
        private const string DRAG_COMPLETED_CLASS_NAME = "drag-just-completed";
        private const string SELECTED_CLASS_NAME = "is-selected";
        private const string ICON_NAME = "skill-icon";
        private const string NAME_LABEL_NAME = "skill-name";
        private const string SKILL_ELEMENT_NAME_PREFIX = "skill-element-";

        private readonly Image _icon;
        private readonly Label _nameLabel;
        private readonly IUISoundEffectCommand _soundEffectCommand;

        /// <summary>
        ///     クリックを選択通知へ変換する。
        /// </summary>
        /// <param name="evt"> クリックイベント。 </param>
        private void HandleClickHandler(ClickEvent evt)
        {
            if (RootElement.ClassListContains(DRAG_COMPLETED_CLASS_NAME) ||
                !CurrentData.HasValue)
            {
                return;
            }

            _soundEffectCommand?.Play(UISoundEffectKind.Select);
            OnSelected?.Invoke(CurrentData.Value.SkillId);
        }
    }
}
