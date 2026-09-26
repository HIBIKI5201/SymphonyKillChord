using System;
using UnityEngine.InputSystem;

namespace KillChord.Runtime.View.Persistent.Input
{
    /// <summary>
    ///     入力マップの要求構成を保持し、入力抑止中は実際のマップを停止する。
    /// </summary>
    public class UnityInputMapController
    {
        /// <summary>
        ///     必須のマップを受け取り、現在のゲーム用マップ構成を初期要求として保持する。
        /// </summary>
        public UnityInputMapController(InputActionMap commonMap,
            InputActionMap inGameMap,
            InputActionMap outGameMap,
            InputActionMap scenarioMap,
            InputActionMap uiMap)
        {
            _gameMaps = new[]
            {
                commonMap ?? throw new ArgumentNullException(nameof(commonMap)),
                inGameMap ?? throw new ArgumentNullException(nameof(inGameMap)),
                outGameMap ?? throw new ArgumentNullException(nameof(outGameMap)),
                scenarioMap ?? throw new ArgumentNullException(nameof(scenarioMap))
            };
            _uiMap = uiMap ?? throw new ArgumentNullException(nameof(uiMap));
            _requestedMaps = new bool[_gameMaps.Length];
            for (int i = 0; i < _gameMaps.Length; i++)
            {
                _requestedMaps[i] = _gameMaps[i].enabled;
            }
        }

        /// <summary>
        ///     ゲーム用マップをすべて無効にする構成を要求する。UIは入力抑止中のみ停止する。
        /// </summary>
        public void DisableAll()
        {
            Array.Clear(_requestedMaps, 0, _requestedMaps.Length);
            ApplyRequestedMaps();
        }

        /// <summary>
        ///     Commonと指定したゲーム用マップを有効にする構成を要求する。
        /// </summary>
        public void EnableCommonWith(string inputMapId)
        {
            int targetIndex = GetInputMapIndex(inputMapId);
            Array.Clear(_requestedMaps, 0, _requestedMaps.Length);
            _requestedMaps[GetInputMapIndex(InputMapNames.Common)] = true;
            _requestedMaps[targetIndex] = true;
            ApplyRequestedMaps();
        }

        /// <summary>
        ///     指定したゲーム用マップだけを有効にする構成を要求する。
        /// </summary>
        public void EnableOnly(string inputMapId)
        {
            int targetIndex = GetInputMapIndex(inputMapId);
            Array.Clear(_requestedMaps, 0, _requestedMaps.Length);
            _requestedMaps[targetIndex] = true;
            ApplyRequestedMaps();
        }

        /// <summary>
        ///     要求構成を保持したまま全マップを抑止し、解除時に最新の要求を復元する。
        /// </summary>
        public void SetInputSuppressed(bool isSuppressed)
        {
            if (_isInputSuppressed == isSuppressed)
            {
                return;
            }

            _isInputSuppressed = isSuppressed;
            ApplyMapStates();
        }

        private readonly InputActionMap[] _gameMaps;
        private readonly InputActionMap _uiMap;
        private readonly bool[] _requestedMaps;
        private bool _isInputSuppressed;

        /// <summary>
        ///     入力抑止中でなければ最新の要求を実際のマップへ反映する。
        /// </summary>
        private void ApplyRequestedMaps()
        {
            if (!_isInputSuppressed)
            {
                ApplyMapStates();
            }
        }

        /// <summary>
        ///     不要なマップを先に停止し、要求と抑止状態から実際の有効状態を適用する。
        /// </summary>
        private void ApplyMapStates()
        {
            for (int i = 0; i < _gameMaps.Length; i++)
            {
                if (_isInputSuppressed || !_requestedMaps[i])
                {
                    _gameMaps[i].Disable();
                }
            }

            if (_isInputSuppressed)
            {
                _uiMap.Disable();
                return;
            }

            for (int i = 0; i < _gameMaps.Length; i++)
            {
                if (_requestedMaps[i])
                {
                    _gameMaps[i].Enable();
                }
            }

            _uiMap.Enable();
        }

        /// <summary>
        ///     ゲーム用マップ名を要求配列の添字へ変換し、不明な名前を拒否する。
        /// </summary>
        private static int GetInputMapIndex(string inputMapId)
        {
            return inputMapId switch
            {
                InputMapNames.Common => 0,
                InputMapNames.InGame => 1,
                InputMapNames.OutGame => 2,
                InputMapNames.Scenario => 3,
                _ => throw new ArgumentOutOfRangeException(nameof(inputMapId), inputMapId, "不明なゲーム用入力マップです。")
            };
        }
    }
}
