using System;
using UnityEngine;
namespace Research.SaveSystem
{
    /// <summary>
    ///     セーブを行うServiceクラス。
    /// </summary>
    public class SaveService<TSaveType, TDtoType>
        where TSaveType : class, new()
        where TDtoType : class, new()
    {
        public SaveService(ISaveRepository<TSaveType, TDtoType> saveRepo, ISaveDataValidatior<TDtoType> validator)
        {
            _saveRepo = saveRepo;
            _saveDataValidatior = validator;
            _isSaving = false;
        }
        /// <summary>
        ///     セーブを行う。
        /// </summary>
        /// <param name="dto">セーブデータを格納するDTO</param>
        public void Save(TDtoType dto)
        {
            if (_isSaving) return;

            SaveTask(dto).Forget();
        }

        private bool _isSaving;
        private ISaveRepository<TSaveType, TDtoType> _saveRepo;
        private ISaveDataValidatior<TDtoType> _saveDataValidatior;

        private async Awaitable SaveTask(TDtoType dto)
        {
            EventBus<EOnSaveStart>.Raise(new EOnSaveStart());
            _isSaving = true;
            try
            {
                // 検証を行う
                ValidationResult validationResult = _saveDataValidatior.Validate(dto);
                if (validationResult.Result)
                {
                    // 検証OKの場合、セーブを行う
                    await _saveRepo.Save(dto);
                }
                else
                {
                    // 検証失敗の場合、エラー時の処理を発砲したり呼び出したりする
                    EOnSaveError errEvent = new EOnSaveError(validationResult.Message);
                    EventBus<EOnSaveError>.Raise(errEvent);
                }
            }
            catch (Exception e)
            {
                EOnSaveError errEvent = new EOnSaveError("セーブ処理でエラーが発生しました。");
                EventBus<EOnSaveError>.Raise(errEvent);
                Debug.LogException(e);
            }
            finally
            {
                EventBus<EOnSaveEnd>.Raise(new EOnSaveEnd());
                _isSaving = false;
            }
        }
    }
}
