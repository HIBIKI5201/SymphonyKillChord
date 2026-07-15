using System;
using UnityEngine;

namespace KillChord.Runtime.Utility.Identity
{
    /// <summary>
    ///     DataIDフィールドが属するSourceDataProviderカテゴリを指定します。
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class DataCategoryAttribute : PropertyAttribute
    {
        /// <summary>
        ///     カテゴリ名を指定して属性を初期化します。
        /// </summary>
        /// <param name="category"> SourceDataProviderへ登録するカテゴリ名です。 </param>
        public DataCategoryAttribute(string category)
        {
            Category = category;
        }

        /// <summary> SourceDataProviderへ登録されたカテゴリ名です。 </summary>
        public string Category { get; }
    }
}
