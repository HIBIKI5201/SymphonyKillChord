using System;
using KillChord.Runtime.Adaptor;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KillChord.Runtime.View
{
    public class MusicSyncView : MonoBehaviour
    {
        public int Bpm => _bpm;

#if UNITY_EDITOR
        [SerializeField] private int _testBpm;
#endif

        private MusicPlayer _mp;
        private MusicViewModel _musicViewModel;
        private IMusicSyncViewModel _musicSyncViewModel;
        private PlayerInputView _playerInputView;
        private int _bpm;

        public void Bind(
            MusicPlayer musicPlayer,
            MusicSyncViewModel syncViewModel,
            PlayerInputView playerInputView)
        {
            _mp = musicPlayer;
            _musicViewModel = _mp.MusicVM;
            _musicViewModel.CueName.Subscribe(PlayBgm).RegisterTo(destroyCancellationToken);
            _musicSyncViewModel = syncViewModel;
            _playerInputView = playerInputView;
            _playerInputView.OnAttackInput += OnAttack;
            _playerInputView.OnDodgeInput += OnDodge;
        }

        private void Update()
        {
            if (_mp == null || _bpm <= 0) return;
        }

        private void OnDestroy()
        {
            _playerInputView.OnAttackInput -= OnAttack;
            _playerInputView.OnDodgeInput -= OnDodge;
        }

        private void PlayBgm(string cueName)
        {
#if UNITY_EDITOR
            _bpm = _testBpm; //TODO : cueNameを引数にデータベースからBPMを取得するように変更
#endif
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
        private int GetNearestSignature(double seconds)
        {
            if (_bpm <= 0) return 4;

            double beatSeconds = 60d / _bpm;
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