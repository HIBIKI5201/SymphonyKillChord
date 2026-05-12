using R3;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Mission
{
    public class MissionHudView : MonoBehaviour
    {
        public void Initialize(MissionHudViewModel viewModel)
        {
            _viewModel = viewModel;

            _mainMissionDisposable = viewModel.MainMissionText.Subscribe(value =>
            {
                if (_mainMissionText != null)
                    _mainMissionText.text = value;
            });

            _resultDisposable = viewModel.ResultText.Subscribe(value =>
            {
                if (_resultText != null)
                    _resultText.text = value;
            });

            if (_viewModel != null)
                _viewModel.OnEvaluationItemsUpdated += ReBuildEvaluationItems;
        }

        [Header("メインミッション表示用UI")]
        [SerializeField] private TMP_Text _mainMissionText;
        [SerializeField] private TMP_Text _resultText;

        [Header("評価ミッション表示用UI")]
        [SerializeField] private RectTransform _evaluationRoot;
        [SerializeField] private MissionEvaluationItemView _evaluationItemPrefab;
        [SerializeField] private float _evaluationItemSpacing;

        private MissionHudViewModel _viewModel;
        private IDisposable _mainMissionDisposable;
        private IDisposable _resultDisposable;
        private readonly List<MissionEvaluationItemView> _spawnedEvaluationItems = new();

        private void OnDestroy()
        {
            _mainMissionDisposable?.Dispose();
            _resultDisposable?.Dispose();

            if (_viewModel != null)
                _viewModel.OnEvaluationItemsUpdated -= ReBuildEvaluationItems;
        }

        private void ReBuildEvaluationItems(IReadOnlyList<MissionEvaluationItemViewModel> items)
        {
            ClearEvaluationItems();

            if (_evaluationRoot == null || _evaluationItemPrefab == null)
            {
                return;
            }

            for (int i = 0; i < items.Count; i++)
            {
                MissionEvaluationItemView view = Instantiate(_evaluationItemPrefab, _evaluationRoot);

                RectTransform rectTransform = view.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = new Vector2(0, -i * _evaluationItemSpacing);
                }

                view.Apply(items[i]);
                _spawnedEvaluationItems.Add(view);
            }
        }

        private void ClearEvaluationItems()
        {
            foreach (var item in _spawnedEvaluationItems)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }

            _spawnedEvaluationItems.Clear();
        }
    }
}
