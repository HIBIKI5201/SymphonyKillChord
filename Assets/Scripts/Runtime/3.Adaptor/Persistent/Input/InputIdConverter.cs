using KillChord.Runtime.Domain.Persistent.Input;

namespace KillChord.Runtime.Adaptor.Persistent.Input
{
    /// <summary>
    ///     InputActionKindをInputActionIdに変換するためのクラス。
    /// </summary>
    public static class InputIdConverter
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
