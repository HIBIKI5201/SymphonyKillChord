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
        private IMusicSyncViewModel _musicSyncViewModel;
        private PlayerInputView _playerInputView;
        private int _bpm;

        public void Bind(
            MusicPlayer musicPlayer,
            MusicSyncViewModel syncViewModel,
            PlayerInputView playerInputView)
        {
            _mp = musicPlayer;
            _musicSyncViewModel = syncViewModel;
            _musicSyncViewModel.CueName.Subscribe(PlayBgm).RegisterTo(destroyCancellationToken);
            _playerInputView = playerInputView;
            _playerInputView.OnAttackInput += OnAttack;
            _playerInputView.OnDodgeInput += OnDodge;
        }

        private void OnDestroy()
        {
            _playerInputView.OnAttackInput -= OnAttack;
            _playerInputView.OnDodgeInput -= OnDodge;
        }

        public void PlayBgm(string cueName)
        {
#if UNITY_EDITOR
            _bpm = _testBpm; //TODO : cueNameを引数にデータベースからBPMを取得するように変更
#endif
            _mp.PlayBgm(cueName);
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

        /// <summary>
        /// 行動キューに保存する
        /// </summary>
        /// <param name="type"></param>
        private void RegisterActionQueue(ActionType type)
        {
            //TODO : 明らかなロジックなので今後適切な層に移動する
            //TOTO : リングバッファを使たものに変更する
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