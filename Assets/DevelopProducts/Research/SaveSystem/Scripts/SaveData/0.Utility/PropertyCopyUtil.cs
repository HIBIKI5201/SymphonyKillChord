using System;
using System.Reflection;
using UnityEngine;
namespace Research.SaveSystem
{
    /// <summary>
    ///     クラスのプロパティをコピーするUtil。
    /// </summary>
    public static class PropertyCopyUtil
    {
        /// <summary>
        ///     sourceにあるプロパティ値をtargetの同名プロパティにコピーする。
        /// </summary>
        /// <typeparam name="TTarget"></typeparam>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="target"></param>
        /// <param name="source"></param>
        public static void CopyFields<TTarget, TSource>(TTarget target, TSource source)
        {
            Type sourceType = typeof(TSource);
            Type targetType = typeof(TTarget);

            foreach (PropertyInfo sProp in sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                PropertyInfo tProp = targetType.GetProperty(sProp.Name);
                try
                {
                    if (tProp == null) continue;
                    if (!tProp.CanWrite) continue;
                    if (!sProp.CanRead) continue;

                    if (tProp.PropertyType != sProp.PropertyType) continue;

                    var value = sProp.GetValue(source);
                    tProp.SetValue(target, value);
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException(
                        $"プロパティコピーに失敗しました: {sourceType.Name}.{sProp.Name} -> {targetType.Name}.{tProp.Name}",e);
                }
            }
        }
    }
}