using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace KillChord.Editor.Scenario
{
    /// <summary>
    /// Converts lightweight authoring CSVs (events/triggers) to the legacy runtime CSV.
    /// </summary>
    public static class ScenarioSpreadsheetConverter
    {
        private static readonly string[] LegacyHeaders =
        {
            "Type", "Step", "ParentStep", "Speaker", "Text", "BackgroundId", "AnimationId", "PortraitId", "PortraitSlot",
            "FadeStart", "FadeEnd", "FadeDuration", "TriggerType", "TriggerIndex", "TriggerKeyword",
            "OnTriggerType", "OnTriggerArg1", "OnTriggerArg2", "OnTriggerArg3",
        };
        private static readonly string[] EventHeaders =
        {
            "Step", "Type", "Speaker", "Text", "BackgroundId", "AnimationId", "PortraitId", "PortraitSlot", "FadeStart", "FadeEnd", "FadeDuration",
        };
        private static readonly string[] TriggerHeaders =
        {
            "ParentStep", "TriggerType", "TriggerIndex", "TriggerKeyword", "OnTriggerType", "Arg1", "Arg2", "Arg3",
        };

        private const string AuthoringDirectory = "Assets/StreamingAssets/ScenarioAuthoring";
        private const string RuntimeDirectory = "Assets/StreamingAssets/Scenario";

        [MenuItem("Tools/KillChord/Scenario/Convert Authoring CSV (All)")]
        public static void ConvertAllFromMenu()
        {
            try
            {
                int converted = ConvertAll();
                AssetDatabase.Refresh();
                Debug.Log($"[ScenarioSpreadsheetConverter] Converted {converted} scenario file(s).");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public static int ConvertAll()
        {
            if (!Directory.Exists(AuthoringDirectory))
            {
                throw new DirectoryNotFoundException(
                    $"Authoring directory not found: {AuthoringDirectory}");
            }

            Directory.CreateDirectory(RuntimeDirectory);

            string[] eventFiles = Directory.GetFiles(AuthoringDirectory, "*.events.csv");
            int convertedCount = 0;
            foreach (string eventFile in eventFiles)
            {
                string scenarioId = GetScenarioIdFromEventFile(eventFile);
                string triggerFile = Path.Combine(AuthoringDirectory, $"{scenarioId}.triggers.csv");

                List<EventRow> events = ReadEvents(eventFile);
                List<TriggerRow> triggers = ReadTriggers(triggerFile);

                Validate(events, triggers, scenarioId);

                List<string> lines = BuildLegacyLines(events, triggers);
                string outputPath = Path.Combine(RuntimeDirectory, $"{scenarioId}.csv");
                File.WriteAllLines(outputPath, lines, new UTF8Encoding(false));
                convertedCount++;
            }

            return convertedCount;
        }

        private static string GetScenarioIdFromEventFile(string eventFilePath)
        {
            string fileName = Path.GetFileName(eventFilePath);
            const string suffix = ".events.csv";
            if (!fileName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                throw new FormatException($"Invalid event filename: {fileName}");
            }

            string id = fileName.Substring(0, fileName.Length - suffix.Length);
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new FormatException($"Scenario id is empty. file={fileName}");
            }

            return id;
        }

        private static List<EventRow> ReadEvents(string path)
        {
            if (TryReadCompactEventCsv(path, out List<EventRow> compactEvents))
            {
                return compactEvents;
            }

            if (TryReadEventCommands(path, out List<EventRow> commandEvents))
            {
                return commandEvents;
            }

            CsvTable table = ReadTable(path, EventHeaders);
            var result = new List<EventRow>(table.Rows.Count);

            foreach (CsvDataRow row in table.Rows)
            {
                int step = ParseRequiredInt(GetValue(row.Values, table.HeaderIndex, "Step"), "Step", path, row.LineNo);
                string typeRaw = GetValue(row.Values, table.HeaderIndex, "Type")?.Trim();
                if (string.IsNullOrWhiteSpace(typeRaw))
                {
                    throw new FormatException($"{path}:{row.LineNo} Type is required.");
                }

                string type = typeRaw.ToLowerInvariant();
                switch (type)
                {
                    case "text":
                        result.Add(new EventRow(
                            row.LineNo,
                            step,
                            "Text",
                            GetValue(row.Values, table.HeaderIndex, "Speaker") ?? string.Empty,
                            GetValue(row.Values, table.HeaderIndex, "Text") ?? string.Empty,
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            string.Empty));
                        break;
                    case "background":
                        {
                            string backgroundId = RequireString(
                                GetValue(row.Values, table.HeaderIndex, "BackgroundId"),
                                "BackgroundId",
                                path,
                                row.LineNo);
                            result.Add(new EventRow(
                                row.LineNo,
                                step,
                                "Background",
                                string.Empty,
                                string.Empty,
                                backgroundId,
                                string.Empty,
                                string.Empty,
                                string.Empty,
                                string.Empty,
                                string.Empty));
                            break;
                        }
                    case "animation":
                        {
                            string animationId = RequireString(
                                GetValue(row.Values, table.HeaderIndex, "AnimationId"),
                                "AnimationId",
                                path,
                                row.LineNo);
                            result.Add(new EventRow(
                                row.LineNo,
                                step,
                                "Animation",
                                string.Empty,
                                string.Empty,
                                string.Empty,
                                animationId,
                                string.Empty,
                                string.Empty,
                                string.Empty,
                                string.Empty));
                            break;
                        }
                    case "portrait":
                        {
                            string portraitId = RequireString(
                                GetValue(row.Values, table.HeaderIndex, "PortraitId"),
                                "PortraitId",
                                path,
                                row.LineNo);
                            string portraitSlot = GetValue(row.Values, table.HeaderIndex, "PortraitSlot");
                            result.Add(new EventRow(
                                row.LineNo,
                                step,
                                "Portrait",
                                string.Empty,
                                string.Empty,
                                string.Empty,
                                string.Empty,
                                portraitId,
                                portraitSlot ?? string.Empty,
                                string.Empty,
                                string.Empty,
                                string.Empty));
                            break;
                        }
                    case "fade":
                        {
                            float start = ParseRequiredFloat(
                                GetValue(row.Values, table.HeaderIndex, "FadeStart"),
                                "FadeStart",
                                path,
                                row.LineNo);
                            float end = ParseRequiredFloat(
                                GetValue(row.Values, table.HeaderIndex, "FadeEnd"),
                                "FadeEnd",
                                path,
                                row.LineNo);
                            float duration = ParseRequiredFloat(
                                GetValue(row.Values, table.HeaderIndex, "FadeDuration"),
                                "FadeDuration",
                                path,
                                row.LineNo);
                            result.Add(new EventRow(
                                row.LineNo,
                                step,
                                "Fade",
                                string.Empty,
                                string.Empty,
                                string.Empty,
                                string.Empty,
                                string.Empty,
                                start.ToString("G9", CultureInfo.InvariantCulture),
                                end.ToString("G9", CultureInfo.InvariantCulture),
                                duration.ToString("G9", CultureInfo.InvariantCulture)));
                            break;
                        }
                    default:
                        throw new FormatException($"{path}:{row.LineNo} unknown Type '{typeRaw}'.");
                }
            }

            return result;
        }

        private static List<TriggerRow> ReadTriggers(string path)
        {
            if (!File.Exists(path))
            {
                return new List<TriggerRow>();
            }

            if (TryReadCompactTriggerCsv(path, out List<TriggerRow> compactTriggers))
            {
                return compactTriggers;
            }

            if (TryReadTriggerCommands(path, out List<TriggerRow> commandTriggers))
            {
                return commandTriggers;
            }

            CsvTable table = ReadTable(path, TriggerHeaders);
            var result = new List<TriggerRow>(table.Rows.Count);

            foreach (CsvDataRow row in table.Rows)
            {
                int parentStep = ParseRequiredInt(
                    GetValue(row.Values, table.HeaderIndex, "ParentStep"),
                    "ParentStep",
                    path,
                    row.LineNo);

                string triggerTypeRaw = RequireString(
                    GetValue(row.Values, table.HeaderIndex, "TriggerType"),
                    "TriggerType",
                    path,
                    row.LineNo);
                string onTriggerTypeRaw = RequireString(
                    GetValue(row.Values, table.HeaderIndex, "OnTriggerType"),
                    "OnTriggerType",
                    path,
                    row.LineNo);

                string triggerType = triggerTypeRaw.ToLowerInvariant();
                string onTriggerType = onTriggerTypeRaw.ToLowerInvariant();

                string triggerIndex = string.Empty;
                string triggerKeyword = string.Empty;
                switch (triggerType)
                {
                    case "atcharindex":
                        {
                            int index = ParseRequiredInt(
                                GetValue(row.Values, table.HeaderIndex, "TriggerIndex"),
                                "TriggerIndex",
                                path,
                                row.LineNo);
                            if (index < 0)
                            {
                                throw new FormatException($"{path}:{row.LineNo} TriggerIndex must be >= 0.");
                            }
                            triggerIndex = index.ToString(CultureInfo.InvariantCulture);
                            break;
                        }
                    case "atkeyword":
                    case "atsuffix":
                        triggerKeyword = RequireString(
                            GetValue(row.Values, table.HeaderIndex, "TriggerKeyword"),
                            "TriggerKeyword",
                            path,
                            row.LineNo);
                        break;
                    case "attextend":
                        break;
                    default:
                        throw new FormatException($"{path}:{row.LineNo} unknown TriggerType '{triggerTypeRaw}'.");
                }

                string arg1 = string.Empty;
                string arg2 = string.Empty;
                string arg3 = string.Empty;

                switch (onTriggerType)
                {
                    case "fade":
                        arg1 = ParseRequiredFloat(
                            GetValue(row.Values, table.HeaderIndex, "Arg1"),
                            "Arg1",
                            path,
                            row.LineNo).ToString("G9", CultureInfo.InvariantCulture);
                        arg2 = ParseRequiredFloat(
                            GetValue(row.Values, table.HeaderIndex, "Arg2"),
                            "Arg2",
                            path,
                            row.LineNo).ToString("G9", CultureInfo.InvariantCulture);
                        arg3 = ParseRequiredFloat(
                            GetValue(row.Values, table.HeaderIndex, "Arg3"),
                            "Arg3",
                            path,
                            row.LineNo).ToString("G9", CultureInfo.InvariantCulture);
                        break;
                    case "background":
                    case "animation":
                        arg1 = RequireString(
                            GetValue(row.Values, table.HeaderIndex, "Arg1"),
                            "Arg1",
                            path,
                            row.LineNo);
                        break;
                    case "portrait":
                        arg1 = RequireString(
                            GetValue(row.Values, table.HeaderIndex, "Arg1"),
                            "Arg1",
                            path,
                            row.LineNo);
                        arg2 = GetValue(row.Values, table.HeaderIndex, "Arg2")?.Trim() ?? string.Empty;
                        break;
                    default:
                        throw new FormatException($"{path}:{row.LineNo} unknown OnTriggerType '{onTriggerTypeRaw}'.");
                }

                result.Add(new TriggerRow(
                    row.LineNo,
                    parentStep,
                    ToCanonicalName(triggerTypeRaw),
                    triggerIndex,
                    triggerKeyword,
                    ToCanonicalName(onTriggerTypeRaw),
                    arg1,
                    arg2,
                    arg3));
            }

            return result;
        }

        private static void Validate(IReadOnlyList<EventRow> events, IReadOnlyList<TriggerRow> triggers, string scenarioId)
        {
            if (events.Count == 0)
            {
                throw new FormatException($"Scenario '{scenarioId}' has no events.");
            }

            var eventByStep = new Dictionary<int, EventRow>();
            foreach (EventRow e in events)
            {
                if (eventByStep.ContainsKey(e.Step))
                {
                    throw new FormatException(
                        $"Scenario '{scenarioId}' has duplicated Step '{e.Step}' (line {e.LineNo}).");
                }
                eventByStep.Add(e.Step, e);
            }

            foreach (TriggerRow trigger in triggers)
            {
                if (!eventByStep.TryGetValue(trigger.ParentStep, out EventRow parent))
                {
                    throw new FormatException(
                        $"Scenario '{scenarioId}' trigger line {trigger.LineNo}: ParentStep '{trigger.ParentStep}' was not found.");
                }

                if (!string.Equals(parent.Type, "Text", StringComparison.OrdinalIgnoreCase))
                {
                    throw new FormatException(
                        $"Scenario '{scenarioId}' trigger line {trigger.LineNo}: ParentStep '{trigger.ParentStep}' must point Text event.");
                }
            }
        }

        private static List<string> BuildLegacyLines(IReadOnlyList<EventRow> events, IReadOnlyList<TriggerRow> triggers)
        {
            var lines = new List<string>(events.Count + triggers.Count + 1)
            {
                string.Join(",", LegacyHeaders),
            };

            ILookup<int, TriggerRow> triggerLookup = triggers.ToLookup(t => t.ParentStep);
            foreach (EventRow e in events)
            {
                string[] row = new string[LegacyHeaders.Length];
                row[0] = e.Type;
                row[1] = e.Step.ToString(CultureInfo.InvariantCulture);
                row[3] = e.Speaker;
                row[4] = e.Text;
                row[5] = e.BackgroundId;
                row[6] = e.AnimationId;
                row[7] = e.PortraitId;
                row[8] = e.PortraitSlot;
                row[9] = e.FadeStart;
                row[10] = e.FadeEnd;
                row[11] = e.FadeDuration;
                lines.Add(ToCsvRow(row));

                foreach (TriggerRow t in triggerLookup[e.Step])
                {
                    string[] triggerRow = new string[LegacyHeaders.Length];
                    triggerRow[0] = "Trigger";
                    triggerRow[2] = t.ParentStep.ToString(CultureInfo.InvariantCulture);
                    triggerRow[12] = t.TriggerType;
                    triggerRow[13] = t.TriggerIndex;
                    triggerRow[14] = t.TriggerKeyword;
                    triggerRow[15] = t.OnTriggerType;
                    triggerRow[16] = t.Arg1;
                    triggerRow[17] = t.Arg2;
                    triggerRow[18] = t.Arg3;
                    lines.Add(ToCsvRow(triggerRow));
                }
            }

            return lines;
        }

        private static CsvTable ReadTable(string path, IReadOnlyList<string> expectedHeaders)
        {
            string[] lines = File.ReadAllLines(path, Encoding.UTF8);
            Dictionary<string, int> headerIndex = null;
            var rows = new List<CsvDataRow>(Math.Max(4, lines.Length - 1));
            bool initialized = false;

            for (int lineNo = 1; lineNo <= lines.Length; lineNo++)
            {
                string line = lines[lineNo - 1];
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (line.TrimStart().StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }

                List<string> values = ParseCsvLine(line);
                if (!initialized)
                {
                    if (IsHeaderRow(values, expectedHeaders))
                    {
                        headerIndex = BuildHeaderIndex(values);
                    }
                    else
                    {
                        headerIndex = BuildHeaderIndex(expectedHeaders);
                        rows.Add(new CsvDataRow(lineNo, values));
                    }
                    initialized = true;
                    continue;
                }

                rows.Add(new CsvDataRow(lineNo, values));
            }

            if (!initialized)
            {
                throw new FormatException($"{path} has no data rows.");
            }

            return new CsvTable(headerIndex, rows);
        }

        private static bool TryReadEventCommands(string path, out List<EventRow> events)
        {
            List<LineEntry> lines = ReadNonCommentLines(path);
            events = new List<EventRow>();
            if (lines.Count == 0)
            {
                return false;
            }

            int startIndex = 0;
            List<string> firstTokens = TokenizeCommand(lines[0].Text);
            if (firstTokens.Count == 1 && string.Equals(firstTokens[0], "Command", StringComparison.OrdinalIgnoreCase))
            {
                startIndex = 1;
                if (lines.Count == 1)
                {
                    return true;
                }
                firstTokens = TokenizeCommand(lines[1].Text);
            }

            if (firstTokens.Count < 2 || !IsInt(firstTokens[0]))
            {
                return false;
            }

            for (int i = startIndex; i < lines.Count; i++)
            {
                List<string> tokens = TokenizeCommand(lines[i].Text);
                if (tokens.Count == 0)
                {
                    continue;
                }

                if (tokens.Count < 2)
                {
                    throw new FormatException($"{path}:{lines[i].LineNo} invalid event command.");
                }

                int step = ParseRequiredInt(tokens[0], "Step", path, lines[i].LineNo);
                string typeRaw = tokens[1];
                string type = typeRaw.ToLowerInvariant();

                switch (type)
                {
                    case "text":
                        {
                            string rest = SliceCommandRemainder(lines[i].Text, 2);
                            string speaker = string.Empty;
                            string text = rest;

                            int separatorIndex = rest.IndexOf('|');
                            if (separatorIndex >= 0)
                            {
                                speaker = rest.Substring(0, separatorIndex).Trim();
                                text = rest.Substring(separatorIndex + 1).Trim();
                            }

                            events.Add(new EventRow(
                                lines[i].LineNo,
                                step,
                                "Text",
                                speaker,
                                text,
                                string.Empty,
                                string.Empty,
                                string.Empty,
                                string.Empty,
                                string.Empty,
                                string.Empty));
                            break;
                        }
                    case "background":
                        if (tokens.Count < 3)
                        {
                            throw new FormatException($"{path}:{lines[i].LineNo} Background requires BackgroundId.");
                        }
                        events.Add(new EventRow(
                            lines[i].LineNo,
                            step,
                            "Background",
                            string.Empty,
                            string.Empty,
                            tokens[2],
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            string.Empty));
                        break;
                    case "animation":
                        if (tokens.Count < 3)
                        {
                            throw new FormatException($"{path}:{lines[i].LineNo} Animation requires AnimationId.");
                        }
                        events.Add(new EventRow(
                            lines[i].LineNo,
                            step,
                            "Animation",
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            tokens[2],
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            string.Empty));
                        break;
                    case "portrait":
                        if (tokens.Count < 3)
                        {
                            throw new FormatException($"{path}:{lines[i].LineNo} Portrait requires PortraitId.");
                        }
                        string commandPortraitSlot = tokens.Count >= 4 ? tokens[3] : string.Empty;
                        events.Add(new EventRow(
                            lines[i].LineNo,
                            step,
                            "Portrait",
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            tokens[2],
                            commandPortraitSlot,
                            string.Empty,
                            string.Empty,
                            string.Empty));
                        break;
                    case "fade":
                        if (tokens.Count < 5)
                        {
                            throw new FormatException($"{path}:{lines[i].LineNo} Fade requires start end duration.");
                        }
                        events.Add(new EventRow(
                            lines[i].LineNo,
                            step,
                            "Fade",
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            ParseRequiredFloat(tokens[2], "FadeStart", path, lines[i].LineNo).ToString("G9", CultureInfo.InvariantCulture),
                            ParseRequiredFloat(tokens[3], "FadeEnd", path, lines[i].LineNo).ToString("G9", CultureInfo.InvariantCulture),
                            ParseRequiredFloat(tokens[4], "FadeDuration", path, lines[i].LineNo).ToString("G9", CultureInfo.InvariantCulture)));
                        break;
                    default:
                        throw new FormatException($"{path}:{lines[i].LineNo} unknown event Type '{typeRaw}'.");
                }
            }

            return true;
        }

        private static bool TryReadCompactEventCsv(string path, out List<EventRow> events)
        {
            List<LineEntry> lines = ReadNonCommentLines(path);
            events = new List<EventRow>();
            if (lines.Count == 0)
            {
                return false;
            }

            List<string> first = ParseCsvLine(lines[0].Text);
            if (first.Count < 2 || !IsInt(first[0]) || !IsEventType(first[1]))
            {
                return false;
            }

            foreach (LineEntry line in lines)
            {
                List<string> cols = ParseCsvLine(line.Text);
                if (cols.Count < 2)
                {
                    throw new FormatException($"{path}:{line.LineNo} invalid compact event row.");
                }

                int step = ParseRequiredInt(cols[0], "Step", path, line.LineNo);
                string typeRaw = cols[1].Trim();
                string type = typeRaw.ToLowerInvariant();

                switch (type)
                {
                    case "text":
                        {
                            // Step,Text,<Text>
                            // Step,Text,<Speaker>,<Text>
                            if (cols.Count < 3)
                            {
                                throw new FormatException($"{path}:{line.LineNo} Text requires at least text.");
                            }

                            string speaker = string.Empty;
                            string text;
                            if (cols.Count == 3)
                            {
                                text = cols[2];
                            }
                            else
                            {
                                speaker = cols[2];
                                text = JoinColumns(cols, 3);
                            }

                            events.Add(new EventRow(
                                line.LineNo,
                                step,
                                "Text",
                                speaker,
                                text,
                                string.Empty,
                                string.Empty,
                                string.Empty,
                                string.Empty,
                                string.Empty,
                                string.Empty));
                            break;
                        }
                    case "background":
                        if (cols.Count < 3)
                        {
                            throw new FormatException($"{path}:{line.LineNo} Background requires BackgroundId.");
                        }
                        events.Add(new EventRow(
                            line.LineNo,
                            step,
                            "Background",
                            string.Empty,
                            string.Empty,
                            cols[2].Trim(),
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            string.Empty));
                        break;
                    case "animation":
                        if (cols.Count < 3)
                        {
                            throw new FormatException($"{path}:{line.LineNo} Animation requires AnimationId.");
                        }
                        events.Add(new EventRow(
                            line.LineNo,
                            step,
                            "Animation",
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            cols[2].Trim(),
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            string.Empty));
                        break;
                    case "portrait":
                        if (cols.Count < 3)
                        {
                            throw new FormatException($"{path}:{line.LineNo} Portrait requires PortraitId.");
                        }
                        string compactPortraitSlot = cols.Count >= 4 ? cols[3].Trim() : string.Empty;
                        events.Add(new EventRow(
                            line.LineNo,
                            step,
                            "Portrait",
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            cols[2].Trim(),
                            compactPortraitSlot,
                            string.Empty,
                            string.Empty,
                            string.Empty));
                        break;
                    case "fade":
                        if (cols.Count < 5)
                        {
                            throw new FormatException($"{path}:{line.LineNo} Fade requires start,end,duration.");
                        }
                        events.Add(new EventRow(
                            line.LineNo,
                            step,
                            "Fade",
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            ParseRequiredFloat(cols[2], "FadeStart", path, line.LineNo).ToString("G9", CultureInfo.InvariantCulture),
                            ParseRequiredFloat(cols[3], "FadeEnd", path, line.LineNo).ToString("G9", CultureInfo.InvariantCulture),
                            ParseRequiredFloat(cols[4], "FadeDuration", path, line.LineNo).ToString("G9", CultureInfo.InvariantCulture)));
                        break;
                    default:
                        throw new FormatException($"{path}:{line.LineNo} unknown event Type '{typeRaw}'.");
                }
            }

            return true;
        }

        private static bool TryReadTriggerCommands(string path, out List<TriggerRow> triggers)
        {
            List<LineEntry> lines = ReadNonCommentLines(path);
            triggers = new List<TriggerRow>();
            if (lines.Count == 0)
            {
                return true;
            }

            int startIndex = 0;
            List<string> firstTokens = TokenizeCommand(lines[0].Text);
            if (firstTokens.Count == 1 && string.Equals(firstTokens[0], "Command", StringComparison.OrdinalIgnoreCase))
            {
                startIndex = 1;
                if (lines.Count == 1)
                {
                    return true;
                }
                firstTokens = TokenizeCommand(lines[1].Text);
            }

            if (firstTokens.Count < 2 || !IsInt(firstTokens[0]))
            {
                return false;
            }

            for (int i = startIndex; i < lines.Count; i++)
            {
                List<string> tokens = TokenizeCommand(lines[i].Text);
                if (tokens.Count == 0)
                {
                    continue;
                }

                if (tokens.Count < 4)
                {
                    throw new FormatException($"{path}:{lines[i].LineNo} invalid trigger command.");
                }

                int parentStep = ParseRequiredInt(tokens[0], "ParentStep", path, lines[i].LineNo);
                string triggerTypeRaw = tokens[1];
                string triggerType = triggerTypeRaw.ToLowerInvariant();

                int cursor = 2;
                string triggerIndex = string.Empty;
                string triggerKeyword = string.Empty;

                switch (triggerType)
                {
                    case "atcharindex":
                        triggerIndex = ParseRequiredInt(tokens[cursor], "TriggerIndex", path, lines[i].LineNo).ToString(CultureInfo.InvariantCulture);
                        cursor += 1;
                        break;
                    case "atkeyword":
                    case "atsuffix":
                        triggerKeyword = tokens[cursor];
                        cursor += 1;
                        break;
                    case "attextend":
                        break;
                    default:
                        throw new FormatException($"{path}:{lines[i].LineNo} unknown TriggerType '{triggerTypeRaw}'.");
                }

                if (cursor >= tokens.Count)
                {
                    throw new FormatException($"{path}:{lines[i].LineNo} OnTriggerType is required.");
                }

                string onTriggerTypeRaw = tokens[cursor];
                string onTriggerType = onTriggerTypeRaw.ToLowerInvariant();
                cursor += 1;

                string arg1 = string.Empty;
                string arg2 = string.Empty;
                string arg3 = string.Empty;

                switch (onTriggerType)
                {
                    case "fade":
                        if (tokens.Count < cursor + 3)
                        {
                            throw new FormatException($"{path}:{lines[i].LineNo} Fade trigger requires 3 args.");
                        }
                        arg1 = ParseRequiredFloat(tokens[cursor], "Arg1", path, lines[i].LineNo).ToString("G9", CultureInfo.InvariantCulture);
                        arg2 = ParseRequiredFloat(tokens[cursor + 1], "Arg2", path, lines[i].LineNo).ToString("G9", CultureInfo.InvariantCulture);
                        arg3 = ParseRequiredFloat(tokens[cursor + 2], "Arg3", path, lines[i].LineNo).ToString("G9", CultureInfo.InvariantCulture);
                        break;
                    case "background":
                    case "animation":
                        if (tokens.Count < cursor + 1)
                        {
                            throw new FormatException($"{path}:{lines[i].LineNo} {onTriggerTypeRaw} trigger requires Arg1.");
                        }
                        arg1 = tokens[cursor];
                        break;
                    case "portrait":
                        if (tokens.Count < cursor + 1)
                        {
                            throw new FormatException($"{path}:{lines[i].LineNo} {onTriggerTypeRaw} trigger requires Arg1.");
                        }
                        arg1 = tokens[cursor];
                        arg2 = tokens.Count >= cursor + 2 ? tokens[cursor + 1] : string.Empty;
                        break;
                    default:
                        throw new FormatException($"{path}:{lines[i].LineNo} unknown OnTriggerType '{onTriggerTypeRaw}'.");
                }

                triggers.Add(new TriggerRow(
                    lines[i].LineNo,
                    parentStep,
                    ToCanonicalName(triggerTypeRaw),
                    triggerIndex,
                    triggerKeyword,
                    ToCanonicalName(onTriggerTypeRaw),
                    arg1,
                    arg2,
                    arg3));
            }

            return true;
        }

        private static bool TryReadCompactTriggerCsv(string path, out List<TriggerRow> triggers)
        {
            List<LineEntry> lines = ReadNonCommentLines(path);
            triggers = new List<TriggerRow>();
            if (lines.Count == 0)
            {
                return true;
            }

            List<string> first = ParseCsvLine(lines[0].Text);
            if (first.Count < 2 || !IsInt(first[0]) || !IsTriggerType(first[1]))
            {
                return false;
            }

            foreach (LineEntry line in lines)
            {
                List<string> cols = ParseCsvLine(line.Text);
                if (cols.Count < 3)
                {
                    throw new FormatException($"{path}:{line.LineNo} invalid compact trigger row.");
                }

                int parentStep = ParseRequiredInt(cols[0], "ParentStep", path, line.LineNo);
                string triggerTypeRaw = cols[1].Trim();
                string triggerType = triggerTypeRaw.ToLowerInvariant();

                int cursor = 2;
                string triggerIndex = string.Empty;
                string triggerKeyword = string.Empty;

                switch (triggerType)
                {
                    case "atcharindex":
                        triggerIndex = ParseRequiredInt(cols[cursor], "TriggerIndex", path, line.LineNo).ToString(CultureInfo.InvariantCulture);
                        cursor += 1;
                        break;
                    case "atkeyword":
                    case "atsuffix":
                        triggerKeyword = RequireString(cols[cursor], "TriggerKeyword", path, line.LineNo);
                        cursor += 1;
                        break;
                    case "attextend":
                        break;
                    default:
                        throw new FormatException($"{path}:{line.LineNo} unknown TriggerType '{triggerTypeRaw}'.");
                }

                if (cursor >= cols.Count)
                {
                    throw new FormatException($"{path}:{line.LineNo} OnTriggerType is required.");
                }

                string onTriggerTypeRaw = cols[cursor].Trim();
                string onTriggerType = onTriggerTypeRaw.ToLowerInvariant();
                cursor += 1;

                string arg1 = string.Empty;
                string arg2 = string.Empty;
                string arg3 = string.Empty;

                switch (onTriggerType)
                {
                    case "fade":
                        if (cols.Count < cursor + 3)
                        {
                            throw new FormatException($"{path}:{line.LineNo} Fade trigger requires Arg1,Arg2,Arg3.");
                        }
                        arg1 = ParseRequiredFloat(cols[cursor], "Arg1", path, line.LineNo).ToString("G9", CultureInfo.InvariantCulture);
                        arg2 = ParseRequiredFloat(cols[cursor + 1], "Arg2", path, line.LineNo).ToString("G9", CultureInfo.InvariantCulture);
                        arg3 = ParseRequiredFloat(cols[cursor + 2], "Arg3", path, line.LineNo).ToString("G9", CultureInfo.InvariantCulture);
                        break;
                    case "background":
                    case "animation":
                        if (cols.Count < cursor + 1)
                        {
                            throw new FormatException($"{path}:{line.LineNo} {onTriggerTypeRaw} trigger requires Arg1.");
                        }
                        arg1 = RequireString(cols[cursor], "Arg1", path, line.LineNo);
                        break;
                    case "portrait":
                        if (cols.Count < cursor + 1)
                        {
                            throw new FormatException($"{path}:{line.LineNo} {onTriggerTypeRaw} trigger requires Arg1.");
                        }
                        arg1 = RequireString(cols[cursor], "Arg1", path, line.LineNo);
                        arg2 = cols.Count >= cursor + 2 ? cols[cursor + 1].Trim() : string.Empty;
                        break;
                    default:
                        throw new FormatException($"{path}:{line.LineNo} unknown OnTriggerType '{onTriggerTypeRaw}'.");
                }

                triggers.Add(new TriggerRow(
                    line.LineNo,
                    parentStep,
                    ToCanonicalName(triggerTypeRaw),
                    triggerIndex,
                    triggerKeyword,
                    ToCanonicalName(onTriggerTypeRaw),
                    arg1,
                    arg2,
                    arg3));
            }

            return true;
        }

        private static List<LineEntry> ReadNonCommentLines(string path)
        {
            string[] rawLines = File.ReadAllLines(path, Encoding.UTF8);
            var lines = new List<LineEntry>(rawLines.Length);
            for (int i = 0; i < rawLines.Length; i++)
            {
                string line = rawLines[i];
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (line.TrimStart().StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }

                lines.Add(new LineEntry(i + 1, line));
            }

            return lines;
        }

        private static bool IsInt(string raw)
        {
            return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out _);
        }

        private static bool IsEventType(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return false;
            }

            switch (raw.Trim().ToLowerInvariant())
            {
                case "text":
                case "background":
                case "animation":
                case "portrait":
                case "fade":
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsTriggerType(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return false;
            }

            switch (raw.Trim().ToLowerInvariant())
            {
                case "atcharindex":
                case "atkeyword":
                case "atsuffix":
                case "attextend":
                    return true;
                default:
                    return false;
            }
        }

        private static string JoinColumns(IReadOnlyList<string> cols, int startIndex)
        {
            if (startIndex >= cols.Count)
            {
                return string.Empty;
            }

            var result = new StringBuilder();
            for (int i = startIndex; i < cols.Count; i++)
            {
                if (i > startIndex)
                {
                    result.Append(",");
                }
                result.Append(cols[i]);
            }
            return result.ToString();
        }

        private static string SliceCommandRemainder(string source, int skipTokens)
        {
            List<string> tokens = TokenizeCommand(source);
            if (tokens.Count <= skipTokens)
            {
                return string.Empty;
            }

            return string.Join(" ", tokens.GetRange(skipTokens, tokens.Count - skipTokens));
        }

        private static List<string> TokenizeCommand(string line)
        {
            var tokens = new List<string>();
            var current = new StringBuilder(line.Length);
            bool inQuote = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                {
                    inQuote = !inQuote;
                    continue;
                }

                if (!inQuote && char.IsWhiteSpace(c))
                {
                    if (current.Length > 0)
                    {
                        tokens.Add(current.ToString());
                        current.Clear();
                    }
                    continue;
                }

                current.Append(c);
            }

            if (current.Length > 0)
            {
                tokens.Add(current.ToString());
            }

            return tokens;
        }

        private static bool IsHeaderRow(IReadOnlyList<string> values, IReadOnlyList<string> expectedHeaders)
        {
            int compareCount = Math.Min(values.Count, expectedHeaders.Count);
            int matched = 0;
            for (int i = 0; i < compareCount; i++)
            {
                if (string.Equals(values[i]?.Trim(), expectedHeaders[i], StringComparison.OrdinalIgnoreCase))
                {
                    matched++;
                }
            }

            return matched >= Math.Min(2, expectedHeaders.Count);
        }

        private static Dictionary<string, int> BuildHeaderIndex(IReadOnlyList<string> headers)
        {
            var index = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < headers.Count; i++)
            {
                string key = headers[i]?.Trim();
                if (string.IsNullOrEmpty(key))
                {
                    continue;
                }
                index[key] = i;
            }
            return index;
        }

        private static string GetValue(
            IReadOnlyList<string> values,
            IReadOnlyDictionary<string, int> headerIndex,
            string key)
        {
            if (!headerIndex.TryGetValue(key, out int idx))
            {
                return string.Empty;
            }

            if (idx < 0 || idx >= values.Count)
            {
                return string.Empty;
            }

            return values[idx];
        }

        private static int ParseRequiredInt(string raw, string column, string path, int lineNo)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                throw new FormatException($"{path}:{lineNo} {column} is required.");
            }

            if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
            {
                throw new FormatException($"{path}:{lineNo} {column} must be int.");
            }

            return value;
        }

        private static float ParseRequiredFloat(string raw, string column, string path, int lineNo)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                throw new FormatException($"{path}:{lineNo} {column} is required.");
            }

            if (!float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
            {
                throw new FormatException($"{path}:{lineNo} {column} must be number.");
            }

            return value;
        }

        private static string RequireString(string raw, string column, string path, int lineNo)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                throw new FormatException($"{path}:{lineNo} {column} is required.");
            }

            return raw.Trim();
        }

        private static string ToCanonicalName(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return string.Empty;
            }

            string lower = raw.Trim().ToLowerInvariant();
            return lower switch
            {
                "text" => "Text",
                "background" => "Background",
                "animation" => "Animation",
                "portrait" => "Portrait",
                "fade" => "Fade",
                "trigger" => "Trigger",
                "atcharindex" => "AtCharIndex",
                "atkeyword" => "AtKeyword",
                "atsuffix" => "AtSuffix",
                "attextend" => "AtTextEnd",
                _ => raw.Trim(),
            };
        }

        private static string ToCsvRow(IReadOnlyList<string> fields)
        {
            var escaped = new string[fields.Count];
            for (int i = 0; i < fields.Count; i++)
            {
                escaped[i] = EscapeCsv(fields[i] ?? string.Empty);
            }

            return string.Join(",", escaped);
        }

        private static string EscapeCsv(string value)
        {
            if (!value.Contains(",") && !value.Contains("\"") && !value.Contains("\n") && !value.Contains("\r"))
            {
                return value;
            }

            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        private static List<string> ParseCsvLine(string line)
        {
            var fields = new List<string>();
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

        private readonly struct CsvTable
        {
            public CsvTable(Dictionary<string, int> headerIndex, List<CsvDataRow> rows)
            {
                HeaderIndex = headerIndex;
                Rows = rows;
            }

            public Dictionary<string, int> HeaderIndex { get; }
            public List<CsvDataRow> Rows { get; }
        }

        private readonly struct CsvDataRow
        {
            public CsvDataRow(int lineNo, List<string> values)
            {
                LineNo = lineNo;
                Values = values;
            }

            public int LineNo { get; }
            public List<string> Values { get; }
        }

        private readonly struct LineEntry
        {
            public LineEntry(int lineNo, string text)
            {
                LineNo = lineNo;
                Text = text;
            }

            public int LineNo { get; }
            public string Text { get; }
        }

        private readonly struct EventRow
        {
            public EventRow(
                int lineNo,
                int step,
                string type,
                string speaker,
                string text,
                string backgroundId,
                string animationId,
                string portraitId,
                string fadeStart,
                string fadeEnd,
                string fadeDuration)
                : this(
                    lineNo,
                    step,
                    type,
                    speaker,
                    text,
                    backgroundId,
                    animationId,
                    portraitId,
                    string.Empty,
                    fadeStart,
                    fadeEnd,
                    fadeDuration)
            {
            }

            public EventRow(
                int lineNo,
                int step,
                string type,
                string speaker,
                string text,
                string backgroundId,
                string animationId,
                string portraitId,
                string portraitSlot,
                string fadeStart,
                string fadeEnd,
                string fadeDuration)
            {
                LineNo = lineNo;
                Step = step;
                Type = type;
                Speaker = speaker;
                Text = text;
                BackgroundId = backgroundId;
                AnimationId = animationId;
                PortraitId = portraitId;
                PortraitSlot = portraitSlot;
                FadeStart = fadeStart;
                FadeEnd = fadeEnd;
                FadeDuration = fadeDuration;
            }

            public int LineNo { get; }
            public int Step { get; }
            public string Type { get; }
            public string Speaker { get; }
            public string Text { get; }
            public string BackgroundId { get; }
            public string AnimationId { get; }
            public string PortraitId { get; }
            public string PortraitSlot { get; }
            public string FadeStart { get; }
            public string FadeEnd { get; }
            public string FadeDuration { get; }
        }

        private readonly struct TriggerRow
        {
            public TriggerRow(
                int lineNo,
                int parentStep,
                string triggerType,
                string triggerIndex,
                string triggerKeyword,
                string onTriggerType,
                string arg1,
                string arg2,
                string arg3)
            {
                LineNo = lineNo;
                ParentStep = parentStep;
                TriggerType = triggerType;
                TriggerIndex = triggerIndex;
                TriggerKeyword = triggerKeyword;
                OnTriggerType = onTriggerType;
                Arg1 = arg1;
                Arg2 = arg2;
                Arg3 = arg3;
            }

            public int LineNo { get; }
            public int ParentStep { get; }
            public string TriggerType { get; }
            public string TriggerIndex { get; }
            public string TriggerKeyword { get; }
            public string OnTriggerType { get; }
            public string Arg1 { get; }
            public string Arg2 { get; }
            public string Arg3 { get; }
        }
    }
}
