using KillChord.Runtime.Adaptor.InGame.Reticle;
using KillChord.Runtime.Utility.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Reticle
{
    /// <summary>
    ///     登録された全敵（注目/候補を除く）のレティクルを Screen Space GUI 上に表示する View。
    ///     唯一の Update 駆動点として LateUpdate で Presenter を回し、
    ///     算出結果をプールしたマーカーへ差分適用する。
    ///     注目/候補の強調表示は既存の HUDEnemyHealthView が担当するため干渉しない。
    /// </summary>
    [DefaultExecutionOrder(ExecutionOrderConst.CAMERA_FOLLOW)]
    public sealed class ReticleHudView : MonoBehaviour
    {
        /// <summary>
        ///     Presenter を注入して駆動を開始する。
        /// </summary>
        /// <param name="presenter"> レティクル表示情報を算出する Presenter。 </param>
        public void Initialize(ReticleHudPresenter presenter)
        {
            _presenter = presenter ?? throw new ArgumentNullException(nameof(presenter));
        }

        private void Awake()
        {
            if (_markerPrefab == null)
            {
                Debug.LogError($"[{nameof(ReticleHudView)}] {nameof(_markerPrefab)} が未設定です。", this);
            }

            if (_markerRoot == null)
            {
                Debug.LogError($"[{nameof(ReticleHudView)}] {nameof(_markerRoot)} が未設定です。", this);
            }
        }

        private void LateUpdate()
        {
            if (_presenter == null || _markerPrefab == null || _markerRoot == null)
            {
                return;
            }

            _presenter.Tick(_buffer);
            ApplyMarkers();
        }

        private void OnDestroy()
        {
            _presenter = null;
        }

        /// <summary>
        ///     算出結果を TargetId をキーにプールへ差分適用する。
        ///     同一敵のマーカーを再利用することで、ちらつきと生成コストを避ける。
        /// </summary>
        private void ApplyMarkers()
        {
            _seenIds.Clear();

            for (int i = 0; i < _buffer.Count; i++)
            {
                ReticleMarker marker = _buffer[i];
                if (!_activeMarkers.TryGetValue(marker.TargetId, out ReticleMarkerView view))
                {
                    view = Rent();
                    _activeMarkers.Add(marker.TargetId, view);
                }

                view.SetPosition(marker.ScreenPosition);
                _seenIds.Add(marker.TargetId);
            }

            // 今フレーム出現しなかったマーカーをプールへ戻す。
            _removeIds.Clear();
            foreach (KeyValuePair<Guid, ReticleMarkerView> pair in _activeMarkers)
            {
                if (!_seenIds.Contains(pair.Key))
                {
                    _removeIds.Add(pair.Key);
                }
            }

            for (int i = 0; i < _removeIds.Count; i++)
            {
                Guid id = _removeIds[i];
                Return(_activeMarkers[id]);
                _activeMarkers.Remove(id);
            }
        }

        /// <summary>
        ///     プールからマーカーを取り出す。無ければ生成する。
        /// </summary>
        /// <returns> 表示状態のマーカー。 </returns>
        private ReticleMarkerView Rent()
        {
            ReticleMarkerView view = _pool.Count > 0
                ? _pool.Pop()
                : Instantiate(_markerPrefab, _markerRoot);
            view.Show();
            return view;
        }

        /// <summary>
        ///     マーカーを非表示にしてプールへ戻す。
        /// </summary>
        /// <param name="view"> 戻すマーカー。 </param>
        private void Return(ReticleMarkerView view)
        {
            view.Hide();
            _pool.Push(view);
        }

        [SerializeField, Tooltip("敵ごとに表示するレティクルマーカーの Prefab。")]
        private ReticleMarkerView _markerPrefab;

        [SerializeField, Tooltip("マーカーを並べる親 RectTransform。ロックオンHUDより下の階層に配置すること。")]
        private RectTransform _markerRoot;

        private ReticleHudPresenter _presenter;

        private readonly List<ReticleMarker> _buffer = new();
        private readonly Dictionary<Guid, ReticleMarkerView> _activeMarkers = new();
        private readonly Stack<ReticleMarkerView> _pool = new();
        private readonly HashSet<Guid> _seenIds = new();
        private readonly List<Guid> _removeIds = new();
    }
}
