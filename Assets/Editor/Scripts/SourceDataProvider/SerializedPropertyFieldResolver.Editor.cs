using System;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;

namespace KillChord.Editor.SourceDataProvider
{
    /// <summary>
    ///     SerializedPropertyのパスを使って宣言元フィールドを解決します。
    /// </summary>
    internal static class SerializedPropertyFieldResolver
    {
        /// <summary>
        ///     対象型とプロパティパスから宣言元フィールドを取得します。
        /// </summary>
        /// <param name="targetType"> SerializedObjectの対象型です。 </param>
        /// <param name="propertyPath"> SerializedPropertyのパスです。 </param>
        /// <param name="fieldInfo"> 解決したフィールド情報です。 </param>
        /// <returns> フィールドを解決できた場合はtrueです。 </returns>
        public static bool TryResolve(Type targetType, string propertyPath, out FieldInfo fieldInfo)
        {
            fieldInfo = null;
            if (targetType == null || string.IsNullOrWhiteSpace(propertyPath))
            {
                return false;
            }

            string normalizedPath = ARRAY_PATH_PATTERN.Replace(propertyPath, "[]");
            string[] segments = normalizedPath.Split('.');
            Type currentType = targetType;

            for (int i = 0; i < segments.Length; i++)
            {
                string segment = segments[i];
                bool isCollection = segment.EndsWith("[]", StringComparison.Ordinal);
                string fieldName = isCollection
                    ? segment.Substring(0, segment.Length - 2)
                    : segment;

                fieldInfo = FindField(currentType, fieldName);
                if (fieldInfo == null)
                {
                    return false;
                }

                currentType = fieldInfo.FieldType;
                if (isCollection)
                {
                    currentType = GetElementType(currentType);
                    if (currentType == null)
                    {
                        return false;
                    }
                }
            }

            return fieldInfo != null;
        }

        /// <summary>
        ///     継承元を含めて指定名のフィールドを検索します。
        /// </summary>
        /// <param name="type"> 検索対象型です。 </param>
        /// <param name="fieldName"> フィールド名です。 </param>
        /// <returns> 見つかったフィールド情報です。 </returns>
        private static FieldInfo FindField(Type type, string fieldName)
        {
            Type currentType = type;
            while (currentType != null)
            {
                FieldInfo field = currentType.GetField(fieldName, FIELD_FLAGS);
                if (field != null)
                {
                    return field;
                }

                currentType = currentType.BaseType;
            }

            return null;
        }

        /// <summary>
        ///     配列またはListの要素型を取得します。
        /// </summary>
        /// <param name="collectionType"> コレクション型です。 </param>
        /// <returns> 要素型です。 </returns>
        private static Type GetElementType(Type collectionType)
        {
            if (collectionType.IsArray)
            {
                return collectionType.GetElementType();
            }

            if (typeof(IList).IsAssignableFrom(collectionType)
                && collectionType.IsGenericType)
            {
                return collectionType.GetGenericArguments()[0];
            }

            return null;
        }

        private const BindingFlags FIELD_FLAGS =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static readonly Regex ARRAY_PATH_PATTERN =
            new(@"\.Array\.data\[\d+\]", RegexOptions.Compiled);
    }
}
