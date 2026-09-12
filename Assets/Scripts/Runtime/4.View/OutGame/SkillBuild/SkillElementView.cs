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
            _genreBadge = RootElement.Q<Image>(GENRE_BADGE_NAME)
                ?? throw new InvalidOperationException(
                    $"[{nameof(SkillElementView)}] {GENRE_BADGE_NAME} が見つかりませんでした。");
            _equippedBadge = RootElement.Q<VisualElement>(EQUIPPED_BADGE_NAME)
                ?? throw new InvalidOperationException(
                    $"[{nameof(SkillElementView)}] {EQUIPPED_BADGE_NAME} が見つかりませんでした。");
            _lockedLabel = RootElement.Q<Label>(LOCKED_LABEL_NAME)
                ?? throw new InvalidOperationException(
                    $"[{nameof(SkillElementView)}] {LOCKED_LABEL_NAME} が見つかりませんでした。");

            RootElement.AddToClassList(DRAGGABLE_CLASS_NAME);
            RootElement.RegisterCallback<ClickEvent>(HandleClickHandler);
            _genreBadge.RegisterCallback<ClickEvent>(HandleGenreBadgeClickHandler);
        }

        /// <summary> スキルが選択された時にスキル ID を通知する。 </summary>
        public event Action<int> OnSelected;

        /// <summary> ジャンルバッジが選択された時にジャンル ID を通知する。 </summary>
        public event Action<int> OnGenreBadgeSelected;

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
            _genreBadge.sprite = data.GenreIcon;
            _genreBadge.style.display = data.GenreIcon == null ? DisplayStyle.None : DisplayStyle.Flex;
            RootElement.EnableInClassList(LOCKED_CLASS_NAME, !data.IsUnlocked);
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
        ///     装備中バッジ表示を更新する。
        /// </summary>
        /// <param name="isEquipped"> 装備中の場合は true。 </param>
        public void SetEquipped(bool isEquipped)
        {
            RootElement.EnableInClassList(EQUIPPED_CLASS_NAME, isEquipped);
        }

        /// <summary>
        ///     イベント購読を解除する。
        /// </summary>
        public void Dispose()
        {
            RootElement.UnregisterCallback<ClickEvent>(HandleClickHandler);
            _genreBadge.UnregisterCallback<ClickEvent>(HandleGenreBadgeClickHandler);
            OnSelected = null;
            OnGenreBadgeSelected = null;
            CurrentData = null;
        }

        private const string DRAGGABLE_CLASS_NAME = "draggable";
        private const string DRAG_COMPLETED_CLASS_NAME = "drag-just-completed";
        private const string SELECTED_CLASS_NAME = "is-selected";
        private const string EQUIPPED_CLASS_NAME = "is-equipped";
        private const string LOCKED_CLASS_NAME = "is-locked";
        private const string ICON_NAME = "skill-icon";
        private const string NAME_LABEL_NAME = "skill-name";
        private const string GENRE_BADGE_NAME = "skill-genre-badge";
        private const string EQUIPPED_BADGE_NAME = "skill-equipped-badge";
        private const string LOCKED_LABEL_NAME = "skill-locked-label";
        private const string SKILL_ELEMENT_NAME_PREFIX = "skill-element-";

        private readonly Image _icon;
        private readonly Label _nameLabel;
        private readonly Image _genreBadge;
        private readonly VisualElement _equippedBadge;
        private readonly Label _lockedLabel;
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

        /// <summary>
        ///     ジャンルバッジのクリックを絞り込み通知へ変換する。
        /// </summary>
        /// <param name="evt"> クリックイベント。 </param>
        private void HandleGenreBadgeClickHandler(ClickEvent evt)
        {
            evt.StopPropagation();

            if (!CurrentData.HasValue ||
                CurrentData.Value.GenreIds == null ||
                CurrentData.Value.GenreIds.Length == 0)
            {
                return;
            }

            OnGenreBadgeSelected?.Invoke(CurrentData.Value.GenreIds[0]);
        }
    }
}
