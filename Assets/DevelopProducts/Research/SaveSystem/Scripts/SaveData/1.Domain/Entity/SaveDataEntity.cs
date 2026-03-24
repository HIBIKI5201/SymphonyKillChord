using System;
using UnityEngine;
namespace Research.SaveSystem
{
    /// <summary>
    ///     セーブデータのEntityクラス。
    /// </summary>
    public class SaveDataEntity
    {
        public SaveDataEntity()
        {
            SystemData = new();
            PlayerData = new();
            OutGameData = new();
        }
        static SaveDataEntity()
        {
            SetterCache<SystemData>.AssignAction = (entity, data) => entity.SystemData = data;
            SetterCache<PlayerData>.AssignAction = (entity, data) => entity.PlayerData = data;
            SetterCache<OutGameData>.AssignAction = (entity, data) => entity.OutGameData = data;

            GetterCache<SystemData>.GetFunc = (entity) => entity.SystemData;
            GetterCache<PlayerData>.GetFunc = (entity) => entity.PlayerData;
            GetterCache<OutGameData>.GetFunc = (entity) => entity.OutGameData;
        }

        /// <summary>システム情報</summary>
        public SystemData SystemData { get; private set; }
        /// <summary>プレイヤー情報</summary>
        public PlayerData PlayerData { get; private set; }
        /// <summary>アウトゲーム情報</summary>
        public OutGameData OutGameData { get; private set; }

        /// <summary>
        ///     渡されたオブジェクトの型を判断し、フィールドに設定する。
        /// </summary>
        /// <typeparam name="TSaveType"></typeparam>
        /// <param name="data"></param>
        public void AssignData<TSaveType>(TSaveType data)
        {
            Action<SaveDataEntity, TSaveType> action = SetterCache<TSaveType>.AssignAction;
            if (action != null)
            {
                action(this, data);
            }
            else
            {
                throw new NotSupportedException($"未対応のセーブデータ型です: {typeof(TSaveType).FullName}");
            }
        }
        /// <summary>
        ///     型引数で判断し、フィールドを返却する
        /// </summary>
        /// <typeparam name="TSaveType"></typeparam>
        /// <returns></returns>
        public TSaveType GetData<TSaveType>()
        {
            Func<SaveDataEntity, TSaveType> func = GetterCache<TSaveType>.GetFunc;
            if(func != null) { 
                return func(this);
            }
            else
            {
                throw new NotSupportedException($"未対応のセーブデータ型です: {typeof(TSaveType).FullName}");
            }
        }

        /// <summary>
        ///     自動割り当てのためのデリゲート
        /// </summary>
        /// <typeparam name="TSaveType"></typeparam>
        private static class SetterCache<TSaveType>
        {
            public static Action<SaveDataEntity, TSaveType> AssignAction;
        }

        /// <summary>
        ///     自動フィールド取得のためのデリゲート
        /// </summary>
        /// <typeparam name="TSaveType"></typeparam>
        private static class GetterCache<TSaveType>
        {
            public static Func<SaveDataEntity, TSaveType> GetFunc;
        }
    }
}