using KillChord.Runtime.Application.OutGame.Scenario;
using KillChord.Runtime.Domain.OutGame.Scenario;
using KillChord.Runtime.Utility.Identity;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.InfraStructure.OutGame.Scenario
{
    /// <summary>
    /// テスト用のシナリオデータをメモリ上で提供する。
    /// </summary>
    public class InMemoryScenarioRepository : IScenarioRepository
    {
        /// <summary>
        /// シナリオ ID から再生用データを読み込む。
        /// </summary>
        public ValueTask<ScenarioDefinition> FindByIdAsync(string id, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            if (!string.Equals(id, "test", StringComparison.Ordinal))
            {
                throw new KeyNotFoundException($"Scenario not found: {id}");
            }

            var backgroundRoom = new BackgroundEvent(new BackgroundId(
                DataIDHasher.Compute("ScenarioBackground", "bg_room")));
            var backgroundStreet = new BackgroundEvent(new BackgroundId(
                DataIDHasher.Compute("ScenarioBackground", "genki_pose")));
            var heroIdle = new AnimationEvent(new AnimationId(
                DataIDHasher.Compute("ScenarioAnimation", "anim_hero_idle")));

            IReadOnlyList<IScenarioEvent> events = new List<IScenarioEvent>
            {
                new TextEvent("misa", "Hello", CreateTriggers(
                    TextTimingTrigger.CreateAtCharIndex(0, _fadeIn),
                    TextTimingTrigger.CreateAtKeyword("danger", backgroundRoom))),
                new TextEvent("misa", "World danger", CreateTriggers(
                    TextTimingTrigger.CreateAtCharIndex(5, heroIdle),
                    TextTimingTrigger.CreateAtKeyword("danger", backgroundStreet))),
                new TextEvent("satoru", "Goodbye", CreateTriggers(
                    TextTimingTrigger.CreateAtCharIndex(1, _fadeOut))),
            };

            return new ValueTask<ScenarioDefinition>(new ScenarioDefinition(events));
        }

        /// <summary>
        /// テキストイベント用のトリガー一覧を生成する。
        /// </summary>
        private static IReadOnlyList<TextTimingTrigger> CreateTriggers(params TextTimingTrigger[] triggers)
        {
            if (triggers == null || triggers.Length == 0) return Array.Empty<TextTimingTrigger>();

            var result = new List<TextTimingTrigger>(triggers.Length);
            for (int i = 0; i < triggers.Length; i++)
            {
                if (triggers[i] == null) continue;
                result.Add(triggers[i]);
            }
            return result;
        }

        private readonly FadeEvent _fadeIn = new(0f, 1f, 1.0f);
        private readonly FadeEvent _fadeOut = new(1f, 0f, 1.0f);
    }
}
