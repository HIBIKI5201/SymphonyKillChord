using KillChord.Runtime.Adaptor.OutGame.Screen;
using KillChord.Runtime.View.OutGame.Navigation;
using KillChord.Runtime.View.OutGame.Screen;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Title
{
    /// <summary>
    ///     クレジット画面の View クラス。
    /// </summary>
    public class CreditScreenView : ScreenViewBase, IMemberListApplicable
    {
        /// <summary>
        ///    クレジット画面の View を初期化します。
        /// </summary>
        /// <param name="rootElement"> クレジット画面のルート要素です。 </param>
        /// <param name="outGameUIEvent"> アウトゲーム UI イベントです。 </param>
        public CreditScreenView(VisualElement rootElement, OutGameUIEvent outGameUIEvent)
            : base(rootElement, outGameUIEvent)
        {
            Initialize(rootElement);
            RegisterButtonCallbacks();
        }

        /// <summary>
        ///   クレジット画面の View のリソースを解放します。
        /// </summary>
        public override void Dispose()
        {
            UnregisterButtonCallbacks();
            base.Dispose();
        }

        /// <summary>
        ///     制作メンバー一覧を UI へ反映します。
        /// </summary>
        /// <param name="members"> 反映する制作メンバー DTO の一覧です。 </param>
        public void ApplyMemberList(IReadOnlyList<MemberViewDTO> members)
        {
            if (_memberScrollView == null)
            {
                return;
            }

            // UXML のプレースホルダや前回の反映結果が残らないよう、毎回作り直す。
            _memberScrollView.Clear();

            if (members == null)
            {
                return;
            }

            for (int i = 0; i < members.Count; i++)
            {
                _memberScrollView.Add(CreateMemberElement(members[i]));
            }
        }

        /// <inheritdoc />
        protected override VisualElement CancelTargetElement => _backButton;

        private const string BACK_BUTTON_NAME = "BackButton";
        private const string BACK_GROUND_NAME = "BackGround";
        private const string MENBER_SCROLL_VIEW_NAME = "MemberScrollView";

        private const string MEMBER_CONTAINER_CLASS = "member-container";
        private const string MEMBER_ROLE_LABEL_CLASS = "member-role-label";
        private const string MEMBER_NAME_LABEL_CLASS = "member-name-label";
        private const string MEMBER_AFFILIATION_LABEL_CLASS = "member-affiliation-label";

        private Button _backButton;
        private VisualElement _backGround;
        private ScrollView _memberScrollView;

        /// <summary>
        ///     クレジット画面の UI 要素を初期化します。
        /// </summary>
        /// <param name="rootElement"> クレジット画面のルート要素です。 </param>
        /// <exception cref="NullReferenceException"> 必要な UI 要素が見つからない場合に発生します。 </exception>
        private void Initialize(VisualElement rootElement)
        {
            if (rootElement == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(CreditScreenView)}: Root VisualElementがnullです。");
#endif
                return;
            }

            _backButton = rootElement.Q<Button>(BACK_BUTTON_NAME)
                ?? throw new NullReferenceException($"{nameof(CreditScreenView)}: {BACK_BUTTON_NAME}が見つかりません。");
            _backGround = rootElement.Q<VisualElement>(BACK_GROUND_NAME)
                ?? throw new NullReferenceException($"{nameof(CreditScreenView)}: {BACK_GROUND_NAME}が見つかりません。");
            _memberScrollView = rootElement.Q<ScrollView>(MENBER_SCROLL_VIEW_NAME)
                ?? throw new NullReferenceException($"{nameof(CreditScreenView)}: {MENBER_SCROLL_VIEW_NAME}が見つかりません。");
        }

        /// <summary>
        ///     各ボタンのコールバックを登録します。
        /// </summary>
        private void RegisterButtonCallbacks()
        {
            _backButton.RegisterCallback<ClickEvent>(OnBackButtonClicked);

            // キャンセル操作で戻れるため、フォーカス移動の対象からは外す。
            _backButton.ExcludeFromNavigation();
            _backGround.RegisterCallback<PointerDownEvent>(OnPointDownEvent);
        }

        /// <summary>
        ///     各ボタンのコールバックを登録解除します。
        /// </summary>
        private void UnregisterButtonCallbacks()
        {
            _backButton.UnregisterCallback<ClickEvent>(OnBackButtonClicked);
            _backGround.UnregisterCallback<PointerDownEvent>(OnPointDownEvent);
        }


        /// <summary>
        ///     戻るボタンが押されたときの処理。
        /// </summary>
        /// <param name="clickEvent"> クリックイベント。 </param>
        private void OnBackButtonClicked(ClickEvent clickEvent)
        {
            OutGameUIEvent.OnScreenClosed?.Invoke();
        }

        /// <summary>
        ///     バックグラウンドが押されたときの処理。
        /// </summary>
        /// <param name="evt"></param>
        private void OnPointDownEvent(PointerDownEvent evt)
        {
            // バックグラウンドの子要素が押された場合は処理を行わない
            if (evt.target != evt.currentTarget) { return; }

            OutGameUIEvent.OnScreenClosed?.Invoke();
        }

        /// <summary>
        ///     制作メンバー 1 人分の表示要素を生成します。
        /// </summary>
        /// <param name="member"> 表示する制作メンバー DTO です。 </param>
        /// <returns> 生成した表示要素です。 </returns>
        private static VisualElement CreateMemberElement(in MemberViewDTO member)
        {
            var memberContainer = new VisualElement();
            memberContainer.AddToClassList(MEMBER_CONTAINER_CLASS);

            var roleLabel = new Label(member.ClassName);
            roleLabel.AddToClassList(MEMBER_ROLE_LABEL_CLASS);

            var nameLabel = new Label(member.Name);
            nameLabel.AddToClassList(MEMBER_NAME_LABEL_CLASS);

            var affiliationLabel = new Label(member.AffiliationName);
            affiliationLabel.AddToClassList(MEMBER_AFFILIATION_LABEL_CLASS);

            memberContainer.Add(roleLabel);
            memberContainer.Add(nameLabel);
            memberContainer.Add(affiliationLabel);
            return memberContainer;
        }
    }
}
