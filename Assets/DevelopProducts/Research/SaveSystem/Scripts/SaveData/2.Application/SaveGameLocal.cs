using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine;
namespace Research.SaveSystem
{
    /// <summary>
    ///     ファイルでセーブデータを管理するクラス（検証用）
    /// </summary>
    public class SaveGameLocal : ISaveService
    {
        public SaveGameLocal()
        {
            _savefilePath = Path.Combine(Application.persistentDataPath, Constants.SAVE_DATA_FILE_NAME);
        }

        public void Save(KillChordGameData newData)
        {
            try
            {
                string json = JsonConvert.SerializeObject(newData);
                File.WriteAllText(_savefilePath, json);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private string _savefilePath;
    }
}