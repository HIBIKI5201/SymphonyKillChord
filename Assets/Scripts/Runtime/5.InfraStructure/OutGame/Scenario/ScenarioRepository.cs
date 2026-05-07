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
            string authoringPath = isUrlPath
                ? $"{root.TrimEnd('/')}/ScenarioAuthoring/{id}.events.csv"
                : Path.Combine(root, "ScenarioAuthoring", $"{id}.events.csv");
            string scenarioPath = isUrlPath
                ? $"{root.TrimEnd('/')}/Scenario/{id}.csv"
                : Path.Combine(root, "Scenario", $"{id}.csv");

            string[] lines = await ReadScenarioLinesAsync(authoringPath, scenarioPath, isUrlPath, ct);
            if (lines.Length == 0)
            {
                return new ScenarioData(Array.Empty<IScenarioEvent>());
            }

            string firstDataLine = FindFirstDataLine(lines);
            if (string.IsNullOrWhiteSpace(firstDataLine))
            {
                return new ScenarioData(Array.Empty<IScenarioEvent>());
            }

            if (firstDataLine.TrimStart().StartsWith("Type,", StringComparison.OrdinalIgnoreCase))
            {
                return ParseNormalizedCsv(lines);
            }

            return ParseAuthoringCsv(lines);
        }

        private static ScenarioData ParseNormalizedCsv(string[] lines)
        {
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

        private static ScenarioData ParseAuthoringCsv(string[] lines)
        {
            var definitions = new Dictionary<int, EventDefinition>();
            var orderedSteps = new List<int>(Math.Max(4, lines.Length));
            var pendingTriggers = new List<AuthoringTriggerRow>(Math.Max(2, lines.Length / 2));

            for (int lineNo = 1; lineNo <= lines.Length; lineNo++)
            {
                string raw = lines[lineNo - 1];
                if (string.IsNullOrWhiteSpace(raw)) continue;
                if (raw.TrimStart().StartsWith("#", StringComparison.Ordinal)) continue;

                List<string> fields = ParseCsvLine(raw);
                if (fields.Count < 2)
                {
                    throw new FormatException($"line {lineNo}: authoring csv requires at least Step and Type.");
                }

                int step = ParseRequiredInt(fields[0], "Step", lineNo);
                string type = fields[1]?.Trim();
                if (string.IsNullOrWhiteSpace(type))
                {
                    throw new FormatException($"line {lineNo}: Type is required.");
                }

                if (type.Equals("Trigger", StringComparison.OrdinalIgnoreCase))
                {
                    pendingTriggers.Add(new AuthoringTriggerRow(lineNo, fields));
                    continue;
                }

                if (definitions.ContainsKey(step))
                {
                    throw new FormatException($"line {lineNo}: duplicated Step '{step}'.");
                }

                EventDefinition definition = CreateAuthoringEventDefinition(step, type, fields, lineNo);
                definitions.Add(step, definition);
                orderedSteps.Add(step);
            }

            foreach (AuthoringTriggerRow triggerRow in pendingTriggers)
            {
                int parentStep = ParseRequiredInt(GetAuthoringField(triggerRow.Fields, 2), "ParentStep", triggerRow.LineNo);
                if (!definitions.TryGetValue(parentStep, out EventDefinition parent))
                {
                    throw new FormatException($"line {triggerRow.LineNo}: ParentStep '{parentStep}' was not found.");
                }
                if (parent is not TextEventDefinition textParent)
                {
                    throw new FormatException($"line {triggerRow.LineNo}: ParentStep '{parentStep}' must point Text event.");
                }

                TextTimingTrigger trigger = CreateAuthoringTrigger(triggerRow.Fields, triggerRow.LineNo, textParent.Text);
                textParent.AddTrigger(trigger);
            }

            var events = new List<IScenarioEvent>(orderedSteps.Count);
            foreach (int step in orderedSteps)
            {
                events.Add(definitions[step].ToEvent());
            }

            return new ScenarioData(events);
        }

        private static EventDefinition CreateAuthoringEventDefinition(int step, string type, IReadOnlyList<string> fields, int lineNo)
        {
            switch (type.Trim().ToLowerInvariant())
            {
                case "text":
                    {
                        string speaker = GetAuthoringField(fields, 2);
                        string text = GetAuthoringField(fields, 3);
                        return new TextEventDefinition(step, speaker ?? string.Empty, text ?? string.Empty);
                    }
                case "background":
                    {
                        string backgroundId = GetAuthoringField(fields, 2);
                        if (string.IsNullOrWhiteSpace(backgroundId))
                        {
                            throw new FormatException($"line {lineNo}: BackgroundId is required.");
                        }
                        return new PlainEventDefinition(step, new BackgroundEvent(backgroundId));
                    }
                case "animation":
                    {
                        string animationId = GetAuthoringField(fields, 2);
                        if (string.IsNullOrWhiteSpace(animationId))
                        {
                            throw new FormatException($"line {lineNo}: AnimationId is required.");
                        }
                        return new PlainEventDefinition(step, new AnimationEvent(animationId));
                    }
                case "fade":
                    {
                        float start = ParseRequiredFloat(GetAuthoringField(fields, 2), "FadeStart", lineNo);
                        float end = ParseRequiredFloat(GetAuthoringField(fields, 3), "FadeEnd", lineNo);
                        float duration = ParseRequiredFloat(GetAuthoringField(fields, 4), "FadeDuration", lineNo);
                        return new PlainEventDefinition(step, new FadeEvent(start, end, duration));
                    }
                case "portrait":
                    {
                        PortraitSlot slot = ParsePortraitSlot(GetAuthoringField(fields, 2), "PortraitSlot", lineNo);
                        string portraitId = GetAuthoringField(fields, 3);
                        if (string.IsNullOrWhiteSpace(portraitId))
                        {
                            throw new FormatException($"line {lineNo}: PortraitId is required.");
                        }

                        float posX = ParseOptionalFloat(GetAuthoringField(fields, 4), 0f, "PortraitPosX", lineNo);
                        float posY = ParseOptionalFloat(GetAuthoringField(fields, 5), 0f, "PortraitPosY", lineNo);
                        float scale = ParseOptionalFloat(GetAuthoringField(fields, 6), 1f, "PortraitScale", lineNo);
                        bool visible = ParseOptionalBool(GetAuthoringField(fields, 7), true, "PortraitVisible", lineNo);

                        return new PlainEventDefinition(step, new PortraitEvent(slot, portraitId, posX, posY, scale, visible));
                    }
                case "layer":
                    {
                        LayerTarget target = ParseLayerTarget(GetAuthoringField(fields, 2), "LayerTarget", lineNo);
                        int order = ParseRequiredInt(GetAuthoringField(fields, 3), "LayerOrder", lineNo);
                        return new PlainEventDefinition(step, new LayerEvent(target, order));
                    }
                default:
                    throw new FormatException($"line {lineNo}: unknown Type '{type}'.");
            }
        }

        private static TextTimingTrigger CreateAuthoringTrigger(IReadOnlyList<string> fields, int lineNo, string text)
        {
            string triggerTypeRaw = GetAuthoringField(fields, 3);
            string triggerType = triggerTypeRaw?.Trim();
            if (string.IsNullOrWhiteSpace(triggerType))
            {
                throw new FormatException($"line {lineNo}: TriggerType is required.");
            }

            string onTriggerTypeRaw = GetAuthoringField(fields, 6);
            string onTriggerType = onTriggerTypeRaw?.Trim();
            if (string.IsNullOrWhiteSpace(onTriggerType))
            {
                throw new FormatException($"line {lineNo}: OnTriggerType is required.");
            }

            IScenarioEvent fireEvent = CreateAuthoringTriggerEvent(fields, lineNo, onTriggerType);
            switch (triggerType.ToLowerInvariant())
            {
                case "atcharindex":
                    {
                        int charIndex = ParseRequiredInt(GetAuthoringField(fields, 4), "TriggerIndex", lineNo);
                        return TextTimingTrigger.AtCharIndex(charIndex, fireEvent);
                    }
                case "atkeyword":
                    {
                        string keyword = GetAuthoringField(fields, 5);
                        if (string.IsNullOrWhiteSpace(keyword))
                        {
                            throw new FormatException($"line {lineNo}: TriggerKeyword is required.");
                        }
                        return TextTimingTrigger.AtKeyword(keyword, fireEvent);
                    }
                case "atsuffix":
                    {
                        string suffix = GetAuthoringField(fields, 5);
                        if (string.IsNullOrWhiteSpace(suffix))
                        {
                            throw new FormatException($"line {lineNo}: TriggerKeyword is required.");
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

        private static IScenarioEvent CreateAuthoringTriggerEvent(IReadOnlyList<string> fields, int lineNo, string onTriggerType)
        {
            switch (onTriggerType.ToLowerInvariant())
            {
                case "fade":
                    {
                        float start = ParseRequiredFloat(GetAuthoringField(fields, 7), "OnTriggerArg1", lineNo);
                        float end = ParseRequiredFloat(GetAuthoringField(fields, 8), "OnTriggerArg2", lineNo);
                        float duration = ParseRequiredFloat(GetAuthoringField(fields, 9), "OnTriggerArg3", lineNo);
                        return new FadeEvent(start, end, duration);
                    }
                case "background":
                    {
                        string backgroundId = GetAuthoringField(fields, 7);
                        if (string.IsNullOrWhiteSpace(backgroundId))
                        {
                            throw new FormatException($"line {lineNo}: OnTriggerArg1 is required for Background.");
                        }
                        return new BackgroundEvent(backgroundId);
                    }
                case "animation":
                    {
                        string animationId = GetAuthoringField(fields, 7);
                        if (string.IsNullOrWhiteSpace(animationId))
                        {
                            throw new FormatException($"line {lineNo}: OnTriggerArg1 is required for Animation.");
                        }
                        return new AnimationEvent(animationId);
                    }
                case "portrait":
                    {
                        PortraitSlot slot = ParsePortraitSlot(GetAuthoringField(fields, 7), "OnTriggerArg1", lineNo);
                        string portraitId = GetAuthoringField(fields, 8);
                        if (string.IsNullOrWhiteSpace(portraitId))
                        {
                            throw new FormatException($"line {lineNo}: OnTriggerArg2 is required for Portrait.");
                        }
                        float posX = ParseOptionalFloat(GetAuthoringField(fields, 9), 0f, "OnTriggerArg3", lineNo);
                        return new PortraitEvent(slot, portraitId, posX, 0f, 1f, true);
                    }
                case "layer":
                    {
                        LayerTarget target = ParseLayerTarget(GetAuthoringField(fields, 7), "OnTriggerArg1", lineNo);
                        int order = ParseRequiredInt(GetAuthoringField(fields, 8), "OnTriggerArg2", lineNo);
                        return new LayerEvent(target, order);
                    }
                default:
                    throw new FormatException($"line {lineNo}: unknown OnTriggerType '{onTriggerType}'.");
            }
        }

        private static string GetAuthoringField(IReadOnlyList<string> fields, int index)
        {
            return ScenarioCsvUtility.GetField(fields, index);
        }

        private static async ValueTask<string[]> ReadScenarioLinesAsync(
            string authoringPath,
            string scenarioPath,
            bool isUrlPath,
            CancellationToken ct)
        {
            try
            {
                return await ReadAllLinesAsync(authoringPath, isUrlPath, ct);
            }
            catch (FileNotFoundException)
            {
                return await ReadAllLinesAsync(scenarioPath, isUrlPath, ct);
            }
            catch (IOException)
            {
                return await ReadAllLinesAsync(scenarioPath, isUrlPath, ct);
            }
        }

        private static string FindFirstDataLine(string[] lines)
        {
            if (lines == null) return string.Empty;
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.TrimStart().StartsWith("#", StringComparison.Ordinal)) continue;
                return line;
            }

            return string.Empty;
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
            return ScenarioCsvUtility.BuildHeaderIndex(headers);
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

                        // 莠呈鋤: Event陦後↓逶ｴ謗･譖ｸ縺九ｌ縺溷腰荳繝医Μ繧ｬ繝ｼ繧ょ女縺大・繧後ｋ
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
                case "portrait":
                    {
                        PortraitSlot slot = ParsePortraitSlot(
                            GetValue(values, headerIndex, "PortraitSlot"),
                            "PortraitSlot",
                            row.LineNo);
                        string portraitId = GetValue(values, headerIndex, "PortraitId");
                        if (string.IsNullOrWhiteSpace(portraitId))
                        {
                            throw new FormatException($"line {row.LineNo}: PortraitId is required for Portrait event.");
                        }
                        float posX = ParseOptionalFloat(GetValue(values, headerIndex, "PortraitPosX"), 0f, "PortraitPosX", row.LineNo);
                        float posY = ParseOptionalFloat(GetValue(values, headerIndex, "PortraitPosY"), 0f, "PortraitPosY", row.LineNo);
                        float scale = ParseOptionalFloat(GetValue(values, headerIndex, "PortraitScale"), 1f, "PortraitScale", row.LineNo);
                        bool visible = ParseOptionalBool(GetValue(values, headerIndex, "PortraitVisible"), true, "PortraitVisible", row.LineNo);

                        return new PlainEventDefinition(
                            row.Step,
                            new PortraitEvent(slot, portraitId, posX, posY, scale, visible));
                    }
                case "layer":
                    {
                        LayerTarget target = ParseLayerTarget(
                            GetValue(values, headerIndex, "LayerTarget"),
                            "LayerTarget",
                            row.LineNo);
                        int order = ParseRequiredInt(GetValue(values, headerIndex, "LayerOrder"), "LayerOrder", row.LineNo);
                        return new PlainEventDefinition(row.Step, new LayerEvent(target, order));
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
                case "portrait":
                    {
                        PortraitSlot slot = ParsePortraitSlot(GetValue(values, headerIndex, "OnTriggerArg1"), "OnTriggerArg1", lineNo);
                        string portraitId = GetValue(values, headerIndex, "OnTriggerArg2");
                        if (string.IsNullOrWhiteSpace(portraitId))
                        {
                            throw new FormatException($"line {lineNo}: OnTriggerArg2 is required for OnTriggerType=Portrait.");
                        }
                        float posX = ParseOptionalFloat(GetValue(values, headerIndex, "OnTriggerArg3"), 0f, "OnTriggerArg3", lineNo);
                        return new PortraitEvent(slot, portraitId, posX, 0f, 1f, true);
                    }
                case "layer":
                    {
                        LayerTarget target = ParseLayerTarget(GetValue(values, headerIndex, "OnTriggerArg1"), "OnTriggerArg1", lineNo);
                        int order = ParseRequiredInt(GetValue(values, headerIndex, "OnTriggerArg2"), "OnTriggerArg2", lineNo);
                        return new LayerEvent(target, order);
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
            return ScenarioCsvUtility.GetValue(values, headerIndex, key);
        }

        private static float ParseRequiredFloat(string raw, string columnName, int lineNo)
        {
            return ScenarioCsvUtility.ParseRequiredFloat(raw, columnName, lineNo);
        }

        private static int ParseRequiredInt(string raw, string columnName, int lineNo)
        {
            return ScenarioCsvUtility.ParseRequiredInt(raw, columnName, lineNo);
        }

        private static int ParseOptionalInt(string raw, int defaultValue, string columnName, int lineNo)
        {
            return ScenarioCsvUtility.ParseOptionalInt(raw, defaultValue, columnName, lineNo);
        }

        private static float ParseOptionalFloat(string raw, float defaultValue, string columnName, int lineNo)
        {
            return ScenarioCsvUtility.ParseOptionalFloat(raw, defaultValue, columnName, lineNo);
        }

        private static bool ParseOptionalBool(string raw, bool defaultValue, string columnName, int lineNo)
        {
            return ScenarioCsvUtility.ParseOptionalBool(raw, defaultValue, columnName, lineNo);
        }

        private static PortraitSlot ParsePortraitSlot(string raw, string columnName, int lineNo)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                throw new FormatException($"line {lineNo}: {columnName} is required.");
            }

            if (!Enum.TryParse(raw.Trim(), true, out PortraitSlot slot))
            {
                throw new FormatException($"line {lineNo}: invalid {columnName} '{raw}'.");
            }

            return slot;
        }

        private static LayerTarget ParseLayerTarget(string raw, string columnName, int lineNo)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                throw new FormatException($"line {lineNo}: {columnName} is required.");
            }

            if (!Enum.TryParse(raw.Trim(), true, out LayerTarget target))
            {
                throw new FormatException($"line {lineNo}: invalid {columnName} '{raw}'.");
            }

            return target;
        }

        private static List<string> ParseCsvLine(string line)
        {
            return ScenarioCsvUtility.ParseCsvLine(line);
        }

    }

}
