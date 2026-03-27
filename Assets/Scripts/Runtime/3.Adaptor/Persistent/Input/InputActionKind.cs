using UnityEngine;

namespace KillChord.Runtime.Adaptor
{
    /// <summary>
    ///     入力の種類を表す列挙型。
    ///     受付用のenum。
    /// </summary>
    public enum InputActionKind
    {
        Option = 0,
        Submit = 10,
        Cancel = 11,
        Move = 100,
        Dodge = 101,
        Attack = 102,
        Look = 103,
    }
}
