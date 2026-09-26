using System;

namespace KillChord.Runtime.View.OutGame.Setting
{
    /// <summary>
    ///     音量設定の値と、値が変わったときのイベントを持つデータ。
    /// </summary>
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

        /// <summary> マスター・BGM・SE・ボイスの順に並んだ音量の配列。 </summary>
        public float[] Settings { get; set; }
        /// <summary> マスター音量が変わったときに発火するイベント。 </summary>
        public event Action<float> MasterVolume;
        /// <summary> BGM 音量が変わったときに発火するイベント。 </summary>
        public event Action<float> BGMVolume;
        
        /// <summary> SE 音量が変わったときに発火するイベント。 </summary>
        public event Action<float> SEVolume;

        /// <summary> ボイス音量が変わったときに発火するイベント。 </summary>
        public event Action<float> VoiceVolume;

        /// <summary>
        ///     指定インデックスの音量を取得する。範囲外の場合は例外を投げる。
        /// </summary>
        public float Get(int index)
        {
            if(Settings.Length <= index || index < 0) throw new Exception();
            return Settings[index];
        }

        /// <summary>
        ///     指定インデックスの音量を設定し、対応するイベントを発火する。
        ///     範囲外の場合は例外を投げる。
        /// </summary>
        public void Set(int index , float value)
        {
            if(Settings.Length <= index|| index < 0) throw new Exception();
            Settings[index] = value;
            _volumeEvents[index]?.Invoke(value);
        }

        private Action<float>[] _volumeEvents;
    }
}
