using R3;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Skill
{
    /// <summary>
    ///     装備スキル全体の入力進行View。
    /// </summary>
    public class SkillInputProgressView : MonoBehaviour
    {
        /// <summary>
        ///     ViewModelを購読する。
        /// </summary>
        public void Bind(SkillInputProgressViewModel viewModel)
        {
            if (_rowRoot == null)
            {
                throw new InvalidOperationException($"{nameof(SkillInputProgressView)}: {_rowRoot} is not assigned.");
            }
            if (_spawner == null)
            {
                throw new InvalidOperationException($"{nameof(SkillInputProgressView)}: {_spawner} is not assigned.");
            }

            _disposable?.Dispose();

            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _disposable = _viewModel.Rows.Subscribe(Apply);
        }

        [SerializeField, Tooltip("RowViewを並べる親Transform。")]
        private Transform _rowRoot;

        [SerializeField, Tooltip("スキル入力進行UIのSpawner。")]
        private SkillInputProgressSpawner _spawner;

        private SkillInputProgressViewModel _viewModel;
        private IDisposable _disposable;
        private readonly List<SkillInputProgressRowView> _rowViews = new();

        /// <summary>
        ///     ViewModelから送られてきたRowDataをもとに、RowViewを生成して表示する。
        /// </summary>
        /// <param name="rows"></param>
        private void Apply(IReadOnlyList<SkillInputProgressRowData> rows)
        {
            Clear();

            for (int i = 0; i < rows.Count; i++)
            {
                SkillInputProgressRowData rowData = rows[i];

                SkillInputProgressRowView rowView = _spawner.SpawnRowWithSteps(
                    _rowRoot,
                    rowData.Steps.Count);

                rowView.Apply(rowData);
                _rowViews.Add(rowView);
            }
        }

        /// <summary>
        ///     生成済みのRowViewをすべて破棄する。
        /// </summary>
        private void Clear()
        {
            foreach (SkillInputProgressRowView rowView in _rowViews)
            {
                if (rowView != null)
                {
                    Destroy(rowView.gameObject);
                }
            }

            _rowViews.Clear();
        }

        private void OnDestroy()
        {
            _disposable?.Dispose();
        }
    }
}
