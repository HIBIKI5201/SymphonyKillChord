using System;
public class AudioSettingData
{
    /// <summary>
    ///     モデルのオーディオデータ。
    /// </summary>
    /// <param name="master">マスター音量</param>
    /// <param name="bgm">BGM音量</param>
    /// <param name="se">SE音量</param>
    public AudioSettingData(float master, float bgm, float se,float voice)
    {
        float[] settings = new float[] { master, bgm, se,voice };
        Settings = settings;
         _volumeEvents = new Action<float>[]
        {
            v => MasterVolume?.Invoke(v),
            v => BGMVolume?.Invoke(v),
            v => SEVolume?.Invoke(v),
            v => VoiceVolume?.Invoke(v)
        };
    }
    public float[] Settings { get; set; }
    public event Action<float> MasterVolume;
    public event Action<float> BGMVolume;
    
    public event Action<float> SEVolume;

    public event Action<float> VoiceVolume;

    public float Get(int index)
    {
        if(Settings.Length <= index || index < 0) throw new Exception();
        return Settings[index];
    }
    public void Set(int index , float value)
    {
        if(Settings.Length <= index|| index < 0) throw new Exception();
        Settings[index] = value;
        _volumeEvents[index]?.Invoke(value);
    }

    private Action<float>[] _volumeEvents;
}