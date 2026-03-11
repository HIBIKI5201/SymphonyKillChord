using DevelopProducts.Persistent.Domain.Input;
using UnityEngine;

namespace DevelopProducts.Persistent.Application
{
    /// <summary>
    ///     入力のマッピングを管理するためのインターフェース。
    /// </summary>
    public interface IInputMapController
    {
        void EnableOnly(InputMapId inputMapId);
        void EnableCommonWith(InputMapId inputMapId);
         void DisableAll();
    }
}
