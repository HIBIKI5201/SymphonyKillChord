using KillChord.Runtime.Application.OutGame.Scenario;
using KillChord.Runtime.Domain.OutGame.Scenario;
using KillChord.Runtime.Utility.Identity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace KillChord.Runtime.InfraStructure.OutGame.Scenario
{
    /// <summary>
    /// シナリオ定義ファイルを読み込みシナリオデータへ変換する。
    /// </summary>
    public class ScenarioRepository : IScenarioRepository
    {
        /// <summary>
        /// シナリオ ID から再生用データを読み込む。
        /// </summary>
        public async ValueTask<ScenarioDefinition> FindByIdAsync(string id, CancellationToken ct)
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
                return new ScenarioDefinition(Array.Empty<IScenarioEvent>());
            }

            string firstDataLine = FindFirstDataLine(lines);
            if (string.IsNullOrWhiteSpace(firstDataLine))
            {
                return new ScenarioDefinition(Array.Empty<IScenarioEvent>());
            }

            if (firstDataLine.TrimStart().StartsWith($"{TYPE_COLUMN},", StringComparison.OrdinalIgnoreCase))
            {
                return ParseNormalizedCsv(lines);
            }

            return ParseAuthoringCsv(lines);
        }

        private const string TYPE_COLUMN = "Type";
        private const string STEP_COLUMN = "Step";
        private const string PARENT_STEP_COLUMN = "ParentStep";
        private const string SPEAKER_COLUMN = "Speaker";
        private const string TEXT_COLUMN = "Text";
        private const string BACKGROUND_ID_COLUMN = "BackgroundId";
        private const string ANIMATION_ID_COLUMN = "AnimationId";
        private const string FADE_START_COLUMN = "FadeStart";
        private const string FADE_END_COLUMN = "FadeEnd";
        private const string FADE_DURATION_COLUMN = "FadeDuration";
        private const string FADE_TARGET_COLUMN = "FadeTarget";
        private const string FADE_MODE_COLUMN = "FadeMode";
        private const string PORTRAIT_SLOT_COLUMN = "PortraitSlot";
        private const string PORTRAIT_ID_COLUMN = "PortraitId";
        private const string PORTRAIT_POS_X_COLUMN = "PortraitPosX";
        private const string PORTRAIT_POS_Y_COLUMN = "PortraitPosY";
        private const string PORTRAIT_SCALE_COLUMN = "PortraitScale";
        private const string PORTRAIT_VISIBLE_COLUMN = "PortraitVisible";
        private const string LAYER_TARGET_COLUMN = "LayerTarget";
        private const string LAYER_ORDER_COLUMN = "LayerOrder";
        private const string TRIGGER_TYPE_COLUMN = "TriggerType";
        private const string TRIGGER_INDEX_COLUMN = "TriggerIndex";
        private const string TRIGGER_KEYWORD_COLUMN = "TriggerKeyword";
        private const string ON_TRIGGER_TYPE_COLUMN = "OnTriggerType";
        private const string ON_TRIGGER_ARG_1_COLUMN = "OnTriggerArg1";
        private const string ON_TRIGGER_ARG_2_COLUMN = "OnTriggerArg2";
        private const string ON_TRIGGER_ARG_3_COLUMN = "OnTriggerArg3";
        private const string ON_TRIGGER_ARG_4_COLUMN = "OnTriggerArg4";
        private const string ON_TRIGGER_ARG_5_COLUMN = "OnTriggerArg5";

        private const string TEXT_EVENT_TYPE = "text";
        private const string BACKGROUND_EVENT_TYPE = "background";
        private const string ANIMATION_EVENT_TYPE = "animation";
        private const string FADE_EVENT_TYPE = "fade";
        private const string PORTRAIT_EVENT_TYPE = "portrait";
        private const string LAYER_EVENT_TYPE = "layer";
        private const string TRIGGER_EVENT_TYPE = "trigger";

        private const string NONE_TRIGGER_TYPE = "none";
        private const string AT_CHAR_INDEX_TRIGGER_TYPE = "atcharindex";
        private const string AT_KEYWORD_TRIGGER_TYPE = "atkeyword";
        private const string AT_SUFFIX_TRIGGER_TYPE = "atsuffix";
        private const string AT_TEXT_END_TRIGGER_TYPE = "attextend";

        /// <summary>
        /// 正規化済み CSV をシナリオデータへ変換する。
        /// </summary>
        private static ScenarioDefinition ParseNormalizedCsv(string[] lines)
        {
            if (lines.Length <= 1)
            {
                return new ScenarioDefinition(Array.Empty<IScenarioEvent>());
            }

            // 1行目のヘッダーから列の位置を引けるようにする。
            List<string> headers = ParseCsvLine(lines[0]);
            var headerIndex = BuildHeaderIndex(headers);
            var eventRows = new List<EventRow>(Math.Max(4, lines.Length - 1));
            var triggerRows = new List<TriggerRow>(Math.Max(2, lines.Length / 2));
            int autoStep = 1;

            // 2行目以降を読み、空行とコメント行は飛ばす。トリガー行とイベント行に分けて集める。
            for (int lineNo = 2; lineNo <= lines.Length; lineNo++)
            {
                string raw = lines[lineNo - 1];
                if (string.IsNullOrWhiteSpace(raw)) continue;
                if (raw.TrimStart().StartsWith("#", StringComparison.Ordinal)) continue;

                List<string> values = ParseCsvLine(raw);
                string type = GetValue(values, headerIndex, TYPE_COLUMN)?.Trim();
                if (string.IsNullOrWhiteSpace(type))
                {
                    continue;
                }

                if (type.Equals(TRIGGER_EVENT_TYPE, StringComparison.OrdinalIgnoreCase))
                {
                    int parentStep = ParseRequiredInt(
                        GetValue(values, headerIndex, PARENT_STEP_COLUMN),
                        PARENT_STEP_COLUMN,
                        lineNo);
                    triggerRows.Add(new TriggerRow(lineNo, parentStep, values));
                    continue;
                }

                // ステップ番号が省略された行は、直前の番号の次を割り当てる。
                int step = ParseOptionalInt(
                    GetValue(values, headerIndex, STEP_COLUMN),
                    autoStep,
                    STEP_COLUMN,
                    lineNo);
                autoStep = Math.Max(autoStep + 1, step + 1);
                eventRows.Add(new EventRow(lineNo, step, type, values));
            }

            // イベントを作り、ステップ番号の重複を確認する。
            var definitions = new Dictionary<int, EventDefinition>();
            var orderedSteps = new List<int>(eventRows.Count);

            foreach (EventRow row in eventRows)
            {
                if (definitions.ContainsKey(row.Step))
                {
                    throw new FormatException($"line {row.LineNo}: duplicated {STEP_COLUMN} '{row.Step}'.");
                }

                EventDefinition definition = CreateEventDefinition(row, headerIndex);
                definitions.Add(row.Step, definition);
                orderedSteps.Add(row.Step);
            }

            // トリガーを親のテキストイベントへ追加する。
            foreach (TriggerRow row in triggerRows)
            {
                if (!definitions.TryGetValue(row.ParentStep, out EventDefinition parent))
                {
                    throw new FormatException(
                        $"line {row.LineNo}: {PARENT_STEP_COLUMN} '{row.ParentStep}' was not found.");
                }
                if (parent is not TextEventDefinition textParent)
                {
                    throw new FormatException(
                        $"line {row.LineNo}: {PARENT_STEP_COLUMN} '{row.ParentStep}' must point Text event.");
                }

                TextTimingTrigger trigger = CreateTrigger(row.Values, headerIndex, row.LineNo, textParent.Text);
                textParent.AddTrigger(trigger);
            }

            // 記述順にイベントを並べて返す。
            var events = new List<IScenarioEvent>(orderedSteps.Count);
            foreach (int step in orderedSteps)
            {
                events.Add(definitions[step].ToEvent());
            }

            return new ScenarioDefinition(events);
        }

        /// <summary>
        /// オーサリング形式の CSV をシナリオデータへ変換する。
        /// </summary>
        private static ScenarioDefinition ParseAuthoringCsv(string[] lines)
        {
            var definitions = new Dictionary<int, EventDefinition>();
            var orderedSteps = new List<int>(Math.Max(4, lines.Length));
            var pendingTriggers = new List<AuthoringTriggerRow>(Math.Max(2, lines.Length / 2));

            // 1列目をステップ番号、2列目を種類として読む。空行とコメント行は飛ばす。
            for (int lineNo = 1; lineNo <= lines.Length; lineNo++)
            {
                string raw = lines[lineNo - 1];
                if (string.IsNullOrWhiteSpace(raw)) continue;
                if (raw.TrimStart().StartsWith("#", StringComparison.Ordinal)) continue;

                List<string> fields = ParseCsvLine(raw);
                if (fields.Count < 2)
                {
                    throw new FormatException(
                        $"line {lineNo}: authoring csv requires at least {STEP_COLUMN} and {TYPE_COLUMN}.");
                }

                int step = ParseRequiredInt(fields[0], STEP_COLUMN, lineNo);
                string type = fields[1]?.Trim();
                if (string.IsNullOrWhiteSpace(type))
                {
                    throw new FormatException($"line {lineNo}: {TYPE_COLUMN} is required.");
                }

                // トリガー行は、すべてのイベントを作った後で親に追加するため保留する。
                if (type.Equals(TRIGGER_EVENT_TYPE, StringComparison.OrdinalIgnoreCase))
                {
                    pendingTriggers.Add(new AuthoringTriggerRow(lineNo, fields));
                    continue;
                }

                if (definitions.ContainsKey(step))
                {
                    throw new FormatException($"line {lineNo}: duplicated {STEP_COLUMN} '{step}'.");
                }

                EventDefinition definition = CreateAuthoringEventDefinition(step, type, fields, lineNo);
                definitions.Add(step, definition);
                orderedSteps.Add(step);
            }

            // 保留していたトリガーを親のテキストイベントへ追加する。
            foreach (AuthoringTriggerRow triggerRow in pendingTriggers)
            {
                int parentStep = ParseRequiredInt(
                    GetAuthoringField(triggerRow.Fields, 2),
                    PARENT_STEP_COLUMN,
                    triggerRow.LineNo);
                if (!definitions.TryGetValue(parentStep, out EventDefinition parent))
                {
                    throw new FormatException(
                        $"line {triggerRow.LineNo}: {PARENT_STEP_COLUMN} '{parentStep}' was not found.");
                }
                if (parent is not TextEventDefinition textParent)
                {
                    throw new FormatException(
                        $"line {triggerRow.LineNo}: {PARENT_STEP_COLUMN} '{parentStep}' must point Text event.");
                }

                TextTimingTrigger trigger = CreateAuthoringTrigger(triggerRow.Fields, triggerRow.LineNo, textParent.Text);
                textParent.AddTrigger(trigger);
            }

            // 記述順にイベントを並べて返す。
            var events = new List<IScenarioEvent>(orderedSteps.Count);
            foreach (int step in orderedSteps)
            {
                events.Add(definitions[step].ToEvent());
            }

            return new ScenarioDefinition(events);
        }

        /// <summary>
        /// オーサリング行からイベント定義を生成する。
        /// </summary>
        private static EventDefinition CreateAuthoringEventDefinition(int step, string type, IReadOnlyList<string> fields, int lineNo)
        {
            // 種類に応じて、3列目以降を引数として読みイベントを作る。
            switch (type.Trim().ToLowerInvariant())
            {
                case TEXT_EVENT_TYPE:
                    {
                        string speaker = GetAuthoringField(fields, 2);
                        string text = GetAuthoringField(fields, 3);
                        return new TextEventDefinition(step, speaker ?? string.Empty, text ?? string.Empty);
                    }
                case BACKGROUND_EVENT_TYPE:
                    {
                        string backgroundId = GetAuthoringField(fields, 2);
                        if (string.IsNullOrWhiteSpace(backgroundId))
                        {
                            throw new FormatException($"line {lineNo}: {BACKGROUND_ID_COLUMN} is required.");
                        }
                        return new PlainEventDefinition(step, new BackgroundEvent(CreateBackgroundId(backgroundId)));
                    }
                case ANIMATION_EVENT_TYPE:
                    {
                        string animationId = GetAuthoringField(fields, 2);
                        if (string.IsNullOrWhiteSpace(animationId))
                        {
                            throw new FormatException($"line {lineNo}: {ANIMATION_ID_COLUMN} is required.");
                        }
                        return new PlainEventDefinition(step, new AnimationEvent(CreateAnimationId(animationId)));
                    }
                case FADE_EVENT_TYPE:
                    {
                        FadeEvent fadeEvent = CreateFadeEvent(
                            GetAuthoringField(fields, 2),
                            GetAuthoringField(fields, 3),
                            GetAuthoringField(fields, 4),
                            GetAuthoringField(fields, 5),
                            GetAuthoringField(fields, 6),
                            FADE_START_COLUMN,
                            FADE_END_COLUMN,
                            FADE_DURATION_COLUMN,
                            FADE_TARGET_COLUMN,
                            FADE_MODE_COLUMN,
                            lineNo);
                        return new PlainEventDefinition(step, fadeEvent);
                    }
                case PORTRAIT_EVENT_TYPE:
                    {
                        PortraitSlot slot = ParsePortraitSlot(GetAuthoringField(fields, 2), PORTRAIT_SLOT_COLUMN, lineNo);
                        string portraitId = GetAuthoringField(fields, 3);
                        if (string.IsNullOrWhiteSpace(portraitId))
                        {
                            throw new FormatException($"line {lineNo}: {PORTRAIT_ID_COLUMN} is required.");
                        }

                        float posX = ParseOptionalFloat(GetAuthoringField(fields, 4), 0f, PORTRAIT_POS_X_COLUMN, lineNo);
                        float posY = ParseOptionalFloat(GetAuthoringField(fields, 5), 0f, PORTRAIT_POS_Y_COLUMN, lineNo);
                        float scale = ParseOptionalFloat(GetAuthoringField(fields, 6), 1f, PORTRAIT_SCALE_COLUMN, lineNo);
                        bool visible = ParseOptionalBool(GetAuthoringField(fields, 7), true, PORTRAIT_VISIBLE_COLUMN, lineNo);

                        return new PlainEventDefinition(step, new PortraitEvent(slot, CreatePortraitId(portraitId), posX, posY, scale, visible));
                    }
                case LAYER_EVENT_TYPE:
                    {
                        LayerTarget target = ParseLayerTarget(GetAuthoringField(fields, 2), LAYER_TARGET_COLUMN, lineNo);
                        int order = ParseRequiredInt(GetAuthoringField(fields, 3), LAYER_ORDER_COLUMN, lineNo);
                        return new PlainEventDefinition(step, new LayerEvent(target, order));
                    }
                default:
                    throw new FormatException($"line {lineNo}: unknown {TYPE_COLUMN} '{type}'.");
            }
        }

        /// <summary>
        /// オーサリング行からテキストトリガーを生成する。
        /// </summary>
        private static TextTimingTrigger CreateAuthoringTrigger(IReadOnlyList<string> fields, int lineNo, string text)
        {
            // 発火条件の種類と、発火させるイベントの種類は必須。
            string triggerTypeRaw = GetAuthoringField(fields, 3);
            string triggerType = triggerTypeRaw?.Trim();
            if (string.IsNullOrWhiteSpace(triggerType))
            {
                throw new FormatException($"line {lineNo}: {TRIGGER_TYPE_COLUMN} is required.");
            }

            string onTriggerTypeRaw = GetAuthoringField(fields, 6);
            string onTriggerType = onTriggerTypeRaw?.Trim();
            if (string.IsNullOrWhiteSpace(onTriggerType))
            {
                throw new FormatException($"line {lineNo}: {ON_TRIGGER_TYPE_COLUMN} is required.");
            }

            IScenarioEvent fireEvent = CreateAuthoringTriggerEvent(fields, lineNo, onTriggerType);
            // 発火条件の種類に応じてトリガーを作る。文末は本文の文字数の位置で発火させる。
            switch (triggerType.ToLowerInvariant())
            {
                case AT_CHAR_INDEX_TRIGGER_TYPE:
                    {
                        int charIndex = ParseRequiredInt(GetAuthoringField(fields, 4), TRIGGER_INDEX_COLUMN, lineNo);
                        return TextTimingTrigger.CreateAtCharIndex(charIndex, fireEvent);
                    }
                case AT_KEYWORD_TRIGGER_TYPE:
                    {
                        string keyword = GetAuthoringField(fields, 5);
                        if (string.IsNullOrWhiteSpace(keyword))
                        {
                            throw new FormatException($"line {lineNo}: {TRIGGER_KEYWORD_COLUMN} is required.");
                        }
                        return TextTimingTrigger.CreateAtKeyword(keyword, fireEvent);
                    }
                case AT_SUFFIX_TRIGGER_TYPE:
                    {
                        string suffix = GetAuthoringField(fields, 5);
                        if (string.IsNullOrWhiteSpace(suffix))
                        {
                            throw new FormatException($"line {lineNo}: {TRIGGER_KEYWORD_COLUMN} is required.");
                        }
                        return TextTimingTrigger.CreateAtSuffix(suffix, fireEvent);
                    }
                case AT_TEXT_END_TRIGGER_TYPE:
                    {
                        int charIndex = string.IsNullOrEmpty(text) ? 0 : text.Length;
                        return TextTimingTrigger.CreateAtCharIndex(charIndex, fireEvent);
                    }
                default:
                    throw new FormatException($"line {lineNo}: unknown {TRIGGER_TYPE_COLUMN} '{triggerTypeRaw}'.");
            }
        }

        /// <summary>
        /// オーサリングトリガー設定から発火イベントを生成する。
        /// </summary>
        private static IScenarioEvent CreateAuthoringTriggerEvent(IReadOnlyList<string> fields, int lineNo, string onTriggerType)
        {
            // 種類に応じて、8列目以降を引数として読みイベントを作る。
            switch (onTriggerType.ToLowerInvariant())
            {
                case FADE_EVENT_TYPE:
                    {
                        return CreateFadeEvent(
                            GetAuthoringField(fields, 7),
                            GetAuthoringField(fields, 8),
                            GetAuthoringField(fields, 9),
                            GetAuthoringField(fields, 10),
                            GetAuthoringField(fields, 11),
                            ON_TRIGGER_ARG_1_COLUMN,
                            ON_TRIGGER_ARG_2_COLUMN,
                            ON_TRIGGER_ARG_3_COLUMN,
                            ON_TRIGGER_ARG_4_COLUMN,
                            ON_TRIGGER_ARG_5_COLUMN,
                            lineNo);
                    }
                case BACKGROUND_EVENT_TYPE:
                    {
                        string backgroundId = GetAuthoringField(fields, 7);
                        if (string.IsNullOrWhiteSpace(backgroundId))
                        {
                            throw new FormatException($"line {lineNo}: {ON_TRIGGER_ARG_1_COLUMN} is required for Background.");
                        }
                        return new BackgroundEvent(CreateBackgroundId(backgroundId));
                    }
                case ANIMATION_EVENT_TYPE:
                    {
                        string animationId = GetAuthoringField(fields, 7);
                        if (string.IsNullOrWhiteSpace(animationId))
                        {
                            throw new FormatException($"line {lineNo}: {ON_TRIGGER_ARG_1_COLUMN} is required for Animation.");
                        }
                        return new AnimationEvent(CreateAnimationId(animationId));
                    }
                case PORTRAIT_EVENT_TYPE:
                    {
                        PortraitSlot slot = ParsePortraitSlot(GetAuthoringField(fields, 7), ON_TRIGGER_ARG_1_COLUMN, lineNo);
                        string portraitId = GetAuthoringField(fields, 8);
                        if (string.IsNullOrWhiteSpace(portraitId))
                        {
                            throw new FormatException($"line {lineNo}: {ON_TRIGGER_ARG_2_COLUMN} is required for Portrait.");
                        }
                        float posX = ParseOptionalFloat(GetAuthoringField(fields, 9), 0f, ON_TRIGGER_ARG_3_COLUMN, lineNo);
                        return new PortraitEvent(slot, CreatePortraitId(portraitId), posX, 0f, 1f, true);
                    }
                case LAYER_EVENT_TYPE:
                    {
                        LayerTarget target = ParseLayerTarget(GetAuthoringField(fields, 7), ON_TRIGGER_ARG_1_COLUMN, lineNo);
                        int order = ParseRequiredInt(GetAuthoringField(fields, 8), ON_TRIGGER_ARG_2_COLUMN, lineNo);
                        return new LayerEvent(target, order);
                    }
                default:
                    throw new FormatException($"line {lineNo}: unknown {ON_TRIGGER_TYPE_COLUMN} '{onTriggerType}'.");
            }
        }

        /// <summary>
        /// オーサリング行から指定位置の値を安全に取得する。
        /// </summary>
        private static string GetAuthoringField(IReadOnlyList<string> fields, int index)
        {
            return ScenarioCsvUtility.GetField(fields, index);
        }

        /// <summary>
        /// シナリオファイルの内容を行配列として読み込む。
        /// </summary>
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

        /// <summary>
        /// コメントや空行を除いた先頭データ行を取得する。
        /// </summary>
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

        /// <summary>
        /// ローカルまたは URL のファイルを全行読み込む。
        /// </summary>
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
            using var ctr = ct.Register(s => ((UnityWebRequest)s).Abort(), request);
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

        /// <summary>
        /// ヘッダー名から列番号を引ける辞書を構築する。
        /// </summary>
        private static Dictionary<string, int> BuildHeaderIndex(IReadOnlyList<string> headers)
        {
            return ScenarioCsvUtility.BuildHeaderIndex(headers);
        }

        /// <summary>
        /// 正規化 CSV の行からイベント定義を生成する。
        /// </summary>
        private static EventDefinition CreateEventDefinition(
            EventRow row,
            IReadOnlyDictionary<string, int> headerIndex)
        {
            IReadOnlyList<string> values = row.Values;
            switch (row.Type.Trim().ToLowerInvariant())
            {
                case TEXT_EVENT_TYPE:
                    {
                        string speaker = GetValue(values, headerIndex, SPEAKER_COLUMN);
                        string text = GetValue(values, headerIndex, TEXT_COLUMN);
                        var def = new TextEventDefinition(row.Step, speaker ?? string.Empty, text ?? string.Empty);

                        // 後方互換: Event 行にもトリガー情報を直接持てるようにしている
                        TextTimingTrigger inlineTrigger = TryCreateTrigger(values, headerIndex, row.LineNo, text);
                        if (inlineTrigger != null)
                        {
                            def.AddTrigger(inlineTrigger);
                        }

                        return def;
                    }
                case BACKGROUND_EVENT_TYPE:
                    {
                        string backgroundId = GetValue(values, headerIndex, BACKGROUND_ID_COLUMN);
                        if (string.IsNullOrWhiteSpace(backgroundId))
                        {
                            throw new FormatException(
                                $"line {row.LineNo}: {BACKGROUND_ID_COLUMN} is required for Background event.");
                        }
                        return new PlainEventDefinition(row.Step, new BackgroundEvent(CreateBackgroundId(backgroundId)));
                    }
                case ANIMATION_EVENT_TYPE:
                    {
                        string animationId = GetValue(values, headerIndex, ANIMATION_ID_COLUMN);
                        if (string.IsNullOrWhiteSpace(animationId))
                        {
                            throw new FormatException(
                                $"line {row.LineNo}: {ANIMATION_ID_COLUMN} is required for Animation event.");
                        }
                        return new PlainEventDefinition(row.Step, new AnimationEvent(CreateAnimationId(animationId)));
                    }
                case FADE_EVENT_TYPE:
                    {
                        FadeEvent fadeEvent = CreateFadeEvent(
                            GetValue(values, headerIndex, FADE_START_COLUMN),
                            GetValue(values, headerIndex, FADE_END_COLUMN),
                            GetValue(values, headerIndex, FADE_DURATION_COLUMN),
                            GetValue(values, headerIndex, FADE_TARGET_COLUMN),
                            GetValue(values, headerIndex, FADE_MODE_COLUMN),
                            FADE_START_COLUMN,
                            FADE_END_COLUMN,
                            FADE_DURATION_COLUMN,
                            FADE_TARGET_COLUMN,
                            FADE_MODE_COLUMN,
                            row.LineNo);
                        return new PlainEventDefinition(row.Step, fadeEvent);
                    }
                case PORTRAIT_EVENT_TYPE:
                    {
                        PortraitSlot slot = ParsePortraitSlot(
                            GetValue(values, headerIndex, PORTRAIT_SLOT_COLUMN),
                            PORTRAIT_SLOT_COLUMN,
                            row.LineNo);
                        string portraitId = GetValue(values, headerIndex, PORTRAIT_ID_COLUMN);
                        if (string.IsNullOrWhiteSpace(portraitId))
                        {
                            throw new FormatException(
                                $"line {row.LineNo}: {PORTRAIT_ID_COLUMN} is required for Portrait event.");
                        }
                        float posX = ParseOptionalFloat(GetValue(values, headerIndex, PORTRAIT_POS_X_COLUMN), 0f, PORTRAIT_POS_X_COLUMN, row.LineNo);
                        float posY = ParseOptionalFloat(GetValue(values, headerIndex, PORTRAIT_POS_Y_COLUMN), 0f, PORTRAIT_POS_Y_COLUMN, row.LineNo);
                        float scale = ParseOptionalFloat(GetValue(values, headerIndex, PORTRAIT_SCALE_COLUMN), 1f, PORTRAIT_SCALE_COLUMN, row.LineNo);
                        bool visible = ParseOptionalBool(GetValue(values, headerIndex, PORTRAIT_VISIBLE_COLUMN), true, PORTRAIT_VISIBLE_COLUMN, row.LineNo);

                        return new PlainEventDefinition(
                            row.Step,
                            new PortraitEvent(slot, CreatePortraitId(portraitId), posX, posY, scale, visible));
                    }
                case LAYER_EVENT_TYPE:
                    {
                        LayerTarget target = ParseLayerTarget(
                            GetValue(values, headerIndex, LAYER_TARGET_COLUMN),
                            LAYER_TARGET_COLUMN,
                            row.LineNo);
                        int order = ParseRequiredInt(
                            GetValue(values, headerIndex, LAYER_ORDER_COLUMN),
                            LAYER_ORDER_COLUMN,
                            row.LineNo);
                        return new PlainEventDefinition(row.Step, new LayerEvent(target, order));
                    }
                default:
                    throw new FormatException($"line {row.LineNo}: unknown {TYPE_COLUMN} '{row.Type}'.");
            }
        }

        /// <summary>
        /// トリガー列が有効な場合のみテキストトリガーを生成する。
        /// </summary>
        private static TextTimingTrigger TryCreateTrigger(
            IReadOnlyList<string> values,
            IReadOnlyDictionary<string, int> headerIndex,
            int lineNo,
            string text)
        {
            string triggerTypeRaw = GetValue(values, headerIndex, TRIGGER_TYPE_COLUMN);
            string triggerType = triggerTypeRaw?.Trim();
            if (string.IsNullOrWhiteSpace(triggerType) ||
                triggerType.Equals(NONE_TRIGGER_TYPE, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return CreateTrigger(values, headerIndex, lineNo, text);
        }

        /// <summary>
        /// トリガー設定からテキストトリガーを生成する。
        /// </summary>
        private static TextTimingTrigger CreateTrigger(
            IReadOnlyList<string> values,
            IReadOnlyDictionary<string, int> headerIndex,
            int lineNo,
            string text)
        {
            // 発火させるイベントの種類は必須。
            string onTriggerTypeRaw = GetValue(values, headerIndex, ON_TRIGGER_TYPE_COLUMN);
            string onTriggerType = onTriggerTypeRaw?.Trim();
            if (string.IsNullOrWhiteSpace(onTriggerType))
            {
                throw new FormatException(
                    $"line {lineNo}: {ON_TRIGGER_TYPE_COLUMN} is required when {TRIGGER_TYPE_COLUMN} is set.");
            }

            IScenarioEvent fireEvent = CreateTriggerEvent(values, headerIndex, lineNo, onTriggerType);
            // 発火条件の種類に応じてトリガーを作る。文末は本文の文字数の位置で発火させる。
            string triggerTypeRaw = GetValue(values, headerIndex, TRIGGER_TYPE_COLUMN);
            string triggerType = triggerTypeRaw?.Trim();

            switch (triggerType.ToLowerInvariant())
            {
                case AT_CHAR_INDEX_TRIGGER_TYPE:
                    {
                        string indexRaw = GetValue(values, headerIndex, TRIGGER_INDEX_COLUMN);
                        if (!int.TryParse(indexRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int charIndex))
                        {
                            throw new FormatException(
                                $"line {lineNo}: {TRIGGER_INDEX_COLUMN} must be int for AtCharIndex.");
                        }
                        return TextTimingTrigger.CreateAtCharIndex(charIndex, fireEvent);
                    }
                case AT_KEYWORD_TRIGGER_TYPE:
                    {
                        string keyword = GetValue(values, headerIndex, TRIGGER_KEYWORD_COLUMN);
                        if (string.IsNullOrWhiteSpace(keyword))
                        {
                            throw new FormatException(
                                $"line {lineNo}: {TRIGGER_KEYWORD_COLUMN} is required for AtKeyword.");
                        }
                        return TextTimingTrigger.CreateAtKeyword(keyword, fireEvent);
                    }
                case AT_SUFFIX_TRIGGER_TYPE:
                    {
                        string suffix = GetValue(values, headerIndex, TRIGGER_KEYWORD_COLUMN);
                        if (string.IsNullOrWhiteSpace(suffix))
                        {
                            throw new FormatException(
                                $"line {lineNo}: {TRIGGER_KEYWORD_COLUMN} is required for AtSuffix.");
                        }
                        return TextTimingTrigger.CreateAtSuffix(suffix, fireEvent);
                    }
                case AT_TEXT_END_TRIGGER_TYPE:
                    {
                        int charIndex = string.IsNullOrEmpty(text) ? 0 : text.Length;
                        return TextTimingTrigger.CreateAtCharIndex(charIndex, fireEvent);
                    }
                default:
                    throw new FormatException($"line {lineNo}: unknown {TRIGGER_TYPE_COLUMN} '{triggerTypeRaw}'.");
            }
        }

        /// <summary>
        /// トリガー設定から発火イベントを生成する。
        /// </summary>
        private static IScenarioEvent CreateTriggerEvent(
            IReadOnlyList<string> values,
            IReadOnlyDictionary<string, int> headerIndex,
            int lineNo,
            string onTriggerType)
        {
            // 種類に応じて、引数の列を読みイベントを作る。
            switch (onTriggerType.ToLowerInvariant())
            {
                case FADE_EVENT_TYPE:
                    {
                        return CreateFadeEvent(
                            GetValue(values, headerIndex, ON_TRIGGER_ARG_1_COLUMN),
                            GetValue(values, headerIndex, ON_TRIGGER_ARG_2_COLUMN),
                            GetValue(values, headerIndex, ON_TRIGGER_ARG_3_COLUMN),
                            GetValue(values, headerIndex, ON_TRIGGER_ARG_4_COLUMN),
                            GetValue(values, headerIndex, ON_TRIGGER_ARG_5_COLUMN),
                            ON_TRIGGER_ARG_1_COLUMN,
                            ON_TRIGGER_ARG_2_COLUMN,
                            ON_TRIGGER_ARG_3_COLUMN,
                            ON_TRIGGER_ARG_4_COLUMN,
                            ON_TRIGGER_ARG_5_COLUMN,
                            lineNo);
                    }
                case BACKGROUND_EVENT_TYPE:
                    {
                        string backgroundId = GetValue(values, headerIndex, ON_TRIGGER_ARG_1_COLUMN);
                        if (string.IsNullOrWhiteSpace(backgroundId))
                        {
                            throw new FormatException(
                                $"line {lineNo}: {ON_TRIGGER_ARG_1_COLUMN} is required "
                                + $"for {ON_TRIGGER_TYPE_COLUMN}=Background.");
                        }
                        return new BackgroundEvent(CreateBackgroundId(backgroundId));
                    }
                case ANIMATION_EVENT_TYPE:
                    {
                        string animationId = GetValue(values, headerIndex, ON_TRIGGER_ARG_1_COLUMN);
                        if (string.IsNullOrWhiteSpace(animationId))
                        {
                            throw new FormatException(
                                $"line {lineNo}: {ON_TRIGGER_ARG_1_COLUMN} is required "
                                + $"for {ON_TRIGGER_TYPE_COLUMN}=Animation.");
                        }
                        return new AnimationEvent(CreateAnimationId(animationId));
                    }
                case PORTRAIT_EVENT_TYPE:
                    {
                        PortraitSlot slot = ParsePortraitSlot(
                            GetValue(values, headerIndex, ON_TRIGGER_ARG_1_COLUMN),
                            ON_TRIGGER_ARG_1_COLUMN,
                            lineNo);
                        string portraitId = GetValue(values, headerIndex, ON_TRIGGER_ARG_2_COLUMN);
                        if (string.IsNullOrWhiteSpace(portraitId))
                        {
                            throw new FormatException(
                                $"line {lineNo}: {ON_TRIGGER_ARG_2_COLUMN} is required "
                                + $"for {ON_TRIGGER_TYPE_COLUMN}=Portrait.");
                        }
                        float posX = ParseOptionalFloat(
                            GetValue(values, headerIndex, ON_TRIGGER_ARG_3_COLUMN),
                            0f,
                            ON_TRIGGER_ARG_3_COLUMN,
                            lineNo);
                        return new PortraitEvent(slot, CreatePortraitId(portraitId), posX, 0f, 1f, true);
                    }
                case LAYER_EVENT_TYPE:
                    {
                        LayerTarget target = ParseLayerTarget(
                            GetValue(values, headerIndex, ON_TRIGGER_ARG_1_COLUMN),
                            ON_TRIGGER_ARG_1_COLUMN,
                            lineNo);
                        int order = ParseRequiredInt(
                            GetValue(values, headerIndex, ON_TRIGGER_ARG_2_COLUMN),
                            ON_TRIGGER_ARG_2_COLUMN,
                            lineNo);
                        return new LayerEvent(target, order);
                    }
                default:
                    throw new FormatException($"line {lineNo}: unknown {ON_TRIGGER_TYPE_COLUMN} '{onTriggerType}'.");
            }
        }

        /// <summary>
        /// 列名に対応する値を取得する。
        /// </summary>
        private static string GetValue(
            IReadOnlyList<string> values,
            IReadOnlyDictionary<string, int> headerIndex,
            string key)
        {
            return ScenarioCsvUtility.GetValue(values, headerIndex, key);
        }

        /// <summary>
        /// 必須の小数値を解析する。
        /// </summary>
        private static float ParseRequiredFloat(string raw, string columnName, int lineNo)
        {
            return ScenarioCsvUtility.ParseRequiredFloat(raw, columnName, lineNo);
        }

        /// <summary>
        /// 必須の整数値を解析する。
        /// </summary>
        private static int ParseRequiredInt(string raw, string columnName, int lineNo)
        {
            return ScenarioCsvUtility.ParseRequiredInt(raw, columnName, lineNo);
        }

        /// <summary>
        /// 任意の整数値を既定値付きで解析する。
        /// </summary>
        private static int ParseOptionalInt(string raw, int defaultValue, string columnName, int lineNo)
        {
            return ScenarioCsvUtility.ParseOptionalInt(raw, defaultValue, columnName, lineNo);
        }

        /// <summary>
        /// 任意の小数値を既定値付きで解析する。
        /// </summary>
        private static float ParseOptionalFloat(string raw, float defaultValue, string columnName, int lineNo)
        {
            return ScenarioCsvUtility.ParseOptionalFloat(raw, defaultValue, columnName, lineNo);
        }

        /// <summary>
        /// 任意の真偽値を既定値付きで解析する。
        /// </summary>
        private static bool ParseOptionalBool(string raw, bool defaultValue, string columnName, int lineNo)
        {
            return ScenarioCsvUtility.ParseOptionalBool(raw, defaultValue, columnName, lineNo);
        }

        /// <summary>
        ///     CSVの文字列IDを背景IDへ変換します。
        /// </summary>
        /// <param name="id"> CSVへ記述された文字列IDです。 </param>
        /// <returns> 実行時用の背景IDです。 </returns>
        private static BackgroundId CreateBackgroundId(string id)
        {
            return new BackgroundId(DataIDHasher.Compute("ScenarioBackground", id));
        }

        /// <summary>
        ///     CSVの文字列IDをアニメーションIDへ変換します。
        /// </summary>
        /// <param name="id"> CSVへ記述された文字列IDです。 </param>
        /// <returns> 実行時用のアニメーションIDです。 </returns>
        private static AnimationId CreateAnimationId(string id)
        {
            return new AnimationId(DataIDHasher.Compute("ScenarioAnimation", id));
        }

        /// <summary>
        ///     CSVの文字列IDを立ち絵IDへ変換します。
        /// </summary>
        /// <param name="id"> CSVへ記述された文字列IDです。 </param>
        /// <returns> 実行時用の立ち絵IDです。 </returns>
        private static PortraitId CreatePortraitId(string id)
        {
            return new PortraitId(DataIDHasher.Compute("ScenarioPortrait", id));
        }

        /// <summary>
        /// 文字列から立ち絵スロットを解析する。
        /// </summary>
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

        /// <summary>
        /// 文字列からレイヤー対象を解析する。
        /// </summary>
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

        /// <summary>
        /// CSV の各列から検証済みのフェードイベントを生成する。
        /// </summary>
        private static FadeEvent CreateFadeEvent(
            string startRaw,
            string endRaw,
            string durationRaw,
            string targetRaw,
            string modeRaw,
            string startColumn,
            string endColumn,
            string durationColumn,
            string targetColumn,
            string modeColumn,
            int lineNo)
        {
            float start = ParseRequiredFiniteFloat(startRaw, startColumn, lineNo);
            float end = ParseRequiredFiniteFloat(endRaw, endColumn, lineNo);
            float duration = ParseRequiredFiniteFloat(durationRaw, durationColumn, lineNo);
            FadeTarget target = ParseFadeTarget(targetRaw, targetColumn, lineNo);
            FadeMode mode = ParseFadeMode(modeRaw, modeColumn, lineNo);

            try
            {
                return new FadeEvent(start, end, duration, target, mode);
            }
            catch (ArgumentException exception)
            {
                throw new FormatException(
                    $"line {lineNo}: invalid {modeColumn} '{modeRaw}' for {targetColumn} '{targetRaw}'.",
                    exception);
            }
        }

        /// <summary>
        /// フェードに使用する有限小数を解析する。
        /// </summary>
        private static float ParseRequiredFiniteFloat(string raw, string columnName, int lineNo)
        {
            float value = ParseRequiredFloat(raw, columnName, lineNo);
            if (!float.IsFinite(value))
            {
                throw new FormatException($"line {lineNo}: invalid {columnName} '{raw}'.");
            }

            return value;
        }

        /// <summary>
        /// 文字列からフェード対象を解析する。省略時は画面全体（Screen）。
        /// </summary>
        private static FadeTarget ParseFadeTarget(string raw, string columnName, int lineNo)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return FadeTarget.Screen;
            }

            string value = raw.Trim();
            if (value.Equals("Screen", StringComparison.OrdinalIgnoreCase)
                || value.Equals("Canvas", StringComparison.OrdinalIgnoreCase)
                || value.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                return FadeTarget.Screen;
            }

            if (!Enum.TryParse(value, true, out FadeTarget target)
                || !Enum.IsDefined(typeof(FadeTarget), target))
            {
                throw new FormatException($"line {lineNo}: unknown {columnName} '{raw}'.");
            }

            return target;
        }

        /// <summary>
        /// 文字列からフェード方法を解析する。省略時は透明度（Alpha）。
        /// </summary>
        private static FadeMode ParseFadeMode(string raw, string columnName, int lineNo)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return FadeMode.Alpha;
            }

            if (!Enum.TryParse(raw.Trim(), true, out FadeMode mode)
                || !Enum.IsDefined(typeof(FadeMode), mode))
            {
                throw new FormatException($"line {lineNo}: unknown {columnName} '{raw}'.");
            }

            return mode;
        }

        /// <summary>
        /// CSV 1 行を列配列へ分解する。
        /// </summary>
        private static List<string> ParseCsvLine(string line)
        {
            return ScenarioCsvUtility.ParseCsvLine(line);
        }

    }

}
