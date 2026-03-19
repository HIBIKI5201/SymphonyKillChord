using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine;
namespace Research.SaveSystem
{
    /// <summary>
    ///     ファイルでセーブデータを管理するクラス（検証用）
    /// </summary>
    public class LoadGameLocal : ILoadService
    {
        public LoadGameLocal()
        {
            _savefilePath = Path.Combine(Application.persistentDataPath, Constants.SAVE_DATA_FILE_NAME);
        }

        public void Load(Action<KillChordGameData> callback)
        {
            KillChordGameData data;
            if (File.Exists(_savefilePath))
            {
                try
                {
                    string json = File.ReadAllText(_savefilePath);
                    data = JsonConvert.DeserializeObject<KillChordGameData>(json);
                    if(data is null)
                    {
                        Debug.Log("セーブデータ読み込みが失敗しました。新しいデータを作成します。");
                        data = new KillChordGameData();
                    }
                }
                catch (Exception e)
                {
                    Debug.Log("セーブデータ読み込みが失敗しました。新しいデータを作成します。");
                    data = new KillChordGameData();
                }
            }
            else
            {
                Debug.Log("セーブデータが見つかりませんでした。新しいデータを作成します。");
                data = new KillChordGameData();
            }
            callback?.Invoke(data);
        }

        private string _savefilePath;
    }


}