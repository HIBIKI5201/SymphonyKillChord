using System;
using KillChord.Runtime.View;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KillChord.Runtime.Adaptor
{
    public class MusicSyncController
    {
        private IMusicSyncViewModel _musicSyncViewModel;

        public MusicSyncController(IMusicSyncViewModel musicSyncViewModel)
        {
            _musicSyncViewModel = musicSyncViewModel;

            _musicSyncViewModel.OnUpdate += Update;
        }

        private void Update()
        {
            var time = _musicSyncViewModel.PlayTime;

            /*
            //登録されているアクションの中から実行すべきものをすべて実行、それ以外はスキップ
            while (_scheduledActions.TryPeek(out var actionData, out _))
            {
                if (actionData.CancellationToken.IsCancellationRequested)
                {
                    _scheduledActions.Dequeue();
                    continue;
                }

                if (actionData.Time <= _musicSyncViewModel.PlayTime)
                {
                    _scheduledActions.Dequeue();
                    actionData.Action?.Invoke();
                    continue;
                }

                break;
            }
            */
        }

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

        /// <summary>
        /// 1~8拍子の中で最も近いものを取得する
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public int GetNearestSignature(double seconds)
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
    }
}