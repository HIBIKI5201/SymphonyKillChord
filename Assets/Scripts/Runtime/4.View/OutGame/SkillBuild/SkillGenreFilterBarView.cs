using KillChord.Runtime.View.OutGame.Navigation;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.SkillBuild
{
    /// <summary>
    ///     ジャンル絞り込み専用のボタン(タブ)一覧を管理する View。
    /// </summary>
    public sealed class SkillGenreFilterBarView : IDisposable
    {
        /// <summary>
        ///     SkillGenreFilterBarView を初期化する。
        /// </summary>
        /// <param name="container"> ボタン表示先。 </param>
        /// <exception cref="ArgumentNullException"></exception>
        public SkillGenreFilterBarView(VisualElement container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _allButton = CreateButton(ALL_BUTTON_CLASS_NAME);
            _allButton.text = "全て";
            _allButton.clicked += HandleAllButtonClickedHandler;
            _container.Add(_allButton);
        }

        /// <summary> ジャンルフィルタが選択された時に通知する。全て選択時は null。 </summary>
        public event Action<int?> OnGenreFilterSelected;

        /// <summary>
        ///     表示するジャンルボタン一覧を更新する。内容が前回と同じ場合は再構築しない。
        /// </summary>
        /// <param name="genres"> ジャンル ID とアイコンの一覧。 </param>
        public void SetAvailableGenres(IReadOnlyList<(int GenreId, Sprite Icon)> genres)
        {
            genres ??= Array.Empty<(int GenreId, Sprite Icon)>();

            if (IsSameGenres(genres))
            {
                return;
            }

            ClearGenreButtons();

            for (int i = 0; i < genres.Count; i++)
            {
                (int genreId, Sprite icon) = genres[i];
                Button button = CreateButton(GENRE_BUTTON_CLASS_NAME);
                Image iconImage = new() { sprite = icon };
                iconImage.AddToClassList(GENRE_BUTTON_ICON_CLASS_NAME);
                button.Add(iconImage);
                button.clicked += () => HandleGenreButtonClickedHandler(genreId);
                _container.Add(button);
                _genreButtons.Add((genreId, button));
            }

            _currentGenreIds = ExtractGenreIds(genres);
        }

        /// <summary>
        ///     選択中ハイライトのみを更新する(ボタン再構築は行わない)。
        /// </summary>
        /// <param name="genreId"> 選択中のジャンル ID。全て選択時は null。 </param>
        public void SetActiveGenre(int? genreId)
        {
            _allButton.EnableInClassList(ACTIVE_CLASS_NAME, !genreId.HasValue);

            for (int i = 0; i < _genreButtons.Count; i++)
            {
                (int buttonGenreId, Button button) = _genreButtons[i];
                button.EnableInClassList(ACTIVE_CLASS_NAME, genreId.HasValue && buttonGenreId == genreId.Value);
            }
        }

        /// <summary>
        ///     イベント購読を解除する。
        /// </summary>
        public void Dispose()
        {
            _allButton.clicked -= HandleAllButtonClickedHandler;
            ClearGenreButtons();
            OnGenreFilterSelected = null;
        }

        private const string ALL_BUTTON_CLASS_NAME = "skill-genre-filter-button-all";
        private const string GENRE_BUTTON_CLASS_NAME = "skill-genre-filter-button";
        private const string GENRE_BUTTON_ICON_CLASS_NAME = "skill-genre-filter-button-icon";
        private const string ACTIVE_CLASS_NAME = "is-active";

        private readonly VisualElement _container;
        private readonly Button _allButton;
        private readonly List<(int GenreId, Button Button)> _genreButtons = new();
        private int[] _currentGenreIds = Array.Empty<int>();

        /// <summary>
        ///     ジャンルボタンを生成する共通処理。
        /// </summary>
        /// <param name="className"> 付与するクラス名。 </param>
        /// <returns> 生成したボタン。 </returns>
        private static Button CreateButton(string className)
        {
            Button button = new();
            button.AddToClassList(className);
            button.MakeNavigable();
            return button;
        }

        /// <summary>
        ///     ジャンルボタンをすべて破棄する。
        /// </summary>
        private void ClearGenreButtons()
        {
            for (int i = 0; i < _genreButtons.Count; i++)
            {
                _genreButtons[i].Button.RemoveFromHierarchy();
            }

            _genreButtons.Clear();
        }

        /// <summary>
        ///     渡されたジャンル一覧が現在の表示内容と同じか判定する。
        /// </summary>
        /// <param name="genres"> 比較対象。 </param>
        /// <returns> 同じ場合は true。 </returns>
        private bool IsSameGenres(IReadOnlyList<(int GenreId, Sprite Icon)> genres)
        {
            if (genres.Count != _currentGenreIds.Length)
            {
                return false;
            }

            for (int i = 0; i < genres.Count; i++)
            {
                if (genres[i].GenreId != _currentGenreIds[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     ジャンル ID 一覧を抽出する。
        /// </summary>
        /// <param name="genres"> 変換元。 </param>
        /// <returns> ジャンル ID 配列。 </returns>
        private static int[] ExtractGenreIds(IReadOnlyList<(int GenreId, Sprite Icon)> genres)
        {
            int[] result = new int[genres.Count];
            for (int i = 0; i < genres.Count; i++)
            {
                result[i] = genres[i].GenreId;
            }

            return result;
        }

        /// <summary>
        ///     「全て」ボタンのクリックを処理する。
        /// </summary>
        private void HandleAllButtonClickedHandler()
        {
            OnGenreFilterSelected?.Invoke(null);
        }

        /// <summary>
        ///     ジャンルボタンのクリックを処理する。
        /// </summary>
        /// <param name="genreId"> クリックされたジャンル ID。 </param>
        private void HandleGenreButtonClickedHandler(int genreId)
        {
            OnGenreFilterSelected?.Invoke(genreId);
        }
    }
}
