using System.Collections.Generic;
using KillChord.Runtime.Domain.OutGame.Scenario;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.OutGame.Scenario
{
    [CreateAssetMenu(
       fileName = "ScenarioSettingsAsset",
       menuName = "KillChord/Runtime/Scenario/Settings")]
    /// <summary>
    /// シナリオ再生に必要な設定値を保持するアセット。
    /// </summary>
    public class ScenarioSettingsAsset : ScriptableObject
    {
        /// <summary> NormalTextCharIntervalSec を取得する。 </summary>
        public float NormalTextCharIntervalSec => _normalTextCharIntervalSec;
        /// <summary> FastForwardTextCharIntervalSec を取得する。 </summary>
        public float FastForwardTextCharIntervalSec => _fastForwardTextCharIntervalSec;
        /// <summary> PausePollIntervalSec を取得する。 </summary>
        public float PausePollIntervalSec => _pausePollIntervalSec;
        /// <summary> CloseDelayAfterCompleteSec を取得する。 </summary>
        public float CloseDelayAfterCompleteSec => _closeDelayAfterCompleteSec;
        /// <summary> AutoAdvanceDelaySec を取得する。 </summary>
        public float AutoAdvanceDelaySec => _autoAdvanceDelaySec;
        /// <summary> SkipClosesImmediately を取得する。 </summary>
        public bool SkipClosesImmediately => _skipClosesImmediately;
        /// <summary> WaitForInputOnLastText を取得する。 </summary>
        public bool WaitForInputOnLastText => _waitForInputOnLastText;
        /// <summary> UI要素の背面→前面の重ね順を取得する。未設定時は既定順を返す。 </summary>
        public IReadOnlyList<ScenarioLayer> LayerBackToFront =>
            _layerBackToFront != null && _layerBackToFront.Count > 0
                ? _layerBackToFront
                : DEFAULT_LAYER_ORDER;

        private static readonly ScenarioLayer[] DEFAULT_LAYER_ORDER =
        {
            ScenarioLayer.Background,
            ScenarioLayer.Portrait,
            ScenarioLayer.Text,
            ScenarioLayer.Effect,
        };

        [Header("Timing")]
        [Tooltip("通常再生時の1文字あたりの表示間隔（秒）です。")]
        [SerializeField, Min(0f)]
        private float _normalTextCharIntervalSec = 0.2f;
        [Tooltip("早送り時の1文字あたりの表示間隔（秒）です。0に近いほど高速になります。")]
        [SerializeField, Min(0f)]
        private float _fastForwardTextCharIntervalSec = 0.02f;
        [Tooltip("停止中（Pause）に状態を再確認する間隔（秒）です。")]
        [SerializeField, Min(0f)]
        private float _pausePollIntervalSec = 0.05f;
        [Tooltip("シナリオ通常終了時にViewを閉じるまで待機する時間（秒）です。")]
        [SerializeField, Min(0f)]
        private float _closeDelayAfterCompleteSec = 3f;
        [Tooltip("自動進行時の待機時間（秒）です。0に近いほど高速になります。")]
        [SerializeField, Min(0f)]
        private float _autoAdvanceDelaySec = 2f;

        [Header("Flow")]
        [Tooltip("有効な場合、スキップ時は待機せずに即時でViewを閉じます。")]
        [SerializeField]
        private bool _skipClosesImmediately = true;
        [Tooltip("有効な場合、最後のテキストでも入力待ちを行います。無効な場合は最後の表示後に終了へ進みます。")]
        [SerializeField]
        private bool _waitForInputOnLastText = false;

        [Header("Layer")]
        [SerializeField, Tooltip("UI要素の重ね順。先頭が最背面・末尾が最前面。生成時にこの順へ並べ替えます。")]
        private List<ScenarioLayer> _layerBackToFront = new()
        {
            ScenarioLayer.Background,
            ScenarioLayer.Portrait,
            ScenarioLayer.Text,
            ScenarioLayer.Effect,
        };
    }
}