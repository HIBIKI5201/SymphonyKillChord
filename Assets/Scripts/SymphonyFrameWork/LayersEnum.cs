using System;

[Flags]
public enum LayersEnum : int
{
    None = 1 << 0,
    Default = 1 << 1,
    TransparentFX = 1 << 2,
    Water = 1 << 3,
    UI = 1 << 4,
}
