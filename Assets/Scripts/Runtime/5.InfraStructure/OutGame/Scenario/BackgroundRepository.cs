using System;
using KillChord.Runtime.Application;
using KillChord.Runtime.Domain;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure
{
    public class BackgroundRepository : CatalogRepositoryBase<BackgroundDefinition, BackgroundCatalogAsset.Entry>, IBackgroundRepository
    {
        public BackgroundRepository(BackgroundCatalogAsset catalog)
            : base(catalog != null ? catalog.Entries : null)
        {
        }

        protected override bool TryBuild(BackgroundCatalogAsset.Entry entry, out string id, out BackgroundDefinition definition)
        {
            id = entry.Id;
            if (string.IsNullOrWhiteSpace(entry.Id) || entry.Asset == null)
            {
                definition = default;
                return false;
            }

            definition = new BackgroundDefinition(entry.Id, entry.Id);
            return true;
        }
    }

    public class ScenarioSettingsRepository : IScenarioSettingsRepository
    {
        public ScenarioSettingsRepository(ScenarioSettingsAsset asset)
        {
            _asset = asset;
        }

        public TimeSpan NormalTextCharInterval => ToTimeSpan(_asset != null ? _asset.NormalTextCharIntervalSec : 0.2f);
        public TimeSpan FastForwardTextCharInterval => ToTimeSpan(_asset != null ? _asset.FastForwardTextCharIntervalSec : 0.02f);
        public TimeSpan PausePollInterval => ToTimeSpan(_asset != null ? _asset.PausePollIntervalSec : 0.05f);
        public TimeSpan CloseDelayAfterComplete => ToTimeSpan(_asset != null ? _asset.CloseDelayAfterCompleteSec : 3f);
        public bool SkipClosesImmediately => _asset == null || _asset.SkipClosesImmediately;
        public bool WaitForInputOnLastText => _asset != null && _asset.WaitForInputOnLastText;
        public string DefaultScenarioId => _asset != null && !string.IsNullOrWhiteSpace(_asset.DefaultScenarioId)
            ? _asset.DefaultScenarioId
            : "test";

        private static TimeSpan ToTimeSpan(float seconds)
        {
            return TimeSpan.FromSeconds(Math.Max(0f, seconds));
        }

        private readonly ScenarioSettingsAsset _asset;
    }
}
