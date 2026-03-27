using UnityEngine.InputSystem;

namespace KillChord.Runtime.View
{
    /// <summary>
    ///     UnityのInputActionMapを制御するためのクラス。
    /// </summary>
    public class UnityInputMapController
    {
        public UnityInputMapController(InputActionMap commonMap,
            InputActionMap inGameMap,
            InputActionMap outGameMap)
        {
            _commonMap = commonMap;
            _inGameMap = inGameMap;
            _outGameMap = outGameMap;
        }

        /// <summary>
        ///     すべてのInputActionMapを無効化する。
        /// </summary>
        public void DisableAll()
        {
            _commonMap.Disable();
            _inGameMap.Disable();
            _outGameMap.Disable();
        }

        /// <summary>
        ///     CommonのInputActionMapを有効化し、さらに引数で指定されたInputActionMapも有効化する。
        /// </summary>
        /// <param name="inputMapId"></param>
        public void EnableCommonWith(string inputMapId)
        {
            DisableAll();
            _commonMap.Enable();

            InputActionMap target = GetInputMap(inputMapId);
            if (target != null)
            {
                target.Enable();
            }
        }

        /// <summary>
        ///     指定したInputActionMapのみを有効化する。
        /// </summary>
        /// <param name="inputMapId"></param>
        public void EnableOnly(string inputMapId)
        {
            DisableAll();

            InputActionMap target = GetInputMap(inputMapId);
            if (target != null)
            {
                target.Enable();
            }
        }

        private readonly InputActionMap _commonMap;
        private readonly InputActionMap _inGameMap;
        private readonly InputActionMap _outGameMap;

        /// <summary>
        ///     引数で指定されたInputMapIdに対応するInputActionMapを返す。
        /// </summary>
        /// <param name="inputMapId"></param>
        /// <returns></returns>
        private InputActionMap GetInputMap(string inputMapId)
        {
            return inputMapId switch
            {
                InputMapNames.Common => _commonMap,
                InputMapNames.InGame => _inGameMap,
                InputMapNames.OutGame => _outGameMap,
                _ => null
            };
        }
    }
}
