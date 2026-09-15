using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Composition.InGame.Player;
using KillChord.Runtime.Domain.InGame.Music;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace KillChord.Editor.AIDebugPlay
{
    /// <summary>
    ///     AIデバッグプレイ向けに、指定された拍種のジャスト攻撃をEditor上で予約実行するクラス。
    /// </summary>
    [InitializeOnLoad]
    public static class AIDebugAttackQueue
    {
        static AIDebugAttackQueue()
        {
            EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
            AssemblyReloadEvents.beforeAssemblyReload += HandleBeforeAssemblyReload;
        }

        /// <summary>
        ///     カンマ区切りの攻撃指定をキューへ登録する。
        /// </summary>
        /// <param name="specification"> 「green:4,Eight:8」形式の攻撃指定。 </param>
        /// <param name="prime"> 最初に基準時刻を作る通常攻撃を行う場合はtrue。 </param>
        /// <param name="runId"> 再送を識別するUUID。省略時は新規発行する。 </param>
        /// <param name="timeoutSeconds"> 一時停止を含めた待機期限の秒数。 </param>
        /// <returns> 登録後の状態を表すJSON。 </returns>
        public static string Enqueue(string specification, bool prime = true, string runId = null, double timeoutSeconds = 120d)
        {
            if (runId != null && !Guid.TryParse(runId, out _))
            {
                return CreateErrorJson("runIdにはUUIDを指定してください。");
            }
            if (runId != null && runId == _runId) { return GetStatusJson(); }
            if (_state == QueueState.Waiting) { return CreateErrorJson("別の攻撃キューを実行中です。"); }
            if (double.IsNaN(timeoutSeconds) || double.IsInfinity(timeoutSeconds)
                || timeoutSeconds < 1d || timeoutSeconds > MAX_TIMEOUT_SECONDS)
            {
                return CreateErrorJson("timeoutSecondsは1〜3600秒で指定してください。");
            }
            if (!EditorApplication.isPlaying)
            {
                return CreateErrorJson("Play Modeで実行してください。");
            }

            if (!TryParseSpecification(specification, out Queue<BeatType> parsedQueue, out string error))
            {
                return CreateErrorJson(error);
            }

            CancelInternal("新しいキューで置き換えました。");

            if (!TryResolveDependencies(out error))
            {
                return CreateErrorJson(error);
            }

            _runId = runId ?? Guid.NewGuid().ToString();
            _deadline = EditorApplication.timeSinceStartup + timeoutSeconds;

            while (parsedQueue.Count > 0)
            {
                _pendingAttacks.Enqueue(parsedQueue.Dequeue());
            }

            _requestedCount = _pendingAttacks.Count;
            _completedCount = 0;
            _isPriming = prime;
            _state = QueueState.Waiting;
            _lastMessage = prime
                ? "基準攻撃の実行待ちです。"
                : "ジャスト攻撃の実行待ちです。";

            _playerModule.PlayerAttackSignal.OnAttackExecuted += HandleAttackBeatExecuted;
            EditorApplication.update += Update;

            if (!prime)
            {
                PrepareNextAttack();
            }

            return GetStatusJson();
        }

        /// <summary>
        ///     現在の攻撃キュー状態をJSONで取得する。
        /// </summary>
        /// <returns> 攻撃キュー状態を表すJSON。 </returns>
        public static string GetStatusJson()
        {
            return AIDebugJson.Serialize(AIDebugJson.Object(
                ("success", _state != QueueState.Failed), ("runId", _runId), ("state", _state.ToString()),
                ("requested", _requestedCount), ("completed", _completedCount),
                ("remaining", _pendingAttacks.Count + (_hasCurrentTarget ? 1 : 0)),
                ("currentBeatType", _hasCurrentTarget ? _currentTarget.ToString() : null),
                ("priming", _isPriming), ("message", _lastMessage)));
        }

        /// <summary>
        ///     実行中の攻撃キューをキャンセルする。
        /// </summary>
        /// <param name="runId"> 所有するキューのUUID。省略は従来の手動操作用。 </param>
        /// <returns> キャンセル後の状態を表すJSON。 </returns>
        public static string Cancel(string runId = null)
        {
            if (runId != null && runId != _runId) { return CreateErrorJson("runIdが現在のキューと一致しません。"); }
            CancelInternal("ユーザー操作によりキャンセルしました。");
            return GetStatusJson();
        }

        private const double ATTACK_RESULT_TIMEOUT_SECONDS = 2d;
        private const int MAX_ATTACK_COUNT_PER_ENTRY = 1000;
        private const int MAX_TOTAL_ATTACK_COUNT = 1000;
        private const int MAX_SPECIFICATION_LENGTH = 16384;
        private const double MAX_TIMEOUT_SECONDS = 3600d;

        private static readonly Queue<BeatType> _pendingAttacks = new();

        private static PlayerModuleContainer _playerModule;
        private static MusicSyncService _musicSyncService;
        private static RhythmJudgmentRange _currentRange;
        private static BeatType _currentTarget;
        private static QueueState _state = QueueState.Idle;
        private static string _lastMessage = "未実行です。";
        private static double _attackRequestedAt;
        private static int _pressFrame = -1;
        private static int _requestedCount;
        private static int _completedCount;
        private static bool _hasCurrentTarget;
        private static bool _isPriming;
        private static bool _isPressHeld;
        private static bool _isAwaitingAttackResult;
        private static bool _shouldAdvanceAfterRelease;
        private static bool _shouldFinishAfterRelease;
        private static string _runId;
        private static double _deadline;
        private static Mouse _pressedMouse;

        /// <summary>
        ///     Editor更新ごとに予約時刻と入力状態を処理する。
        /// </summary>
        private static void Update()
        {
            try { Tick(); }
            catch (Exception exception) { Fail("攻撃キューの実行に失敗しました: " + exception.Message); }
        }

        /// <summary>
        ///     所有するキューの期限と、現在のシーンに属するサービスを確認して進める。
        /// </summary>
        private static void Tick()
        {
            if (!EditorApplication.isPlaying)
            {
                CancelInternal("Play Modeが終了しました。");
                return;
            }

            if (EditorApplication.timeSinceStartup >= _deadline)
            {
                Fail("攻撃キュー全体の待機期限を超えました。");
                return;
            }
            if (!ServiceLocator.TryGetInstance<PlayerModuleContainer>(out var currentPlayer)
                || !ReferenceEquals(currentPlayer, _playerModule)
                || !ServiceLocator.TryGetInstance<IMusicSyncService>(out var currentMusic)
                || !ReferenceEquals(currentMusic, _musicSyncService))
            {
                Fail("シーン遷移またはサービスの破棄により停止しました。");
                return;
            }
            if (EditorApplication.isPaused)
            {
                return;
            }

            if (_isPressHeld && Time.frameCount > _pressFrame)
            {
                ReleaseAttackInput();
            }

            if (_isPressHeld)
            {
                return;
            }

            if (_shouldFinishAfterRelease)
            {
                Complete();
                return;
            }

            if (_shouldAdvanceAfterRelease)
            {
                _shouldAdvanceAfterRelease = false;
                PrepareNextAttack();
            }

            if (_isAwaitingAttackResult)
            {
                if (EditorApplication.timeSinceStartup - _attackRequestedAt > ATTACK_RESULT_TIMEOUT_SECONDS)
                {
                    Fail("攻撃入力に対する成立通知がタイムアウトしました。入力抑制またはゲームプレイ停止状態を確認してください。");
                }

                return;
            }

            if (_isPriming)
            {
                if (CanInjectAttack())
                {
                    InjectAttackInput();
                    if (_state == QueueState.Waiting && _isAwaitingAttackResult)
                    {
                        _lastMessage = "基準攻撃を入力しました。";
                    }
                }

                return;
            }

            if (!_hasCurrentTarget)
            {
                return;
            }

            float progress = _musicSyncService.GetBarProgressUnclamped();
            if (progress >= _currentRange.JustEndNormalized)
            {
                Fail($"{_currentTarget}のジャスト範囲を通過しました。progress={progress:F4}");
                return;
            }

            float targetProgress = (_currentRange.JustStartNormalized + _currentRange.JustEndNormalized) * 0.5f;
            if (progress < targetProgress || !CanInjectAttack())
            {
                return;
            }

            BeatType resolvedBeatType = _musicSyncService.GetCurrentBeatType(out bool isJustHit);
            if (!isJustHit || resolvedBeatType != _currentTarget)
            {
                Fail($"予約位置で期待した判定を取得できませんでした。expected={_currentTarget}, actual={resolvedBeatType}, just={isJustHit}, progress={progress:F4}");
                return;
            }

            InjectAttackInput();
            if (_state == QueueState.Waiting && _isAwaitingAttackResult)
            {
                _lastMessage = $"{_currentTarget}のジャスト攻撃を入力しました。";
            }
        }

        /// <summary>
        ///     攻撃成立通知を受け取り、予約した拍種と照合する。
        /// </summary>
        /// <param name="beatCount"> 実際に成立した拍種の値。 </param>
        /// <param name="isJustHit"> 実際の攻撃に適用されたJust判定。 </param>
        private static void HandleAttackBeatExecuted(int beatCount, bool isJustHit)
        {
            BeatType actualBeatType = (BeatType)beatCount;
            if (!_isAwaitingAttackResult)
            {
                return;
            }

            _isAwaitingAttackResult = false;

            if (_isPriming)
            {
                _isPriming = false;
                _lastMessage = $"基準攻撃が成立しました。actual={actualBeatType}";
                _shouldAdvanceAfterRelease = true;
                return;
            }

            if (!_hasCurrentTarget || actualBeatType != _currentTarget || !isJustHit)
            {
                Fail($"成立した判定が予約と一致しません。expected={_currentTarget}, actual={actualBeatType}, just={isJustHit}");
                return;
            }

            _completedCount++;
            _lastMessage = $"{actualBeatType}のジャスト攻撃が成立しました（{_completedCount}/{_requestedCount}）。";
            _hasCurrentTarget = false;

            if (_pendingAttacks.Count == 0)
            {
                _shouldFinishAfterRelease = true;
                return;
            }

            _shouldAdvanceAfterRelease = true;
        }

        /// <summary>
        ///     次の予約攻撃と対応するジャスト判定範囲を準備する。
        /// </summary>
        private static void PrepareNextAttack()
        {
            if (_pendingAttacks.Count == 0)
            {
                Complete();
                return;
            }

            _currentTarget = _pendingAttacks.Dequeue();
            if (!TryFindJudgmentRange(_currentTarget, out _currentRange))
            {
                Fail($"{_currentTarget}のリズム判定範囲が見つかりませんでした。");
                return;
            }

            _hasCurrentTarget = true;
            _lastMessage = $"{_currentTarget}のジャスト範囲を待機しています。";
        }

        /// <summary>
        ///     攻撃入力を注入できる状態か判定する。
        /// </summary>
        /// <returns> 入力可能な場合はtrue。 </returns>
        private static bool CanInjectAttack()
        {
            if (_playerModule?.PlayerAttackController == null
                || _playerModule.PlayerAttackController.IsAttacking
                || _playerModule.PlayerAttackController.IsAttackCooldown)
            {
                return false;
            }

            return _playerModule.InputSuppressionState == null
                || !_playerModule.InputSuppressionState.IsSuppressed;
        }

        /// <summary>
        ///     Input Systemへマウス左ボタンの押下を登録する。
        /// </summary>
        private static void InjectAttackInput()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null)
            {
                Fail("Input SystemにMouseデバイスが存在しません。");
                return;
            }

            if (mouse.leftButton.isPressed)
            {
                Fail("マウス左ボタンが既に押されています。実デバイス入力を解除して再実行してください。");
                return;
            }

            _pressFrame = Time.frameCount;
            _attackRequestedAt = EditorApplication.timeSinceStartup;
            _isPressHeld = true;
            _isAwaitingAttackResult = true;
            _pressedMouse = mouse;
            // InputState.Changeから成立通知が同期発火しても受け取れるよう、先に待機状態にする。
            QueueMouseButtonState(mouse, true);
        }

        /// <summary>
        ///     Input Systemへマウス左ボタンの解放を登録する。
        /// </summary>
        private static void ReleaseAttackInput()
        {
            Mouse mouse = _pressedMouse;
            bool wasHeld = _isPressHeld;
            _isPressHeld = false;
            _pressFrame = -1;
            _pressedMouse = null;
            if (wasHeld && mouse != null && mouse.added)
            {
                QueueMouseButtonState(mouse, false);
            }
        }

        /// <summary>
        ///     現在のポインター状態を維持したまま、左ボタンの状態を入力キューへ登録する。
        /// </summary>
        /// <param name="mouse"> 入力対象のMouseデバイス。 </param>
        /// <param name="pressed"> 押下状態にする場合はtrue。 </param>
        private static void QueueMouseButtonState(Mouse mouse, bool pressed)
        {
            using (StateEvent.From(mouse, out InputEventPtr eventPtr))
            {
                mouse.leftButton.WriteValueIntoEvent(pressed ? 1f : 0f, eventPtr);
                InputState.Change(mouse, eventPtr, InputUpdateType.Dynamic);
            }
        }

        /// <summary>
        ///     サービスロケーターから実行に必要な依存を取得する。
        /// </summary>
        /// <param name="error"> 取得失敗時の理由。 </param>
        /// <returns> 全て取得できた場合はtrue。 </returns>
        private static bool TryResolveDependencies(out string error)
        {
            if (!ServiceLocator.TryGetInstance(out _playerModule)
                || _playerModule?.PlayerAttackController == null || _playerModule.PlayerAttackSignal == null)
            {
                error = "PlayerModuleContainerまたはPlayerAttackControllerが初期化されていません。";
                return false;
            }

            if (!ServiceLocator.TryGetInstance<IMusicSyncService>(out IMusicSyncService musicSyncService)
                || musicSyncService is not MusicSyncService concreteService)
            {
                error = "MusicSyncServiceが初期化されていません。";
                return false;
            }

            _musicSyncService = concreteService;
            error = string.Empty;
            return true;
        }

        /// <summary>
        ///     指定拍種に対応するジャスト判定範囲を検索する。
        /// </summary>
        /// <param name="beatType"> 検索する拍種。 </param>
        /// <param name="range"> 見つかった判定範囲。 </param>
        /// <returns> 判定範囲が見つかった場合はtrue。 </returns>
        private static bool TryFindJudgmentRange(BeatType beatType, out RhythmJudgmentRange range)
        {
            IReadOnlyList<RhythmJudgmentRange> ranges = _musicSyncService.RhythmJudgmentDefinition.JudgmentRanges;
            for (int i = 0; i < ranges.Count; i++)
            {
                if (ranges[i].BeatType == beatType)
                {
                    range = ranges[i];
                    return true;
                }
            }

            range = default;
            return false;
        }

        /// <summary>
        ///     外部向けの攻撃指定文字列を拍種キューへ変換する。
        /// </summary>
        /// <param name="specification"> 変換対象の文字列。 </param>
        /// <param name="queue"> 変換後の拍種キュー。 </param>
        /// <param name="error"> 変換失敗時の理由。 </param>
        /// <returns> 変換できた場合はtrue。 </returns>
        private static bool TryParseSpecification(
            string specification,
            out Queue<BeatType> queue,
            out string error)
        {
            queue = new Queue<BeatType>();
            if (string.IsNullOrWhiteSpace(specification) || specification.Length > MAX_SPECIFICATION_LENGTH)
            {
                error = "攻撃指定が空です。例: green:4,orange:8";
                return false;
            }

            string[] entries = specification.Split(',', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < entries.Length; i++)
            {
                string[] pair = entries[i].Split(':');
                if (pair.Length != 2
                    || !TryParseBeatType(pair[0].Trim(), out BeatType beatType)
                    || !int.TryParse(pair[1].Trim(), out int count)
                    || count <= 0
                    || count > MAX_ATTACK_COUNT_PER_ENTRY
                    || queue.Count + count > MAX_TOTAL_ATTACK_COUNT)
                {
                    error = $"攻撃指定'{entries[i]}'が不正です。例: green:4,orange:8";
                    queue.Clear();
                    return false;
                }

                for (int countIndex = 0; countIndex < count; countIndex++)
                {
                    queue.Enqueue(beatType);
                }
            }

            if (queue.Count == 0)
            {
                error = "有効な攻撃指定がありません。";
                return false;
            }

            error = string.Empty;
            return true;
        }

        /// <summary>
        ///     拍種名または色名を拍種へ変換する。
        /// </summary>
        /// <param name="value"> 拍種名、数値、または色名。 </param>
        /// <param name="beatType"> 変換後の拍種。 </param>
        /// <returns> 変換できた場合はtrue。 </returns>
        private static bool TryParseBeatType(string value, out BeatType beatType)
        {
            string normalized = value.Trim().ToLowerInvariant();
            switch (normalized)
            {
                case "purple":
                case "紫":
                case "one":
                case "1":
                    beatType = BeatType.One;
                    return true;
                case "blue":
                case "青":
                case "two":
                case "2":
                    beatType = BeatType.Two;
                    return true;
                case "cyan":
                case "水色":
                case "three":
                case "3":
                    beatType = BeatType.Three;
                    return true;
                case "green":
                case "緑":
                case "four":
                case "4":
                    beatType = BeatType.Four;
                    return true;
                case "yellow":
                case "黄":
                case "six":
                case "6":
                    beatType = BeatType.Six;
                    return true;
                case "orange":
                case "オレンジ":
                case "eight":
                case "8":
                    beatType = BeatType.Eight;
                    return true;
                default:
                    beatType = default;
                    return false;
            }
        }

        /// <summary>
        ///     全ての予約攻撃が成立した状態へ遷移する。
        /// </summary>
        private static void Complete()
        {
            UnsubscribeRuntimeEvents();
            _state = QueueState.Completed;
            _lastMessage = $"全てのジャスト攻撃が成立しました（{_completedCount}/{_requestedCount}）。";
            _shouldFinishAfterRelease = false;
        }

        /// <summary>
        ///     エラー状態へ遷移し、残りの予約を停止する。
        /// </summary>
        /// <param name="message"> エラー理由。 </param>
        private static void Fail(string message)
        {
            ReleaseAttackInput();
            UnsubscribeRuntimeEvents();
            _pendingAttacks.Clear();
            _hasCurrentTarget = false;
            _isPriming = false;
            _isAwaitingAttackResult = false;
            _shouldAdvanceAfterRelease = false;
            _shouldFinishAfterRelease = false;
            _state = QueueState.Failed;
            _lastMessage = message;
            Debug.LogError($"[{nameof(AIDebugAttackQueue)}] {message}");
        }

        /// <summary>
        ///     内部状態をキャンセル済みに戻す。
        /// </summary>
        /// <param name="message"> キャンセル理由。 </param>
        private static void CancelInternal(string message)
        {
            ReleaseAttackInput();
            UnsubscribeRuntimeEvents();
            _pendingAttacks.Clear();
            _playerModule = null;
            _musicSyncService = null;
            _hasCurrentTarget = false;
            _isPriming = false;
            _isAwaitingAttackResult = false;
            _shouldAdvanceAfterRelease = false;
            _shouldFinishAfterRelease = false;
            _state = QueueState.Cancelled;
            _lastMessage = message;
        }

        /// <summary>
        ///     Editorおよびランタイムのイベント購読を解除する。
        /// </summary>
        private static void UnsubscribeRuntimeEvents()
        {
            EditorApplication.update -= Update;
            if (_playerModule?.PlayerAttackSignal != null)
            {
                _playerModule.PlayerAttackSignal.OnAttackExecuted -= HandleAttackBeatExecuted;
            }
        }

        /// <summary>
        ///     Play Mode状態変更時にキューを安全に破棄する。
        /// </summary>
        /// <param name="state"> 変更後のPlay Mode状態。 </param>
        private static void HandlePlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode
                || state == PlayModeStateChange.EnteredEditMode)
            {
                CancelInternal("Play Modeが終了しました。");
            }
        }

        /// <summary>
        ///     Assembly Reload前に入力とイベント購読を解放する。
        /// </summary>
        private static void HandleBeforeAssemblyReload()
        {
            CancelInternal("Assembly Reloadによりキャンセルしました。");
        }

        /// <summary>
        ///     エラー応答JSONを生成する。
        /// </summary>
        /// <param name="message"> エラー内容。 </param>
        /// <returns> エラー応答JSON。 </returns>
        private static string CreateErrorJson(string message)
        {
            return AIDebugJson.Serialize(AIDebugJson.Object(("success", false), ("state", "Rejected"), ("message", message)));
        }

        private enum QueueState
        {
            Idle,
            Waiting,
            Completed,
            Failed,
            Cancelled
        }
    }
}
