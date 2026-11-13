using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.DebugWorld.Hub
{
    /// <summary>
    ///     裏世界のハブ画面を組み立てて、選んだテストを通知するViewクラスです。
    /// </summary>
    public sealed class DebugHubView : IDisposable
    {
        /// <summary>
        ///     ルート要素の下にハブ画面を組み立てます。
        /// </summary>
        /// <param name="rootElement"> ハブ画面を置くルート要素です。 </param>
        /// <param name="enemyNames"> 敵テストで選べる敵の表示名の一覧です。 </param>
        /// <param name="isBattleAvailable"> 戦闘のテストを始められる場合はtrueです。 </param>
        /// <param name="isBossTestAvailable"> ボステストを始められる場合はtrueです。 </param>
        public DebugHubView(
            VisualElement rootElement,
            IReadOnlyList<string> enemyNames,
            bool isBattleAvailable,
            bool isBossTestAvailable)
        {
            _rootElement = rootElement ?? throw new ArgumentNullException(nameof(rootElement));
            List<string> choices = enemyNames?.ToList() ?? new List<string>();

            _container = CreateContainer();
            _container.Add(CreateHeader());

            _playerControlTestButton = CreateButton(PLAYER_CONTROL_TEST_TEXT, HandlePlayerControlTestClicked);
            _playerControlTestButton.SetEnabled(isBattleAvailable);
            _container.Add(_playerControlTestButton);

            _enemyDropdown = new DropdownField(ENEMY_LABEL_TEXT, choices, choices.Count > 0 ? 0 : -1);
            _enemyAmountSlider = new SliderInt(ENEMY_AMOUNT_LABEL_TEXT, MIN_ENEMY_AMOUNT, MAX_ENEMY_AMOUNT)
            {
                value = DEFAULT_ENEMY_AMOUNT,
                showInputField = true,
            };
            _enemyTestButton = CreateButton(ENEMY_TEST_TEXT, HandleEnemyTestClicked);
            _container.Add(CreateEnemyTestSection(_enemyDropdown, _enemyAmountSlider, _enemyTestButton));
            bool isEnemyTestAvailable = isBattleAvailable && choices.Count > 0;
            _enemyDropdown.SetEnabled(isEnemyTestAvailable);
            _enemyAmountSlider.SetEnabled(isEnemyTestAvailable);
            _enemyTestButton.SetEnabled(isEnemyTestAvailable);

            _bossTestButton = CreateButton(BOSS_TEST_TEXT, HandleBossTestClicked);
            _bossTestButton.SetEnabled(isBattleAvailable && isBossTestAvailable);
            _container.Add(_bossTestButton);

            _returnButton = CreateButton(RETURN_TEXT, HandleReturnClicked);
            _container.Add(_returnButton);
            _rootElement.Add(_container);
            _returnButton.Focus();
        }

        /// <summary> 敵を出さない操作テストが選ばれたときに通知します。 </summary>
        public event Action OnPlayerControlTestRequested;

        /// <summary> 敵テストが選ばれたときに、敵の一覧の位置と数を通知します。 </summary>
        public event Action<int, int> OnEnemyTestRequested;

        /// <summary> ボステストが選ばれたときに通知します。 </summary>
        public event Action OnBossTestRequested;

        /// <summary> タイトルへ戻るボタンが押されたときに通知します。 </summary>
        public event Action OnReturnRequested;

        /// <summary>
        ///     ボタンを押せなくして、二重に遷移しないようにします。
        /// </summary>
        public void Lock()
        {
            _container.SetEnabled(false);
        }

        /// <summary>
        ///     ボタンを押せる状態に戻します。
        /// </summary>
        public void Unlock()
        {
            _container.SetEnabled(true);
        }

        /// <summary>
        ///     組み立てた画面を取り除きます。
        /// </summary>
        public void Dispose()
        {
            _playerControlTestButton.clicked -= HandlePlayerControlTestClicked;
            _enemyTestButton.clicked -= HandleEnemyTestClicked;
            _bossTestButton.clicked -= HandleBossTestClicked;
            _returnButton.clicked -= HandleReturnClicked;
            _container.RemoveFromHierarchy();
            OnPlayerControlTestRequested = null;
            OnEnemyTestRequested = null;
            OnBossTestRequested = null;
            OnReturnRequested = null;
        }

        private const string HEADER_TEXT = "裏世界";
        private const string PLAYER_CONTROL_TEST_TEXT = "プレイヤー操作テスト（敵なし）";
        private const string ENEMY_TEST_TEXT = "敵テストへ出撃";
        private const string ENEMY_LABEL_TEXT = "敵";
        private const string ENEMY_AMOUNT_LABEL_TEXT = "1回に出す数";
        private const string BOSS_TEST_TEXT = "ボステストへ出撃";
        private const string RETURN_TEXT = "タイトルへ戻る";
        private const int MIN_ENEMY_AMOUNT = 1;
        private const int MAX_ENEMY_AMOUNT = 20;
        private const int DEFAULT_ENEMY_AMOUNT = 3;
        private const int HEADER_FONT_SIZE = 56;
        private const int BUTTON_FONT_SIZE = 32;
        private const int FIELD_FONT_SIZE = 24;
        private const float BUTTON_WIDTH = 560f;
        private const float BUTTON_MARGIN = 12f;
        private const float BUTTON_PADDING = 16f;
        private const float SECTION_PADDING = 12f;
        private const float HEADER_MARGIN_BOTTOM = 32f;
        private static readonly Color BACKGROUND_COLOR = new(0.05f, 0.05f, 0.08f, 1f);
        private static readonly Color SECTION_COLOR = new(1f, 1f, 1f, 0.06f);

        private readonly VisualElement _rootElement;
        private readonly VisualElement _container;
        private readonly Button _playerControlTestButton;
        private readonly DropdownField _enemyDropdown;
        private readonly SliderInt _enemyAmountSlider;
        private readonly Button _enemyTestButton;
        private readonly Button _bossTestButton;
        private readonly Button _returnButton;

        /// <summary>
        ///     敵を出さない操作テストが選ばれたことを通知します。
        /// </summary>
        private void HandlePlayerControlTestClicked()
        {
            OnPlayerControlTestRequested?.Invoke();
        }

        /// <summary>
        ///     選んでいる敵と数で、敵テストが選ばれたことを通知します。
        /// </summary>
        private void HandleEnemyTestClicked()
        {
            if (_enemyDropdown.index < 0)
            {
                return;
            }

            OnEnemyTestRequested?.Invoke(_enemyDropdown.index, _enemyAmountSlider.value);
        }

        /// <summary>
        ///     ボステストが選ばれたことを通知します。
        /// </summary>
        private void HandleBossTestClicked()
        {
            OnBossTestRequested?.Invoke();
        }

        /// <summary>
        ///     タイトルへ戻るボタンが押されたことを通知します。
        /// </summary>
        private void HandleReturnClicked()
        {
            OnReturnRequested?.Invoke();
        }

        /// <summary>
        ///     画面全体を覆い、中身を中央に並べる要素を作ります。
        /// </summary>
        /// <returns> 作った要素です。 </returns>
        private static VisualElement CreateContainer()
        {
            VisualElement container = new();
            container.style.position = Position.Absolute;
            container.style.left = 0f;
            container.style.right = 0f;
            container.style.top = 0f;
            container.style.bottom = 0f;
            container.style.alignItems = Align.Center;
            container.style.justifyContent = Justify.Center;
            container.style.backgroundColor = BACKGROUND_COLOR;
            return container;
        }

        /// <summary>
        ///     画面の見出しを作ります。
        /// </summary>
        /// <returns> 作った見出しです。 </returns>
        private static Label CreateHeader()
        {
            Label header = new(HEADER_TEXT);
            header.style.fontSize = HEADER_FONT_SIZE;
            header.style.color = Color.white;
            header.style.marginBottom = HEADER_MARGIN_BOTTOM;
            return header;
        }

        /// <summary>
        ///     敵の選択・数・出撃ボタンを1つの枠にまとめます。
        /// </summary>
        /// <param name="enemyDropdown"> 敵の選択欄です。 </param>
        /// <param name="enemyAmountSlider"> 敵の数の入力欄です。 </param>
        /// <param name="enemyTestButton"> 出撃ボタンです。 </param>
        /// <returns> 作った枠です。 </returns>
        private static VisualElement CreateEnemyTestSection(
            DropdownField enemyDropdown,
            SliderInt enemyAmountSlider,
            Button enemyTestButton)
        {
            VisualElement section = new();
            section.style.width = BUTTON_WIDTH;
            section.style.marginTop = BUTTON_MARGIN;
            section.style.marginBottom = BUTTON_MARGIN;
            section.style.paddingLeft = SECTION_PADDING;
            section.style.paddingRight = SECTION_PADDING;
            section.style.paddingTop = SECTION_PADDING;
            section.style.paddingBottom = SECTION_PADDING;
            section.style.backgroundColor = SECTION_COLOR;

            enemyDropdown.style.fontSize = FIELD_FONT_SIZE;
            enemyAmountSlider.style.fontSize = FIELD_FONT_SIZE;
            enemyTestButton.style.width = StyleKeyword.Auto;
            section.Add(enemyDropdown);
            section.Add(enemyAmountSlider);
            section.Add(enemyTestButton);
            return section;
        }

        /// <summary>
        ///     ハブ画面で使う大きさのボタンを作ります。
        /// </summary>
        /// <param name="text"> ボタンの文言です。 </param>
        /// <param name="clicked"> 押されたときの処理です。nullの場合は何もしません。 </param>
        /// <returns> 作ったボタンです。 </returns>
        private static Button CreateButton(string text, Action clicked)
        {
            Button button = new(clicked) { text = text };
            button.style.width = BUTTON_WIDTH;
            button.style.marginTop = BUTTON_MARGIN;
            button.style.marginBottom = BUTTON_MARGIN;
            button.style.paddingTop = BUTTON_PADDING;
            button.style.paddingBottom = BUTTON_PADDING;
            button.style.fontSize = BUTTON_FONT_SIZE;
            return button;
        }
    }
}
