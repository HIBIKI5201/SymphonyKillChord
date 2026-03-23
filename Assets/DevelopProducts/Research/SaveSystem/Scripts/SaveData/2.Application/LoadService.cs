using System;
using UnityEngine;

namespace Research.SaveSystem
{
    /// <summary>
    ///     ロードを行うServiceクラス。
    /// </summary>
    /// <typeparam name="TSaveType"></typeparam>
    /// <typeparam name="TDtoType"></typeparam>
    public class LoadService<TSaveType, TDtoType>
        where TSaveType : class, new()
        where TDtoType : class, new()
    {
        public LoadService(SaveDataEntity saveDataEntity, ILoadRepository<TSaveType> loadRepo)
        {
            _saveDataEntity = saveDataEntity;
            _loadRepo = loadRepo;
        }

        /// <summary>
        ///     ロードを行う。
        /// </summary>
        /// <param name="callback">ロード後に実行した処理</param>
        public void Load(Action<TDtoType> callback)
        {
            if (_isLoading) return;

            LoadAsyncTask(callback).Forget();
        }

        private async Awaitable LoadAsyncTask(Action<TDtoType> callback)
        {
            _isLoading = true;
            EventBus<EOnLoadStart>.Raise(new EOnLoadStart());
            try
            {
                await _loadRepo.Load();
                TDtoType dto = new();
                PropertyCopyUtil.CopyFields(dto, _saveDataEntity.GetData<TSaveType>());
                callback?.Invoke(dto);
            }
            catch (Exception e)
            {
                EventBus<EOnLoadError>.Raise(new EOnLoadError("ロード処理でエラーが発生しました。"));
                Debug.LogException(e);
            }
            finally
            {
                EventBus<EOnLoadEnd>.Raise(new EOnLoadEnd());
                _isLoading = false;
            }
        }

        private SaveDataEntity _saveDataEntity;
        private ILoadRepository<TSaveType> _loadRepo;
        private bool _isLoading;
    }
}