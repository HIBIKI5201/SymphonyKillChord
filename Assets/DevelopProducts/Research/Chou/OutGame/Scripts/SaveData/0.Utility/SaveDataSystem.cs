//using Newtonsoft.Json;
//using System;
//using UnityEngine;
//namespace Research.Chou.OutGame
//{
//    /// <summary>
//    ///     セーブデータを管理するクラス
//    /// </summary>
//    /// <typeparam name="DataType">データの型</typeparam>
//    public static class SaveDataSystem<DataType>
//        where DataType : class, new()
//    {
//        public static DataType Data
//        {
//            get
//            {
//                if (_saveData is null || _saveData.MainData is null)
//                    Load();
//                return _saveData?.MainData;
//            }
//        }

//        public static string SaveDate
//        {
//            get
//            {
//                if (_saveData is null)
//                    Load();
//                return _saveData?.SaveDate;
//            }
//        }

//        public static string Version
//        {
//            get
//            {
//                if (_saveData is null)
//                    Load();
//                return _saveData?.Version;
//            }
//        }

//        public static void Dispose()
//        {
//            _saveData = null;
//        }

//        private static SaveData _saveData;

//        /// <summary>
//        ///     saveDataを保存する
//        /// </summary>
//        public static void Save()
//        {
//            _saveData = new SaveData(Data); //今のデータでセーブデータを作る

//            //Json化してセーブ
//            var data = JsonConvert.SerializeObject(_saveData);
//            PlayerPrefs.SetString(typeof(DataType).Name, data);

//            Debug.Log($"[{nameof(SaveDataSystem<DataType>)}]\nデータをセーブしました date : {_saveData.SaveDate}\n{data}");
//        }

//        /// <summary>
//        ///     DataTypeのデータを取得する
//        /// </summary>
//        private static void Load()
//        {
//            #region Prefsからデータをロードする

//            var json = PlayerPrefs.GetString(typeof(DataType).Name);
//            if (string.IsNullOrEmpty(json))
//            {
//                Debug.Log($"[{nameof(SaveDataSystem<DataType>)}]\n{typeof(DataType).Name}のデータが見つからないので生成しました");
//                _saveData = new SaveData(new DataType());
//                return;
//            }

//            #endregion

//            #region JSONに変換して保存

//            var data = JsonConvert.DeserializeObject<SaveData>(json);
//            if (data is not null)
//            {
//                Debug.Log($"[{nameof(SaveDataSystem<DataType>)}]\n{typeof(DataType).Name}のデータがロードされました\n{data}");
//                _saveData = data;
//            }
//            else
//            {
//                Debug.LogWarning($"[{nameof(SaveDataSystem<DataType>)}]\n{typeof(DataType).Name}のロードが出来ませんでした\n新たなインスタンスを生成します");
//                _saveData = new SaveData(new DataType()); //新しいデータを取得
//            }

//            #endregion
//        }

//        [Serializable]
//        private class SaveData
//        {
//            public SaveData(DataType dataType)
//            {
//                _saveDate = DateTime.Now.ToString("O");
//                _mainData = dataType;
//            }

//            public string SaveDate => _saveDate;
//            public string Version => _version;
//            public DataType MainData => _mainData;

//            private readonly string _saveDate;
//            private readonly string _version;
//            private readonly DataType _mainData;
//        }
//    }
//}
