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
        [SerializeField, Min(0f)]
        private float _normalTextCharIntervalSec = 0.2f;
        [SerializeField, Min(0f)]
        private float _fastForwardTextCharIntervalSec = 0.02f;
        [SerializeField, Min(0f)]
        private float _pausePollIntervalSec = 0.05f;
        [SerializeField, Min(0f)]
        private float _closeDelayAfterCompleteSec = 3f;

        [Header("Flow")]
        [SerializeField]
        private bool _skipClosesImmediately = true;
        [SerializeField]
        private bool _waitForInputOnLastText = false;
        [SerializeField]
        private string _defaultScenarioId = "test";
    }
}
