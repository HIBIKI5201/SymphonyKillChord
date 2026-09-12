using KillChord.Runtime.Adaptor.OutGame.Audio;
using KillChord.Runtime.Adaptor.OutGame.SkillBuild;
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.SkillBuild
{
    /// <summary>
    ///     入手済みスキル一覧を表示する View。
    /// </summary>
    public sealed class SkillListView : IDisposable
    {
        /// <summary>
        ///     SkillListView を初期化する。
        /// </summary>
        /// <param name="scrollView"> 表示先。 </param>
        /// <param name="skillElementTemplate"> 各要素のテンプレート。 </param>
        /// <param name="onSkillElementCreated"> 要素生成時のコールバック。 </param>
        /// <param name="soundEffectCommand"> UI操作音の再生コマンド。 </param>
        /// <exception cref="ArgumentNullException"></exception>
        public SkillListView(
            VisualElement scrollView,
            VisualTreeAsset skillElementTemplate,
            Action<VisualElement> onSkillElementCreated,
            IUISoundEffectCommand soundEffectCommand)
        {
            _scrollView = scrollView ?? throw new ArgumentNullException(nameof(scrollView));
            _skillElementTemplate = skillElementTemplate ?? throw new ArgumentNullException(nameof(skillElementTemplate));
            _onSkillElementCreated = onSkillElementCreated;
            _soundEffectCommand = soundEffectCommand;
            ConfigureScrollView();
        }

        /// <summary> スキル選択時にスキル ID を通知する。 </summary>
        public event Action<int> OnSkillSelected;

        /// <summary>
        ///     スキル一覧を再構築する。
        /// </summary>
        /// <param name="skills"> 表示するスキル一覧。 </param>
        /// <exception cref="ArgumentNullException"></exception>
        public void SetSkills(IReadOnlyList<SkillViewData> skills)
        {
            if (skills == null)
            {
                throw new ArgumentNullException(nameof(skills));
            }

            Clear();
            for (int i = 0; i < skills.Count; i++)
            {
                TemplateContainer rootElement = _skillElementTemplate.Instantiate();
                SkillElementView skillElementView = new(rootElement, _soundEffectCommand);
                SkillViewData data = skills[i];
                skillElementView.Bind(in data);
                skillElementView.OnSelected += HandleSkillSelectedHandler;
                _skillElementViews.Add(skillElementView);
                _scrollView.Add(rootElement);
                _onSkillElementCreated?.Invoke(rootElement);
            }
        }

        /// <summary>
        ///     選択表示を更新する。
        /// </summary>
        /// <param name="selectedSkillId"> 明示選択中のスキル ID。 </param>
        public void SetSelectedSkill(int? selectedSkillId)
        {
            for (int i = 0; i < _skillElementViews.Count; i++)
            {
                SkillViewData? data = _skillElementViews[i].CurrentData;
                bool isSelected =
                    selectedSkillId.HasValue &&
                    data.HasValue &&
                    data.Value.SkillId == selectedSkillId.Value;
                _skillElementViews[i].SetSelected(isSelected);
            }
        }

        /// <summary>
        ///     一覧要素を破棄する。
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < _skillElementViews.Count; i++)
            {
                SkillElementView view = _skillElementViews[i];
                view.OnSelected -= HandleSkillSelectedHandler;
                view.Dispose();
                view.RootElement.RemoveFromHierarchy();
            }

            _skillElementViews.Clear();
            _scrollView.Clear();
        }

        /// <summary>
        ///     一覧を破棄する。
        /// </summary>
        public void Dispose()
        {
            Clear();
            OnSkillSelected = null;
        }

        private readonly VisualElement _scrollView;
        private readonly VisualTreeAsset _skillElementTemplate;
        private readonly Action<VisualElement> _onSkillElementCreated;
        private readonly IUISoundEffectCommand _soundEffectCommand;
        private readonly List<SkillElementView> _skillElementViews = new();

        /// <summary>
        ///     ScrollView の基本設定を行う。
        /// </summary>
        private void ConfigureScrollView()
        {
            if (_scrollView is ScrollView scrollView)
            {
                scrollView.mode = ScrollViewMode.Horizontal;
                scrollView.horizontalScrollerVisibility = ScrollerVisibility.Auto;
                scrollView.verticalScrollerVisibility = ScrollerVisibility.Hidden;
            }
        }

        /// <summary>
        ///     子要素からの選択通知を転送する。
        /// </summary>
        /// <param name="skillId"> スキル ID。 </param>
        private void HandleSkillSelectedHandler(int skillId)
        {
            OnSkillSelected?.Invoke(skillId);
        }

        /// <summary>
        ///     スキル ID に対応するルート要素を検索する。
        /// </summary>
        /// <param name="skillId"> スキル ID。 </param>
        /// <returns> 対応するルート要素。 </returns>
        internal VisualElement FindSkillElementRoot(int skillId)
        {
            for (int i = 0; i < _skillElementViews.Count; i++)
            {
                SkillViewData? data = _skillElementViews[i].CurrentData;
                if (data.HasValue && data.Value.SkillId == skillId)
                {
                    return _skillElementViews[i].RootElement;
                }
            }

            return null;
        }
    }
}
