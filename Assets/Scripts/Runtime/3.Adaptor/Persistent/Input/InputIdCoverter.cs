using UnityEngine;
using KillChord.Runtime.Domain;

namespace KillChord.Runtime.Adaptor
{
    /// <summary>
    ///     InputActionKindをInputActionIdに変換するためのクラス。
    /// </summary>
    public static class InputIdCoverter
    {
        public static InputActionId Convert(InputActionKind actionKind)
        {
            InputActionId actionId = actionKind switch
            {
                InputActionKind.Option => InputActionId.Option,
                InputActionKind.Submit => InputActionId.Submit,
                InputActionKind.Cancel => InputActionId.Cancel,
                InputActionKind.Move => InputActionId.Move,
                InputActionKind.Dodge => InputActionId.Dodge,
                InputActionKind.Attack => InputActionId.Attack,
                _ => throw new System.ArgumentOutOfRangeException(nameof(actionKind), $"Unsupported action kind: {actionKind}")
            };
            
            return actionId;
        }
    }
}
