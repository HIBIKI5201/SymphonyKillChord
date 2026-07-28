using KillChord.Runtime.Adaptor.OutGame.SkillBuild;
using System;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.SkillBuild
{
    /// <summary>
    ///     スキル 1 枚分のアイコン UI を表す View クラス。
    ///     SkillElementDragAndDropSetup によってドラッグ&ドロップのマニピュレーターが付与される。
    ///     TODO : スキルの情報が実装されたときに、スキルの名前やタイプを表示するように拡張する。
    /// </summary>
    public class SkillElementView
    {
        /// <summary>
        ///     SkillElementView のコンストラクタ。
        ///     VisualElement の参照を取得し、draggable クラスを付与してクリックイベントを登録する。
        /// </summary>
        /// <param name="root"> スキル要素のルート VisualElement。 </param>
        public SkillElementView(VisualElement root)
        {
            RootElement = root
                ?? throw new ArgumentNullException(nameof(root));

            _icon = root.Q<Image>(ICON_NAME)
                ?? throw new ArgumentNullException(
                    $"[{nameof(SkillElementView)}] {ICON_NAME} が見つかりませんでした。");

            _nameLabel = root.Q<Label>(NAME_LABEL_NAME)
                ?? throw new ArgumentNullException(
                    $"[{nameof(SkillElementView)}] {NAME_LABEL_NAME} が見つかりませんでした。");

            // ドラッグ&ドロップセットアップがクラス名で draggable 要素を検索するため
            // コンストラクタで付与する。
            RootElement.AddToClassList(DRAGGABLE_CLASS_NAME);

            RootElement.RegisterCallback<ClickEvent>(OnClickHandler);
        }

        /// <summary> スキルが選択されたときに発火するイベント。 </summary>
        public event Action<SkillViewDataDTO> OnSelected;

        /// <summary> スキル要素のルート VisualElement。マニピュレーターの付与に使用する。 </summary>
        public VisualElement RootElement { get; }

        /// <summary> 現在バインドされているスキルデータ。 </summary>
        public SkillViewDataDTO? CurrentData { get; private set; }

        /// <summary>
        ///     DTO を受け取りアイコン・名前・タイプを表示に反映する。
        /// </summary>
        /// <param name="dto"> バインドするスキルデータ。 </param>
        public void Bind(in SkillViewDataDTO dto)
        {
            CurrentData = dto;
            _icon.sprite = dto.Icon;
            _nameLabel.text = dto.SkillId;
        }

        /// <summary>
        ///     表示をクリアしてバインドを解除する。
        /// </summary>
        public void Unbind()
        {
            CurrentData = null;
            _icon.sprite = null;
            _nameLabel.text = string.Empty;
        }

        /// <summary>
        ///     SkillElementView を破棄し、イベントの登録を解除する。
        /// </summary>
        public void Dispose()
        {
            RootElement.UnregisterCallback<ClickEvent>(OnClickHandler);
        }

        private const string DRAGGABLE_CLASS_NAME = "draggable";
        private const string ICON_NAME = "SkillIcon";
        private const string NAME_LABEL_NAME = "SkillName";

        private readonly Image _icon;
        private readonly Label _nameLabel;

        /// <summary>
        ///     クリック時に OnSelected イベントを発火するハンドラ。
        /// </summary>
        private void OnClickHandler(ClickEvent evt)
        {
            if (CurrentData.HasValue)
            {
                OnSelected?.Invoke(CurrentData.Value);
            }
        }
    }
}