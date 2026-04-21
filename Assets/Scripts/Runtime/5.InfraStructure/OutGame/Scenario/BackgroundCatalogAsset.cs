using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure
{
    [CreateAssetMenu(
        fileName = "BackgroundCatalogAsset",
        menuName = "KillChord/Runtime/Scenario/Background Catalog")]
    public class BackgroundCatalogAsset : ScriptableObject
    {
        public IReadOnlyList<Entry> Entries => _entries;

        [SerializeField]
        private Entry[] _entries = Array.Empty<Entry>();

        [Serializable]
        public struct Entry
        {
            public string Id;
            public Sprite Asset;
        }
    }

    [CreateAssetMenu(
        fileName = "ScenarioSettingsAsset",
        menuName = "KillChord/Runtime/Scenario/Settings")]
    public class ScenarioSettingsAsset : ScriptableObject
    {
        public float NormalTextCharIntervalSec => _normalTextCharIntervalSec;
        public float FastForwardTextCharIntervalSec => _fastForwardTextCharIntervalSec;
        public float PausePollIntervalSec => _pausePollIntervalSec;
        public float CloseDelayAfterCompleteSec => _closeDelayAfterCompleteSec;
        public bool SkipClosesImmediately => _skipClosesImmediately;
        public bool WaitForInputOnLastText => _waitForInputOnLastText;
        public string DefaultScenarioId => _defaultScenarioId;

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

        [Header("Flow")]
        [Tooltip("有効な場合、スキップ時は待機せずに即時でViewを閉じます。")]
        [SerializeField]
        private bool _skipClosesImmediately = true;
        [Tooltip("有効な場合、最後のテキストでも入力待ちを行います。無効な場合は最後の表示後に終了へ進みます。")]
        [SerializeField]
        private bool _waitForInputOnLastText = false;
        [Tooltip("PlayScenario実行時に最初に読み込むシナリオIDです。")]
        [SerializeField]
        private string _defaultScenarioId = "test";
    }
}
