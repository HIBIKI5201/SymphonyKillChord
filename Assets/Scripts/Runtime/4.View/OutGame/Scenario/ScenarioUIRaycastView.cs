using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace KillChord.Runtime.View.OutGame.Scenario
{
    /// <summary>
    ///     シナリオUIのヒット判定を行うクラス。
    /// </summary>
    public class ScenarioUIRaycastView : MonoBehaviour
    {
        /// <summary>
        ///     シナリオUI上にポインターがあるか判定する。
        /// </summary>
        public bool IsPointerOverScenarioUI()
        {
            if (EventSystem.current == null)
            {
                return false;
            }

            PointerEventData eventData = new PointerEventData(EventSystem.current)
            {
                position = Mouse.current.position.ReadValue(),
            };

            _results.Clear();

            EventSystem.current.RaycastAll(eventData, _results);

            for (int i = 0; i < _results.Count; i++)
            {
                if (_results[i].gameObject.CompareTag(ScenarioIgnoreTag))
                {
                    return true;
                }
            }

            return false;
        }

        private const string ScenarioIgnoreTag = "ScenarioIgnore";
        private readonly List<RaycastResult> _results = new();
    }
}
