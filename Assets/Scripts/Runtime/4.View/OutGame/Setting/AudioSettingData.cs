using System;

public struct AudioSettingData
{
    public AudioSettingData(float[] settings)
    {
        SettingsFlaot = settings;
        MasterVolume = settings[0];
        BgmVolume = settings[1];
        SeVolume = settings[2];
    }
    public float[] SettingsFlaot { get; set; }
    public float MasterVolume { get; set; }
    public float BgmVolume { get; set; }
    public float SeVolume { get; set; }
}