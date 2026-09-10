using System;

/// <summary>
///     Symphony Frameworkが自動生成したフラグ列挙型。
/// </summary>
[Flags]
public enum TagsEnum : int
{
    /// <summary> Noneを表す。 </summary>
    None = 1 << 0,
    /// <summary> Untaggedを表す。 </summary>
    Untagged = 1 << 1,
    /// <summary> Respawnを表す。 </summary>
    Respawn = 1 << 2,
    /// <summary> Finishを表す。 </summary>
    Finish = 1 << 3,
    /// <summary> EditorOnlyを表す。 </summary>
    EditorOnly = 1 << 4,
    /// <summary> MainCameraを表す。 </summary>
    MainCamera = 1 << 5,
    /// <summary> Playerを表す。 </summary>
    Player = 1 << 6,
    /// <summary> GameControllerを表す。 </summary>
    GameController = 1 << 7,
    /// <summary> GameSystemを表す。 </summary>
    GameSystem = 1 << 8,
    /// <summary> TouchAreaを表す。 </summary>
    TouchArea = 1 << 9,
    /// <summary> ScenarioIgnoreを表す。 </summary>
    ScenarioIgnore = 1 << 10,
}
