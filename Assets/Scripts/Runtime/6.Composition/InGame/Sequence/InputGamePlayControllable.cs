using KillChord.Runtime.Composition.Persistent.Input;
using KillChord.Runtime.View.InGame.Sequence;
using KillChord.Runtime.View.Persistent.Input;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.Sequence
{
    /// <summary>
    ///     入力のゲームプレイ制御クラス。
    /// </summary>
    public class InputGamePlayControllable : MonoBehaviour, IGameplayControllable
    {
        /// <summary>
        ///     ゲームプレイ開始時の処理。
        /// </summary>
        public void StartGameplay()
        {
            InputComposition inputComposition = GetInputComposition();

            if (inputComposition == null)
            {
                return;
            }

            inputComposition.GetInputMapController.EnableOnly(InputMapNames.InGame);
        }

        /// <summary>
        ///     ゲームプレイ停止時の処理。
        /// </summary>
        public void StopGameplay()
        {
            InputComposition inputComposition = GetInputComposition();

            if (inputComposition == null)
            {
                return;
            }

            // ゲームプレイ停止時は、Common(設定やポーズ)のみを有効化して他のマップは無効化する。
            inputComposition.GetInputMapController.EnableOnly(InputMapNames.Common);
        }

        /// <summary>
        ///    InputComposition を取得するためのヘルパーメソッド。
        /// </summary>
        /// <returns></returns>
        private InputComposition GetInputComposition()
        {
            if (_inputComposition != null)
            {
                return _inputComposition;
            }

            _inputComposition = ServiceLocator.GetInstance<InputComposition>();

            if (_inputComposition == null)
            {
                Debug.LogError("[InputGamePlayControllable] InputComposition の取得に失敗しました。", this);
            }

            return _inputComposition;
        }

        private InputComposition _inputComposition;
    }
}
