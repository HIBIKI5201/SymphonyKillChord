using System;
public struct AudioSettingData
{
    /// <summary>
    ///     モデルのオーディオデータ。
    /// </summary>
    /// <param name="master">マスター音量</param>
    /// <param name="bgm">BGM音量</param>
    /// <param name="se">SE音量</param>
    public AudioSettingData(float master, float bgm, float se)
    {
        float[] settings = new float[] { master, bgm, se };
        Settings = settings;
    }
    public float[] Settings { get; set; }
}