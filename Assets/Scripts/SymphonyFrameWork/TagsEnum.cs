using System;

[Flags]
public enum TagsEnum : int
{
    None = 1 << 0,
    Untagged = 1 << 1,
    Respawn = 1 << 2,
    Finish = 1 << 3,
    EditorOnly = 1 << 4,
    MainCamera = 1 << 5,
    Player = 1 << 6,
    GameController = 1 << 7,
    GameSystem = 1 << 8,
    TouchArea = 1 << 9,
}
