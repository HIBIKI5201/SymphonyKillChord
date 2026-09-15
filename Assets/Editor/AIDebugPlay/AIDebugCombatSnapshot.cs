using KillChord.Runtime.Composition.InGame.Mission;
using KillChord.Runtime.Composition.InGame.Music;
using KillChord.Runtime.Composition.InGame.Player;
using KillChord.Runtime.Composition.InGame.Sequence;
using KillChord.Runtime.Composition.InGame.Target;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.Mission;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using System.Collections.Generic;
using UnityEngine;
using static KillChord.Editor.AIDebugPlay.AIDebugJson;

namespace KillChord.Editor.AIDebugPlay
{
    /// <summary>
    ///     公開済みの戦闘サービスからQAに必要な値をコピーする。
    /// </summary>
    internal static class AIDebugCombatSnapshot
    {
        /// <summary>
        ///     現在公開されているサービスを取得し、未初期化を明示する。
        /// </summary>
        internal static T Require<T>() where T : class
        {
            if (!ServiceLocator.TryGetInstance<T>(out T service) || service == null)
            {
                throw new InvalidOperationException(typeof(T).Name + " is not initialized");
            }
            return service;
        }

        /// <summary>
        ///     プレイヤーのHP、位置、攻撃制限を取得する。
        /// </summary>
        internal static object ReadPlayer()
        {
            var module = Require<PlayerModuleContainer>();
            var result = ReadCharacter(module.PlayerEntity);
            result.Add("position", module.PlayerView != null ? Position(module.PlayerView.transform.position) : null);
            result.Add("isAttacking", module.PlayerAttackController?.IsAttacking);
            result.Add("isAttackCooldown", module.PlayerAttackController?.IsAttackCooldown);
            result.Add("isInputSuppressed", module.InputSuppressionState?.IsSuppressed);
            return result;
        }

        /// <summary>
        ///     拍、判定窓、入力履歴を取得する。現在のJust範囲と過去の成立結果は区別する。
        /// </summary>
        internal static object ReadRhythm()
        {
            var module = Require<MusicSyncModuleContainer>();
            var service = module.MusicSyncService;
            var state = module.MusicSyncState;
            var beat = service.GetCurrentBeatType(out bool isJustWindow);
            var beats = service.GetBeatTypeHistory();
            var times = service.GetBeatTypeTiming();
            var actions = service.GetActionHistory();
            var history = new List<object>();
            for (int index = 0; index < beats.Length; index++)
            {
                history.Add(Object(("beatType", beats[index].ToString()), ("action", actions[index].ToString()),
                    ("musicTimeSeconds", times[index])));
            }
            var ranges = new List<object>();
            foreach (var range in service.RhythmJudgmentDefinition.JudgmentRanges)
            {
                ranges.Add(Object(("beatType", range.BeatType.ToString()),
                    ("justStartNormalized", range.JustStartNormalized), ("justEndNormalized", range.JustEndNormalized)));
            }
            return Object(("bpm", state.Bpm), ("musicTimeSeconds", state.PlayTime),
                ("currentBeat", state.CurrentBeat), ("beatTypeAtCapture", beat.ToString()),
                ("isJustWindowAtCapture", isJustWindow), ("barProgress", service.GetBarProgressUnclamped()),
                ("history", history), ("judgmentRanges", ranges));
        }

        /// <summary>
        ///     ミッションの終了理由、コンボ、行動回数を取得する。
        /// </summary>
        internal static object ReadMission()
        {
            var service = Require<MissionModuleContainer>().MissionRuntimeService;
            var progress = service.MissionProgress;
            var actions = new Dictionary<string, object>();
            foreach (MissionActionKind kind in Enum.GetValues(typeof(MissionActionKind)))
            {
                actions[kind.ToString()] = progress.ActionRecord.GetCount(kind);
            }
            return Object(("name", service.MissionDefinition.DisplayName),
                ("mainMissionText", service.MissionDefinition.MainMissionText),
                ("elapsedSeconds", progress.ElapsedTime.Value), ("objectiveStepIndex", progress.ObjectiveStepIndex),
                ("isFinished", progress.IsFinished), ("endReason", progress.EndReason.ToString()),
                ("kills", progress.EnemyKillRecord.TotalKillCount), ("damageTaken", progress.DamageTaken.Value),
                ("combo", progress.ComboCount.Value), ("maxCombo", progress.MaxCombo.Value), ("actions", actions));
        }

        /// <summary>
        ///     登録された対象と現在のロックオンを取得する。
        /// </summary>
        internal static object ReadTargets()
        {
            var module = Require<TargetSystemModuleContainer>();
            var targets = module.TargetSystemViewModel.GetRegisteredTargetsSnapshot();
            var entries = new List<object>();
            foreach (var target in targets)
            {
                if (entries.Count >= MAX_TARGETS) { break; }
                var entry = Object(("id", target.TargetId.ToString()), ("position", Position(target.Position)),
                    ("isAlive", target.IsAlive));
                entry.Add("character", module.TargetEntityRegistry.TryGetEntity(target.TargetId, out var entity)
                    ? ReadCharacter(entity) : null);
                entries.Add(entry);
            }
            bool hasTarget = module.TargetSystemViewModel.TryGetCurrentTargetId(out var id);
            return Object(("currentTargetId", hasTarget ? id.ToString() : null),
                ("total", targets.Length), ("truncated", targets.Length > MAX_TARGETS), ("items", entries));
        }

        /// <summary>
        ///     ゲーム内ポーズをEditorの一時停止とは別に取得する。
        /// </summary>
        internal static object ReadSequence()
        {
            var controller = Require<SequenceModuleContainer>().BattlePauseController;
            if (controller == null) { throw new InvalidOperationException("BattlePauseController is not initialized"); }
            return Object(("isBattlePaused", controller.IsPaused));
        }

        /// <summary>
        ///     キャラクターの公開された状態をコピーする。
        /// </summary>
        private static Dictionary<string, object> ReadCharacter(CharacterEntity entity)
        {
            return Object(("id", entity.Id.ToString()), ("name", entity.Name.Value),
                ("health", entity.CurrentHealth.Value), ("maxHealth", entity.MaxHealth.Value),
                ("barrier", entity.CurrentBarrier), ("isDead", entity.IsDead), ("isInvincible", entity.IsInvincible));
        }

        /// <summary>
        ///     Unityの座標をJSONで扱える値にコピーする。
        /// </summary>
        private static object Position(Vector3 position)
        {
            return Object(("x", position.x), ("y", position.y), ("z", position.z));
        }

        private const int MAX_TARGETS = 256;
    }
}
