namespace KillChord.Runtime.Adaptor.Persistent.Input
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
        LockOn = 104,
        ScenarioAdvance = 200,
        ScenarioFastForward = 201,
        ScenarioPause = 202,
        ScenarioSkip = 203,
        ScenarioAuto = 204,
        ScenarioHideUI = 205,
    }
}
