using KillChord.Runtime.Adaptor.InGame.Battle;
using KillChord.Runtime.Composition.InGame.Player;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Utility.Persistent;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using System.Collections.Generic;
using UnityEngine;
using static KillChord.Editor.AIDebugPlay.AIDebugJson;

namespace KillChord.Editor.AIDebugPlay
{
    /// <summary>
    ///     観測開始後の攻撃・スキル・被弾イベントを有限の履歴として保持する。
    /// </summary>
    internal sealed class AIDebugCombatRecorder : IDisposable
    {
        /// <summary>
        ///     既存イベントを購読し、現在のプレイヤーへ接続する。
        /// </summary>
        internal AIDebugCombatRecorder()
        {
            EventBus<EOnSkillExecuted>.Register(SkillExecutedHandler);
            EventBus<EOnTakeDamage>.Register(DamageHandler);
            EventBus<EOnPlayerTakeDamage>.Register(PlayerDamageHandler);
            RefreshPlayer();
        }

        /// <summary>
        ///     シーン遷移で交換されたプレイヤーの購読を張り替える。
        /// </summary>
        internal void RefreshPlayer()
        {
            ServiceLocator.TryGetInstance<PlayerModuleContainer>(out var module);
            var next = module?.PlayerAttackController;
            if (ReferenceEquals(_player, next) && ReferenceEquals(_signal, module?.PlayerAttackSignal)) { return; }
            UnbindPlayer();
            _player = next;
            _signal = module?.PlayerAttackSignal;
            if (_signal != null) { _signal.OnAttackExecuted += AttackJudgmentHandler; }
            if (_player != null)
            {
                _player.OnAttackBeatExecuted += AttackBeatHandler;
                _player.OnAttackExecuted += AttackExecutedHandler;
            }
        }

        /// <summary>
        ///     件数と履歴を返す。履歴の欠落数も必ず通知する。
        /// </summary>
        internal object Read()
        {
            return Object(("sequence", _sequence), ("dropped", Math.Max(0, _sequence - MAX_EVENTS)),
                ("counts", new Dictionary<string, object>(_counts)), ("recentEvents", _events.ToArray()));
        }

        /// <summary>
        ///     所有するイベント購読だけを解除する。
        /// </summary>
        public void Dispose()
        {
            UnbindPlayer();
            EventBus<EOnSkillExecuted>.Unregister(SkillExecutedHandler);
            EventBus<EOnTakeDamage>.Unregister(DamageHandler);
            EventBus<EOnPlayerTakeDamage>.Unregister(PlayerDamageHandler);
        }

        private const int MAX_EVENTS = 128;
        private readonly Queue<object> _events = new();
        private readonly Dictionary<string, object> _counts = new();
        private PlayerAttackController _player;
        private IPlayerAttackSignal _signal;
        private int _sequence;

        /// <summary>
        ///     成立した拍種を記録する。
        /// </summary>
        private void AttackBeatHandler(BeatType beat)
        {
            Record("attackBeat", Object(("beatType", beat.ToString())));
        }

        /// <summary>
        ///     武器名と初撃の命中有無を記録する。
        /// </summary>
        private void AttackExecutedHandler(string attackName, bool hasHit)
        {
            Record("attack", Object(("attackName", attackName), ("firstHitConnected", hasHit)));
        }

        /// <summary>
        ///     攻撃に適用された拍種とJust判定を記録する。
        /// </summary>
        private void AttackJudgmentHandler(int beatCount, bool isJustHit)
        {
            Record("attackJudgment", Object(("beatCount", beatCount), ("isJustHit", isJustHit)));
        }

        /// <summary>
        ///     発動が成立したスキルIDを記録する。効果の成功とは区別する。
        /// </summary>
        private void SkillExecutedHandler(EOnSkillExecuted data)
        {
            Record("skill", Object(("skillId", data.SkillId)));
        }

        /// <summary>
        ///     実際に通知されたダメージとJust・クリティカル情報を記録する。
        /// </summary>
        private void DamageHandler(EOnTakeDamage data)
        {
            Record("enemyDamage", Object(("defenderId", data.DefenderId.ToString()), ("damage", data.Damage),
                ("critical", data.Critical), ("isJustHit", data.IsJustHit), ("attackType", data.AttackType.ToString())));
        }

        /// <summary>
        ///     プレイヤーの被弾を記録する。
        /// </summary>
        private void PlayerDamageHandler(EOnPlayerTakeDamage data)
        {
            Record("playerDamage", Object(("damage", data.Damage)));
        }

        /// <summary>
        ///     フレーム番号付きのイベントを追記する。
        /// </summary>
        private void Record(string kind, object data)
        {
            _sequence++;
            _counts[kind] = _counts.TryGetValue(kind, out var count) ? (int)count + 1 : 1;
            if (_events.Count == MAX_EVENTS) { _events.Dequeue(); }
            _events.Enqueue(Object(("sequence", _sequence), ("frame", Time.frameCount),
                ("gameTimeSeconds", Time.timeAsDouble), ("kind", kind), ("data", data)));
        }

        /// <summary>
        ///     以前のプレイヤーの購読を解除する。
        /// </summary>
        private void UnbindPlayer()
        {
            if (_signal != null) { _signal.OnAttackExecuted -= AttackJudgmentHandler; }
            _signal = null;
            if (_player == null) { return; }
            _player.OnAttackBeatExecuted -= AttackBeatHandler;
            _player.OnAttackExecuted -= AttackExecutedHandler;
            _player = null;
        }
    }
}
