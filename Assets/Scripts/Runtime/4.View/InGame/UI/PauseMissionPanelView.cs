using R3;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.UI
{
    /// <summary>
    ///     ポーズ画面に、メインミッションとサブミッションの達成状況を並べるView。
    /// </summary>
    [AddComponentMenu("KillChord/View/UI/PauseMissionPanelView")]
    public sealed class PauseMissionPanelView : MonoBehaviour
    {
        /// <summary>
        ///     ViewModelを購読し、見出しとミッションの一覧を表示へ反映する。
        /// </summary>
        /// <param name="viewModel"> ポーズ画面のViewModel。 </param>
        public void Bind(PauseWindowViewModel viewModel)
        {
            Unbind();
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _subscriptions = new CompositeDisposable();
            SubscribeLabel(_viewModel.MissionLabel, _headerText);
            SubscribeLabel(_viewModel.MainMissionLabel, _mainMissionHeaderText);
            SubscribeLabel(_viewModel.SubMissionLabel, _subMissionHeaderText);
            _viewModel.OnMissionsUpdated += HandleMissionsUpdated;
            HandleMissionsUpdated();
        }

        /// <summary>
        ///     ViewModelの購読を解除する。
        /// </summary>
        public void Unbind()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
            if (_viewModel != null)
            {
                _viewModel.OnMissionsUpdated -= HandleMissionsUpdated;
                _viewModel = null;
            }
        }

        [SerializeField, Tooltip("ミッション欄の見出しのテキストです。")]
        private TMP_Text _headerText;

        [SerializeField, Tooltip("メインミッションの見出しのテキストです。")]
        private TMP_Text _mainMissionHeaderText;

        [SerializeField, Tooltip("メインミッション1件分の行です。")]
        private PauseMissionRowView _mainMissionRow;

        [SerializeField, Tooltip("サブミッションの見出しのテキストです。")]
        private TMP_Text _subMissionHeaderText;

        [SerializeField, Tooltip("サブミッションの行を並べる親です。")]
        private Transform _subMissionRoot;

        [SerializeField, Tooltip("サブミッションの行の複製元です。非表示のまま置いておきます。")]
        private PauseMissionRowView _subMissionRowTemplate;

        [SerializeField, Tooltip("サブミッションが無いときに出すテキストです。")]
        private TMP_Text _emptySubMissionText;

        private readonly List<PauseMissionRowView> _subMissionRows = new();
        private PauseWindowViewModel _viewModel;
        private CompositeDisposable _subscriptions;

        /// <summary> 破棄時に購読を解除します。 </summary>
        private void OnDestroy()
        {
            Unbind();
        }

        /// <summary>
        ///     ミッションの一覧を作り直す。足りない行だけ複製し、余った行は隠す。
        /// </summary>
        private void HandleMissionsUpdated()
        {
            if (_viewModel == null)
            {
                return;
            }

            gameObject.SetActive(_viewModel.HasMission);
            if (!_viewModel.HasMission)
            {
                return;
            }

            PauseMissionRowViewModel mainMission = _viewModel.MainMission;
            if (_mainMissionRow != null && mainMission != null)
            {
                _mainMissionRow.SetContent(mainMission.Description, mainMission.StatusText, mainMission.IsAchieved);
            }

            IReadOnlyList<PauseMissionRowViewModel> subMissions = _viewModel.SubMissions;
            for (int i = 0; i < subMissions.Count; i++)
            {
                PauseMissionRowView row = GetOrCreateRow(i);
                if (row == null)
                {
                    break;
                }

                row.gameObject.SetActive(true);
                row.SetContent(subMissions[i].Description, subMissions[i].StatusText, subMissions[i].IsAchieved);
            }

            for (int i = subMissions.Count; i < _subMissionRows.Count; i++)
            {
                _subMissionRows[i].gameObject.SetActive(false);
            }

            if (_emptySubMissionText != null)
            {
                _emptySubMissionText.text = _viewModel.EmptySubMissionText;
                _emptySubMissionText.gameObject.SetActive(subMissions.Count == 0);
            }
        }

        /// <summary>
        ///     指定した位置のサブミッション行を取得する。無ければ複製元から作る。
        /// </summary>
        /// <param name="index"> 行の位置。 </param>
        /// <returns> サブミッションの行。複製元が無い場合はnull。 </returns>
        private PauseMissionRowView GetOrCreateRow(int index)
        {
            if (index < _subMissionRows.Count)
            {
                return _subMissionRows[index];
            }

            if (_subMissionRowTemplate == null || _subMissionRoot == null)
            {
                Debug.LogWarning($"[{nameof(PauseMissionPanelView)}] サブミッション行の複製元が設定されていません。", this);
                return null;
            }

            PauseMissionRowView row = Instantiate(_subMissionRowTemplate, _subMissionRoot);
            _subMissionRows.Add(row);
            return row;
        }

        /// <summary>
        ///     文言の変化をテキストへ反映する。
        /// </summary>
        /// <param name="label"> 文言の通知元。 </param>
        /// <param name="text"> 反映先のテキスト。 </param>
        private void SubscribeLabel(ReadOnlyReactiveProperty<string> label, TMP_Text text)
        {
            if (text == null)
            {
                return;
            }

            label.Subscribe(value => text.text = value).AddTo(_subscriptions);
        }
    }
}
