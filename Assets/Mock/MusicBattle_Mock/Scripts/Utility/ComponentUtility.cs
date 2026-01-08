using System.Runtime.CompilerServices;
using UnityEngine;

namespace Mock.MusicBattle.Utility
{
    public static class ComponentUtility
    {
        /// <summary>
        ///     オブジェクトのコンポーネントを取得する。
        ///     もしコンポーネントが無ければ追加する。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T AddOrGetComponent<T>(this GameObject obj) where T : Component
        {
            if (obj.TryGetComponent(out T component))
            {
                return component;
            }
            else
            {
                return obj.AddOrGetComponent<T>();
            }
        }
    }
}
