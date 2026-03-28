using System;
using System.Threading;
using KillChord.Runtime.Adaptor;
using KillChord.Runtime.Utility;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KillChord.Runtime.View
{
    public class MusicSyncView : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] private int _testBpm;
#endif

        private MusicPlayer _mp;
        private MusicViewModel _musicViewModel;
        private MusicSyncViewModel _musicSyncViewModel;

        private PriorityQueue<ScheduledAction, double> _scheduledActions = new();

        private double _beatLength;
        private double _currentBeat;

        public void Bind(
            MusicPlayer musicPlayer,
            MusicSyncViewModel syncViewModel)
        {
            _mp = musicPlayer;
            _musicViewModel = _mp.MusicVM;
            _musicViewModel.CueName.Subscribe(PlayBgm).RegisterTo(destroyCancellationToken);
            _musicSyncViewModel = syncViewModel;

            _musicSyncViewModel.Register = SchedulingAction;
        }

        private void Update()
        {
            if (_mp == null || _musicSyncViewModel.Bpm <= 0) return;

            _musicSyncViewModel.Update(_mp.Time);
            
            _currentBeat = _mp.Time / _beatLength;
            _musicSyncViewModel.NearestBeat = (int)Math.Round(_currentBeat);
            _musicSyncViewModel.CurrentBeat = (int)Math.Floor(_currentBeat);

            /*
            //登録されているアクションの中から実行すべきものをすべて実行、それ以外はスキップ
            while (_scheduledActions.TryPeek(out var actionData, out _))
            {
                if (actionData.CancellationToken.IsCancellationRequested)
                {
                    _scheduledActions.Dequeue();
                    continue;
                }

                if (actionData.Time <= _mp.Time)
                {
                    _scheduledActions.Dequeue();
                    actionData.Action?.Invoke();
                    continue;
                }

                break;
            }
            */
        }

        private void PlayBgm(string cueName)
        {
#if UNITY_EDITOR
            _musicSyncViewModel.Bpm = _testBpm; //TODO : cueNameを引数にデータベースからBPMを取得するように変更
#endif
            _musicSyncViewModel.BeatLength = 60000d / _musicSyncViewModel.Bpm;
        }

        /*
        private void OnAttack(InputContext<float> context)
        {
            if (context.Phase == InputActionPhase.Started)
            {
                RegisterActionQueue(ActionType.Attack);
            }
        }

        private void OnDodge(InputContext<float> context)
        {
            if (context.Phase == InputActionPhase.Started)
            {
                RegisterActionQueue(ActionType.Dodge);
            }
        }


        private void RegisterActionQueue(ActionType type)
        {
            int signature = 1;
            if (_musicSyncViewModel.Count != 0)
            {
                var actionLength =
                    Time.unscaledTime - _musicSyncViewModel.LastAction.Timing;
                signature = GetNearestSignature(actionLength);
            }

            var param = new ActionParams(type, signature);

            _musicSyncViewModel.Enqueue(param);
        }
        */

        private void SchedulingAction(ExecuteRequestTiming ert, Action action, CancellationToken ct)
        {
            var d = GetExecuteTime(ert);
            _scheduledActions.Enqueue(new(d, action, ct), d);
        }

        private double GetExecuteTime(ExecuteRequestTiming timing)
        {
            if (_musicSyncViewModel.Bpm <= 0) return 0;
            const double propTimeSignature = 4d;
            double currentBar = Math.Floor(_currentBeat / propTimeSignature);
            double targetBar = currentBar + timing.BarFlag;

            double barLengthMs = _beatLength * propTimeSignature;
            double targetBarStartTimeMs = targetBar * barLengthMs;
            double offsetInBarMs = (barLengthMs / timing.Beat.Signature) * (timing.Beat.Count - 1);

            return targetBarStartTimeMs + offsetInBarMs;
        }

        /// <summary>
        /// 1~8拍子の中で最も近いものを取得する
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        private int GetNearestSignature(double seconds)
        {
            if (_musicSyncViewModel.Bpm <= 0) return 4;

            double beatSeconds = 60d / _musicSyncViewModel.Bpm;
            double barSeconds = beatSeconds * 4d;

            int nearestSignature = 1;
            double minDiff = double.MaxValue;

            for (int i = 1; i <= 8; i++)
            {
                double targetSeconds = barSeconds / i;
                double diff = Math.Abs(seconds - targetSeconds);

                if (diff < minDiff)
                {
                    minDiff = diff;
                    nearestSignature = i;
                }
            }

            return nearestSignature;
        }

        private readonly struct ScheduledAction
        {
            public ScheduledAction(double time, Action action, CancellationToken ct)
            {
                Time = time;
                Action = action;
                CancellationToken = ct;
            }

            public double Time { get; }
            public Action Action { get; }
            public CancellationToken CancellationToken { get; }
        }
    }
}