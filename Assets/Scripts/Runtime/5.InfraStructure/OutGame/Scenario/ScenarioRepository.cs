using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using KillChord.Runtime.Application;
using KillChord.Runtime.Domain;

namespace KillChord.Runtime.InfraStructure
{
    public class ScenarioRepository : IScenarioRepository
    {
        public ScenarioData FindById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("scenario id is empty.", nameof(id));
            }

            string path = Path.Combine(UnityEngine.Application.streamingAssetsPath, "Scenario", $"{id}.csv");
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Scenario CSV not found. id={id}, path={path}", path);
            }

            string[] lines = File.ReadAllLines(path, Encoding.UTF8);
            if (lines.Length <= 1)
            {
                return new ScenarioData(Array.Empty<IScenarioEvent>());
            }

            List<string> headers = ParseCsvLine(lines[0]);
            var headerIndex = BuildHeaderIndex(headers);
            var events = new List<IScenarioEvent>(Math.Max(4, lines.Length - 1));

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

                IScenarioEvent scenarioEvent = CreateEvent(type, values, headerIndex, lineNo);
                events.Add(scenarioEvent);
            }

            return new ScenarioData(events);
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

        private static IScenarioEvent CreateEvent(
            string type,
            IReadOnlyList<string> values,
            IReadOnlyDictionary<string, int> headerIndex,
            int lineNo)
        {
            switch (type.Trim().ToLowerInvariant())
            {
                case "text":
                {
                    string speaker = GetValue(values, headerIndex, "Speaker");
                    string text = GetValue(values, headerIndex, "Text");
                    TextTimingTrigger trigger = CreateTextTrigger(values, headerIndex, lineNo, text);
                    IReadOnlyList<TextTimingTrigger> triggers =
                        trigger == null ? Array.Empty<TextTimingTrigger>() : new[] { trigger };
                    return new TextEvent(speaker ?? string.Empty, text ?? string.Empty, triggers);
                }
                case "background":
                {
                    string backgroundId = GetValue(values, headerIndex, "BackgroundId");
                    if (string.IsNullOrWhiteSpace(backgroundId))
                    {
                        throw new FormatException($"line {lineNo}: BackgroundId is required for Background event.");
                    }
                    return new BackgroundEvent(backgroundId);
                }
                case "animation":
                {
                    string animationId = GetValue(values, headerIndex, "AnimationId");
                    if (string.IsNullOrWhiteSpace(animationId))
                    {
                        throw new FormatException($"line {lineNo}: AnimationId is required for Animation event.");
                    }
                    return new KillChord.Runtime.Domain.AnimationEvent(animationId);
                }
                case "fade":
                {
                    float start = ParseRequiredFloat(GetValue(values, headerIndex, "FadeStart"), "FadeStart", lineNo);
                    float end = ParseRequiredFloat(GetValue(values, headerIndex, "FadeEnd"), "FadeEnd", lineNo);
                    float duration = ParseRequiredFloat(GetValue(values, headerIndex, "FadeDuration"), "FadeDuration", lineNo);
                    return new FadeEvent(start, end, duration);
                }
                default:
                    throw new FormatException($"line {lineNo}: unknown Type '{type}'.");
            }
        }

        private static TextTimingTrigger CreateTextTrigger(
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

            string onTriggerTypeRaw = GetValue(values, headerIndex, "OnTriggerType");
            string onTriggerType = onTriggerTypeRaw?.Trim();
            if (string.IsNullOrWhiteSpace(onTriggerType))
            {
                throw new FormatException($"line {lineNo}: OnTriggerType is required when TriggerType is set.");
            }

            IScenarioEvent fireEvent = CreateTriggerEvent(values, headerIndex, lineNo, onTriggerType);

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
    }

    public class InMemoryScenarioRepository : IScenarioRepository
    {
        public ScenarioData FindById(string id)
        {
            var backgroundRoom = new BackgroundEvent("bg_room");
            var backgroundStreet = new BackgroundEvent("genki_pose");
            var heroIdle = new KillChord.Runtime.Domain.AnimationEvent("anim_hero_idle");

            IReadOnlyList<IScenarioEvent> events = new List<IScenarioEvent>
            {
                new TextEvent("misa", "Hello", CreateTriggers(
                    TextTimingTrigger.AtCharIndex(0, _fadeIn),
                    TextTimingTrigger.AtKeyword("danger", backgroundRoom))),
                new TextEvent("misa", "World danger", CreateTriggers(
                    TextTimingTrigger.AtCharIndex(5, heroIdle),
                    TextTimingTrigger.AtKeyword("danger", backgroundStreet))),
                new TextEvent("satoru", "Goodbye", CreateTriggers(
                    TextTimingTrigger.AtCharIndex(1, _fadeOut))),
            };

            return new ScenarioData(events);
        }

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
