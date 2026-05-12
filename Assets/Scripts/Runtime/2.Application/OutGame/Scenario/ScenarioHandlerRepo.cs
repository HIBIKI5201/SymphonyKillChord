using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Domain.OutGame.Scenario;

namespace KillChord.Runtime.Application.OutGame.Scenario
{
    /// <summary>
    /// シナリオイベントの型に応じて処理ハンドラを管理する。
    /// </summary>
    public class ScenarioHandlerRepo
    {
        public void Register<TEvent>(Func<TEvent, CancellationToken, ValueTask> handler)
            where TEvent : IScenarioEvent
        {
            _map[typeof(TEvent)] = (e, token) => handler((TEvent)e, token);
        }

        /// <summary>
        /// 受け取ったイベントを現在の出力先へ反映する。
        /// </summary>
        public ValueTask HandleAsync(IScenarioEvent e, CancellationToken ct)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));

            if (!_map.TryGetValue(e.GetType(), out var handler))
            {
                throw new InvalidOperationException($"Handler not found: {e.GetType().Name}");
            }

            return handler(e, ct);
        }

        private readonly Dictionary<Type, Func<IScenarioEvent, CancellationToken, ValueTask>> _map = new();
    }
}