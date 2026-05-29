using KillChord.Runtime.Adaptor.InGame.Mission;
using KillChord.Runtime.View.InGame.Mission;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.Mission
{
    /// <summary>
    ///     アウトゲームにおけるミッションシステムの初期化を行うクラス。
    /// </summary>
    public class OutGameMissionInitializer : MonoBehaviour
    {
        /// <summary> ミッション選択ボタンのリスト。 </summary>
        [SerializeField, Tooltip("ミッションを選択するためのボタンのリスト。")] private OutGameMissionButtonView[] _buttons;

        /// <summary>
        ///     UnityのAwakeメソッド。
        /// </summary>
        private void Awake()
        {
            SelectedMissionState selectedMissionState = new SelectedMissionState();
            OutGameMissionSelectController controller = new OutGameMissionSelectController(selectedMissionState);

            for (int i = 0; i < _buttons.Length; i++)
            {
                _buttons[i].Initialize(controller);
            }

            ServiceLocator.RegisterInstance(selectedMissionState);
        }
    }
}
