using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Application;
using KillChord.Runtime.Domain;
using UnityEngine.Networking;

namespace KillChord.Runtime.InfraStructure
{
    public class ScenarioRepository : IScenarioRepository
    {
        public async ValueTask<ScenarioData> FindByIdAsync(string id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("scenario id is empty.", nameof(id));
            }

            string root = UnityEngine.Application.streamingAssetsPath;
            bool isUrlPath = root.Contains("://", StringComparison.Ordinal);
            string path = isUrlPath
                ? $"{root.TrimEnd('/')}/Scenario/{id}.csv"
                : Path.Combine(root, "Scenario", $"{id}.csv");

            string[] lines = await ReadAllLinesAsync(path, isUrlPath, ct);
            if (lines.Length <= 1)
            {
                return new ScenarioData(Array.Empty<IScenarioEvent>());
            }

            List<string> headers = ParseCsvLine(lines[0]);
            var headerIndex = BuildHeaderIndex(headers);
            var eventRows = new List<EventRow>(Math.Max(4, lines.Length - 1));
            var triggerRows = new List<TriggerRow>(Math.Max(2, lines.Length / 2));
            int autoStep = 1;

            for (int lineNo = 2; lineNo <= lines.Length; lineNo++)
            {
                string raw = lines[lineNo - 1];
                if (string.IsNullOrWhiteSpace(raw)) continue;
                if (raw.TrimStart().StartsWith("#", StringComparison.Ordinal)) continue;

                List<string> values = ParseCsvLine(raw);
                string type = GetValue(values, headerIndex, "Type")?.Trim();
                if (string.IsNullOrWhiteSpace(type))
                {
                    continue;
                }

                if (type.Equals("Trigger", StringComparison.OrdinalIgnoreCase))
                {
                    int parentStep = ParseRequiredInt(GetValue(values, headerIndex, "ParentStep"), "ParentStep", lineNo);
                    triggerRows.Add(new TriggerRow(lineNo, parentStep, values));
                    continue;
                }

                int step = ParseOptionalInt(
                    GetValue(values, headerIndex, "Step"),
                    autoStep,
                    "Step",
                    lineNo);
                autoStep = Math.Max(autoStep + 1, step + 1);
                eventRows.Add(new EventRow(lineNo, step, type, values));
            }

            var definitions = new Dictionary<int, EventDefinition>();
            var orderedSteps = new List<int>(eventRows.Count);

            foreach (EventRow row in eventRows)
            {
                if (definitions.ContainsKey(row.Step))
                {
                    throw new FormatException($"line {row.LineNo}: duplicated Step '{row.Step}'.");
                }

                EventDefinition definition = CreateEventDefinition(row, headerIndex);
                definitions.Add(row.Step, definition);
                orderedSteps.Add(row.Step);
            }

            foreach (TriggerRow row in triggerRows)
            {
                if (!definitions.TryGetValue(row.ParentStep, out EventDefinition parent))
                {
                    throw new FormatException($"line {row.LineNo}: ParentStep '{row.ParentStep}' was not found.");
                }
                if (parent is not TextEventDefinition textParent)
                {
                    throw new FormatException($"line {row.LineNo}: ParentStep '{row.ParentStep}' must point Text event.");
                }

                TextTimingTrigger trigger = CreateTrigger(row.Values, headerIndex, row.LineNo, textParent.Text);
                textParent.AddTrigger(trigger);
            }

            var events = new List<IScenarioEvent>(orderedSteps.Count);
            foreach (int step in orderedSteps)
            {
                events.Add(definitions[step].ToEvent());
            }

            return new ScenarioData(events);
        }

        private static async ValueTask<string[]> ReadAllLinesAsync(string path, bool isUrlPath, CancellationToken ct)
        {
            if (!isUrlPath)
            {
                if (!File.Exists(path))
                {
                    throw new FileNotFoundException($"Scenario CSV not found. path={path}", path);
                }
                return await File.ReadAllLinesAsync(path, Encoding.UTF8, ct);
            }

            using var request = UnityWebRequest.Get(path);
            using var ctr = ct.Register(static s => ((UnityWebRequest)s).Abort(), request);
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            while (!operation.isDone)
            {
                ct.ThrowIfCancellationRequested();
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new IOException($"Scenario CSV request failed. path={path}, error={request.error}");
            }

            string text = request.downloadHandler != null ? request.downloadHandler.text : string.Empty;
            string normalized = text.Replace("\r\n", "\n").Replace('\r', '\n');
            return normalized.Split('\n');
        }

        private static Dictionary<string, int> BuildHeaderIndex(IReadOnlyList<string> headers)
        {
            var index = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < headers.Count; i++)
            {
                string key = headers[i]?.Trim();
                if (string.IsNullOrEmpty(key)) continue;
                index[key] = i;
            }

            return index;
        }

        private static EventDefinition CreateEventDefinition(
            EventRow row,
            IReadOnlyDictionary<string, int> headerIndex)
        {
            IReadOnlyList<string> values = row.Values;
            switch (row.Type.Trim().ToLowerInvariant())
            {
                case "text":
                    {
                        string speaker = GetValue(values, headerIndex, "Speaker");
                        string text = GetValue(values, headerIndex, "Text");
                        var def = new TextEventDefinition(row.Step, speaker ?? string.Empty, text ?? string.Empty);

                        // 互換: Event行に直接書かれた単一トリガーも受け入れる
                        TextTimingTrigger inlineTrigger = TryCreateTrigger(values, headerIndex, row.LineNo, text);
                        if (inlineTrigger != null)
                        {
                            def.AddTrigger(inlineTrigger);
                        }

                        return def;
                    }
                case "background":
                    {
                        string backgroundId = GetValue(values, headerIndex, "BackgroundId");
                        if (string.IsNullOrWhiteSpace(backgroundId))
                        {
                            throw new FormatException($"line {row.LineNo}: BackgroundId is required for Background event.");
                        }
                        return new PlainEventDefinition(row.Step, new BackgroundEvent(backgroundId));
                    }
                case "animation":
                    {
                        string animationId = GetValue(values, headerIndex, "AnimationId");
                        if (string.IsNullOrWhiteSpace(animationId))
                        {
                            throw new FormatException($"line {row.LineNo}: AnimationId is required for Animation event.");
                        }
                        return new PlainEventDefinition(row.Step, new KillChord.Runtime.Domain.AnimationEvent(animationId));
                    }
                case "fade":
                    {
                        float start = ParseRequiredFloat(GetValue(values, headerIndex, "FadeStart"), "FadeStart", row.LineNo);
                        float end = ParseRequiredFloat(GetValue(values, headerIndex, "FadeEnd"), "FadeEnd", row.LineNo);
                        float duration = ParseRequiredFloat(GetValue(values, headerIndex, "FadeDuration"), "FadeDuration", row.LineNo);
                        return new PlainEventDefinition(row.Step, new FadeEvent(start, end, duration));
                    }
                default:
                    throw new FormatException($"line {row.LineNo}: unknown Type '{row.Type}'.");
            }
        }

        private static TextTimingTrigger TryCreateTrigger(
            IReadOnlyList<string> values,
            IReadOnlyDictionary<string, int> headerIndex,
            int lineNo,
            string text)
        {
            string triggerTypeRaw = GetValue(values, headerIndex, "TriggerType");
            string triggerType = triggerTypeRaw?.Trim();
            if (string.IsNullOrWhiteSpace(triggerType) ||
                triggerType.Equals("None", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return CreateTrigger(values, headerIndex, lineNo, text);
        }

        private static TextTimingTrigger CreateTrigger(
            IReadOnlyList<string> values,
            IReadOnlyDictionary<string, int> headerIndex,
            int lineNo,
            string text)
        {
            string onTriggerTypeRaw = GetValue(values, headerIndex, "OnTriggerType");
            string onTriggerType = onTriggerTypeRaw?.Trim();
            if (string.IsNullOrWhiteSpace(onTriggerType))
            {
                throw new FormatException($"line {lineNo}: OnTriggerType is required when TriggerType is set.");
            }

            IScenarioEvent fireEvent = CreateTriggerEvent(values, headerIndex, lineNo, onTriggerType);
            string triggerTypeRaw = GetValue(values, headerIndex, "TriggerType");
            string triggerType = triggerTypeRaw?.Trim();

            switch (triggerType.ToLowerInvariant())
            {
                case "atcharindex":
                    {
                        string indexRaw = GetValue(values, headerIndex, "TriggerIndex");
                        if (!int.TryParse(indexRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int charIndex))
                        {
                            throw new FormatException($"line {lineNo}: TriggerIndex must be int for AtCharIndex.");
                        }
                        return TextTimingTrigger.AtCharIndex(charIndex, fireEvent);
                    }
                case "atkeyword":
                    {
                        string keyword = GetValue(values, headerIndex, "TriggerKeyword");
                        if (string.IsNullOrWhiteSpace(keyword))
                        {
                            throw new FormatException($"line {lineNo}: TriggerKeyword is required for AtKeyword.");
                        }
                        return TextTimingTrigger.AtKeyword(keyword, fireEvent);
                    }
                case "atsuffix":
                    {
                        string suffix = GetValue(values, headerIndex, "TriggerKeyword");
                        if (string.IsNullOrWhiteSpace(suffix))
                        {
                            throw new FormatException($"line {lineNo}: TriggerKeyword is required for AtSuffix.");
                        }
                        return TextTimingTrigger.AtSuffix(suffix, fireEvent);
                    }
                case "attextend":
                    {
                        int charIndex = string.IsNullOrEmpty(text) ? 0 : text.Length;
                        return TextTimingTrigger.AtCharIndex(charIndex, fireEvent);
                    }
                default:
                    throw new FormatException($"line {lineNo}: unknown TriggerType '{triggerTypeRaw}'.");
            }
        }

        private static IScenarioEvent CreateTriggerEvent(
            IReadOnlyList<string> values,
            IReadOnlyDictionary<string, int> headerIndex,
            int lineNo,
            string onTriggerType)
        {
            switch (onTriggerType.ToLowerInvariant())
            {
                case "fade":
                    {
                        float start = ParseRequiredFloat(GetValue(values, headerIndex, "OnTriggerArg1"), "OnTriggerArg1", lineNo);
                        float end = ParseRequiredFloat(GetValue(values, headerIndex, "OnTriggerArg2"), "OnTriggerArg2", lineNo);
                        float duration = ParseRequiredFloat(GetValue(values, headerIndex, "OnTriggerArg3"), "OnTriggerArg3", lineNo);
                        return new FadeEvent(start, end, duration);
                    }
                case "background":
                    {
                        string backgroundId = GetValue(values, headerIndex, "OnTriggerArg1");
                        if (string.IsNullOrWhiteSpace(backgroundId))
                        {
                            throw new FormatException($"line {lineNo}: OnTriggerArg1 is required for OnTriggerType=Background.");
                        }
                        return new BackgroundEvent(backgroundId);
                    }
                case "animation":
                    {
                        string animationId = GetValue(values, headerIndex, "OnTriggerArg1");
                        if (string.IsNullOrWhiteSpace(animationId))
                        {
                            throw new FormatException($"line {lineNo}: OnTriggerArg1 is required for OnTriggerType=Animation.");
                        }
                        return new KillChord.Runtime.Domain.AnimationEvent(animationId);
                    }
                default:
                    throw new FormatException($"line {lineNo}: unknown OnTriggerType '{onTriggerType}'.");
            }
        }

        private static string GetValue(
            IReadOnlyList<string> values,
            IReadOnlyDictionary<string, int> headerIndex,
            string key)
        {
            if (!headerIndex.TryGetValue(key, out int idx)) return string.Empty;
            if (idx < 0 || idx >= values.Count) return string.Empty;
            return values[idx];
        }

        private static float ParseRequiredFloat(string raw, string columnName, int lineNo)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                throw new FormatException($"line {lineNo}: {columnName} is required.");
            }

            if (!float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
            {
                throw new FormatException($"line {lineNo}: {columnName} must be number.");
            }

            return value;
        }

        private static int ParseRequiredInt(string raw, string columnName, int lineNo)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                throw new FormatException($"line {lineNo}: {columnName} is required.");
            }

            if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
            {
                throw new FormatException($"line {lineNo}: {columnName} must be int.");
            }
            return value;
        }

        private static int ParseOptionalInt(string raw, int defaultValue, string columnName, int lineNo)
        {
            if (string.IsNullOrWhiteSpace(raw)) return defaultValue;

            if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
            {
                throw new FormatException($"line {lineNo}: {columnName} must be int.");
            }

            return value;
        }

        private static List<string> ParseCsvLine(string line)
        {
            var fields = new List<string>();
            if (line == null)
            {
                fields.Add(string.Empty);
                return fields;
            }

            var current = new StringBuilder(line.Length);
            bool inQuote = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (inQuote)
                {
                    if (c == '"')
                    {
                        bool escapedQuote = i + 1 < line.Length && line[i + 1] == '"';
                        if (escapedQuote)
                        {
                            current.Append('"');
                            i++;
                        }
                        else
                        {
                            inQuote = false;
                        }
                    }
                    else
                    {
                        current.Append(c);
                    }
                }
                else
                {
                    if (c == ',')
                    {
                        fields.Add(current.ToString());
                        current.Clear();
                    }
                    else if (c == '"')
                    {
                        inQuote = true;
                    }
                    else
                    {
                        current.Append(c);
                    }
                }
            }

            fields.Add(current.ToString());
            return fields;
        }

        private readonly struct EventRow
        {
            public EventRow(int lineNo, int step, string type, IReadOnlyList<string> values)
            {
                LineNo = lineNo;
                Step = step;
                Type = type;
                Values = values;
            }

            public int LineNo { get; }
            public int Step { get; }
            public string Type { get; }
            public IReadOnlyList<string> Values { get; }
        }

        private readonly struct TriggerRow
        {
            public TriggerRow(int lineNo, int parentStep, IReadOnlyList<string> values)
            {
                LineNo = lineNo;
                ParentStep = parentStep;
                Values = values;
            }

            public int LineNo { get; }
            public int ParentStep { get; }
            public IReadOnlyList<string> Values { get; }
        }

        private abstract class EventDefinition
        {
            protected EventDefinition(int step) => Step = step;
            public int Step { get; }
            public abstract IScenarioEvent ToEvent();
        }

        private sealed class PlainEventDefinition : EventDefinition
        {
            public PlainEventDefinition(int step, IScenarioEvent scenarioEvent) : base(step)
            {
                _scenarioEvent = scenarioEvent;
            }

            public override IScenarioEvent ToEvent() => _scenarioEvent;
            private readonly IScenarioEvent _scenarioEvent;
        }

        private sealed class TextEventDefinition : EventDefinition
        {
            public TextEventDefinition(int step, string speaker, string text) : base(step)
            {
                Speaker = speaker;
                Text = text;
            }

            public string Speaker { get; }
            public string Text { get; }

            public void AddTrigger(TextTimingTrigger trigger) => _triggers.Add(trigger);

            public override IScenarioEvent ToEvent()
            {
                _cached ??= new TextEvent(Speaker, Text, _triggers.ToArray());
                return _cached;
            }

            private readonly List<TextTimingTrigger> _triggers = new();
            private IScenarioEvent _cached;
        }
    }

}
