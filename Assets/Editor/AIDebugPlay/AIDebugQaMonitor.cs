using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using static KillChord.Editor.AIDebugPlay.AIDebugJson;

namespace KillChord.Editor.AIDebugPlay
{
    /// <summary>
    ///     有限時間のEditor観測を行い、エラーとフレーム進行・メモリを記録する。
    /// </summary>
    [InitializeOnLoad]
    public static class AIDebugQaMonitor
    {
        /// <summary>
        ///     Editor終了時の観測解除を登録する。
        /// </summary>
        static AIDebugQaMonitor()
        {
            AssemblyReloadEvents.beforeAssemblyReload += BeforeReloadHandler;
            EditorApplication.playModeStateChanged += PlayModeChangedHandler;
        }

        /// <summary>
        ///     実行IDを指定して観測を開始する。同じIDの再送は観測をリセットしない。
        /// </summary>
        public static string Start(string runId, double durationSeconds = 900d)
        {
            // 引数と実行状態を検証する。同じ runId の再要求は現在の状態を返す。
            if (!Guid.TryParse(runId, out _) || double.IsNaN(durationSeconds) || double.IsInfinity(durationSeconds)
                || durationSeconds < 1d || durationSeconds > MAX_DURATION_SECONDS)
            {
                return Serialize(Object(("success", false), ("message", "UUID and duration 1..3600 seconds are required")));
            }
            if (runId == _runId) { return GetStatusJson(); }
            if (_state == "Running")
            {
                return Serialize(Object(("success", false), ("message", "Another monitor owns the session")));
            }
            if (!EditorApplication.isPlaying || EditorApplication.isPaused)
            {
                return Serialize(Object(("success", false), ("message", "An unpaused PlayMode session is required")));
            }
            // 計測値を初期化する。
            _runId = runId;
            _state = "Running";
            _reason = "";
            _startedAt = _sampleAt = EditorApplication.timeSinceStartup;
            _duration = durationSeconds;
            _endedAt = 0d;
            _lastFrame = Time.frameCount;
            _frames = 0;
            _measuredSeconds = 0d;
            _lastFps = _minFps = double.NaN;
            _memoryStart = _memoryCurrent = Profiler.GetTotalAllocatedMemoryLong();
            _memoryPeak = _memoryCurrent;
            _sampleCount = 0;
            // 前回のログを消し、ログと戦闘の記録を開始する。
            lock (_logLock)
            {
                _logs.Clear();
                _errors = _warnings = _logSequence = 0;
            }
            SessionState.EraseString(RELOAD_KEY);
            _combatRecorder?.Dispose();
            _combatRecorder = new AIDebugCombatRecorder();
            Application.logMessageReceivedThreaded += LogReceivedHandler;
            EditorApplication.update += Update;
            return GetStatusJson();
        }

        /// <summary>
        ///     観測状態を取得する。Completedは観測時間の満了を表し、QA合格ではない。
        /// </summary>
        public static string GetStatusJson()
        {
            // 計測していない場合は、ドメインリロードで中断された結果があればそれを返す。
            if (_runId == null)
            {
                string interrupted = SessionState.GetString(RELOAD_KEY, "");
                if (!string.IsNullOrEmpty(interrupted)) { return interrupted; }
            }
            object[] logs;
            int errors;
            int warnings;
            int sequence;
            // ログは別スレッドからも書き込まれるため、ロックしてから写し取る。
            lock (_logLock)
            {
                logs = _logs.ToArray();
                errors = _errors;
                warnings = _warnings;
                sequence = _logSequence;
            }
            // 計測結果を JSON にまとめる。
            double elapsed = _runId == null ? 0d
                : (_state == "Running" ? EditorApplication.timeSinceStartup : _endedAt) - _startedAt;
            return Serialize(Object(("success", true), ("runId", _runId), ("state", _state), ("reason", _reason),
                ("environment", "UnityEditor"), ("elapsedSeconds", elapsed), ("durationSeconds", _duration),
                ("measuredSeconds", _measuredSeconds), ("sampleCount", _sampleCount),
                ("averageFps", _measuredSeconds > 0d ? _frames / _measuredSeconds : double.NaN),
                ("minimumSampleFps", _minFps), ("lastSampleFps", _lastFps),
                ("memoryStartBytes", _memoryStart), ("memoryCurrentBytes", _memoryCurrent),
                ("memoryPeakBytes", _memoryPeak), ("errors", errors), ("warnings", warnings),
                ("logSequence", sequence), ("logsDropped", Math.Max(0, sequence - MAX_LOGS)), ("recentLogs", logs),
                ("combat", ReadCombat())));
        }

        /// <summary>
        ///     所有する観測だけを明示的に停止する。
        /// </summary>
        public static string Stop(string runId)
        {
            if (runId != _runId)
            {
                return Serialize(Object(("success", false), ("message", "Monitor runId does not match")));
            }
            if (_state == "Running") { Finish("Cancelled", "Requested by owner"); }
            return GetStatusJson();
        }

        /// <summary>
        ///     スナップショットへ観測期間を明記したイベント履歴を渡す。
        /// </summary>
        internal static object ReadCombat()
        {
            return _combatRecorder == null ? Unavailable("Start a monitor to record combat events")
                : Object(("available", true), ("runId", _runId), ("isRecording", _state == "Running"),
                    ("value", _combatRecorder.Read()));
        }

        private const double MAX_DURATION_SECONDS = 3600d;
        private const double SAMPLE_SECONDS = 1d;
        private const int MAX_LOGS = 32;
        private const int MAX_LOG_LENGTH = 2048;
        private const string RELOAD_KEY = "KillChord.QA.InterruptedMonitor";
        private static readonly object _logLock = new();
        private static readonly Queue<object> _logs = new();
        private static string _runId;
        private static string _state = "Idle";
        private static string _reason = "";
        private static double _startedAt, _endedAt, _duration, _sampleAt, _measuredSeconds, _lastFps, _minFps;
        private static long _memoryStart, _memoryCurrent, _memoryPeak, _frames;
        private static int _lastFrame, _sampleCount, _errors, _warnings, _logSequence;
        private static AIDebugCombatRecorder _combatRecorder;

        /// <summary>
        ///     実時間で期限を確認し、Editorの呼び出し数でなくゲームのフレーム差分を計測する。
        /// </summary>
        private static void Update()
        {
            _combatRecorder?.RefreshPlayer();
            double now = EditorApplication.timeSinceStartup;
            if (EditorApplication.isPaused)
            {
                _sampleAt = now;
                _lastFrame = Time.frameCount;
            }
            else if (now - _sampleAt >= SAMPLE_SECONDS)
            {
                double seconds = now - _sampleAt;
                int frames = Math.Max(0, Time.frameCount - _lastFrame);
                _lastFps = frames / seconds;
                _minFps = double.IsNaN(_minFps) ? _lastFps : Math.Min(_minFps, _lastFps);
                _frames += frames;
                _measuredSeconds += seconds;
                _sampleCount++;
                _memoryCurrent = Profiler.GetTotalAllocatedMemoryLong();
                _memoryPeak = Math.Max(_memoryPeak, _memoryCurrent);
                _sampleAt = now;
                _lastFrame = Time.frameCount;
            }
            if (now - _startedAt >= _duration) { Finish("Completed", "Observation duration reached"); }
        }

        /// <summary>
        ///     バックグラウンドスレッドから来るログも上限付きで記録する。
        /// </summary>
        private static void LogReceivedHandler(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Log) { return; }
            lock (_logLock)
            {
                if (type == LogType.Warning) { _warnings++; }
                else { _errors++; }
                _logSequence++;
                if (_logs.Count == MAX_LOGS) { _logs.Dequeue(); }
                _logs.Enqueue(Object(("sequence", _logSequence), ("type", type.ToString()),
                    ("capturedAtUtc", DateTime.UtcNow.ToString("O")), ("message", Truncate(condition)),
                    ("stackTrace", Truncate(stackTrace))));
            }
        }

        /// <summary>
        ///     PlayMode離脱を成功と区別して記録する。
        /// </summary>
        private static void PlayModeChangedHandler(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode && _state == "Running")
            {
                Finish("Interrupted", "PlayMode exited");
            }
        }

        /// <summary>
        ///     リロード後も中断結果を取得できるよう保存する。
        /// </summary>
        private static void BeforeReloadHandler()
        {
            if (_state == "Running") { Finish("Interrupted", "Assembly reload"); }
            if (_runId != null) { SessionState.SetString(RELOAD_KEY, GetStatusJson()); }
        }

        /// <summary>
        ///     観測の購読を解除して最終状態を保持する。
        /// </summary>
        private static void Finish(string state, string reason)
        {
            EditorApplication.update -= Update;
            Application.logMessageReceivedThreaded -= LogReceivedHandler;
            _combatRecorder?.Dispose();
            _endedAt = EditorApplication.timeSinceStartup;
            _state = state;
            _reason = reason;
        }

        /// <summary>
        ///     記録サイズを制限する。
        /// </summary>
        private static string Truncate(string value)
        {
            return value != null && value.Length > MAX_LOG_LENGTH ? value.Substring(0, MAX_LOG_LENGTH) : value;
        }
    }
}
