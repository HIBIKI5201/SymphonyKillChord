#if UNITY_EDITOR
using KillChord.Runtime.Utility.Identity;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
#endif

namespace KillChord.Develop
{
    /// <summary>
    ///     DataIDの数値IDからデバッグ表示用の文字列IDを解決します。
    /// </summary>
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public static class DataIDDebugUtility
    {
#if UNITY_EDITOR
        /// <summary>
        ///     エディタ上のアセット変更時に逆引きキャッシュを破棄するよう設定します。
        /// </summary>
        static DataIDDebugUtility()
        {
            EditorApplication.projectChanged += HandleProjectChanged;
        }
#endif

        /// <summary>
        ///     数値IDに対応する文字列IDを取得します。
        /// </summary>
        /// <param name="hashId"> DataIDに焼き込まれた数値IDです。 </param>
        /// <param name="id"> 解決した人間可読な文字列IDです。 </param>
        /// <returns>
        ///     エディタ上で対応する文字列IDが1種類だけ見つかった場合はtrueです。
        ///     エディタ外、未登録、またはハッシュ衝突がある場合はfalseです。
        /// </returns>
        public static bool TryGetId(int hashId, out string id)
        {
#if UNITY_EDITOR
            EnsureCache();
            return _idsByHash.TryGetValue(hashId, out id);
#else
            id = null;
            return false;
#endif
        }

#if UNITY_EDITOR
        private const string ASSET_SEARCH_FILTER = "t:ScriptableObject";
        private const string ID_PROPERTY_NAME = "_id";
        private const string HASH_PROPERTY_NAME = "_hashId";
        private static readonly string[] SEARCH_FOLDERS = { "Assets" };
        private static readonly Dictionary<int, string> _idsByHash = new();
        private static readonly HashSet<int> _collidedHashIds = new();
        private static bool _isCacheInitialized;

        /// <summary>
        ///     アセット変更後に逆引きキャッシュを破棄します。
        /// </summary>
        private static void HandleProjectChanged()
        {
            _idsByHash.Clear();
            _collidedHashIds.Clear();
            _isCacheInitialized = false;
        }

        /// <summary>
        ///     プロジェクト内のDataIDから逆引きキャッシュを構築します。
        /// </summary>
        private static void EnsureCache()
        {
            if (_isCacheInitialized)
            {
                return;
            }

            string[] assetGuids = AssetDatabase.FindAssets(ASSET_SEARCH_FILTER, SEARCH_FOLDERS);
            for (int i = 0; i < assetGuids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assetGuids[i]);
                UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                for (int j = 0; j < assets.Length; j++)
                {
                    CollectFromObject(assets[j]);
                }
            }

            _isCacheInitialized = true;
        }

        /// <summary>
        ///     Unityオブジェクトが持つDataIDを逆引きキャッシュへ登録します。
        /// </summary>
        /// <param name="target"> 走査するUnityオブジェクトです。 </param>
        private static void CollectFromObject(UnityEngine.Object target)
        {
            if (target == null)
            {
                return;
            }

            SerializedObject serializedObject = new(target);
            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = true;
                if (!string.Equals(iterator.type, nameof(DataID), StringComparison.Ordinal))
                {
                    continue;
                }

                SerializedProperty idProperty = iterator.FindPropertyRelative(ID_PROPERTY_NAME);
                SerializedProperty hashProperty = iterator.FindPropertyRelative(HASH_PROPERTY_NAME);
                if (idProperty == null
                    || hashProperty == null
                    || string.IsNullOrWhiteSpace(idProperty.stringValue))
                {
                    continue;
                }

                Register(hashProperty.intValue, idProperty.stringValue);
            }
        }

        /// <summary>
        ///     文字列IDを逆引きキャッシュへ登録します。
        /// </summary>
        /// <param name="hashId"> 登録する数値IDです。 </param>
        /// <param name="id"> 登録する文字列IDです。 </param>
        private static void Register(int hashId, string id)
        {
            if (_collidedHashIds.Contains(hashId))
            {
                return;
            }

            if (!_idsByHash.TryGetValue(hashId, out string registeredId))
            {
                _idsByHash.Add(hashId, id);
                return;
            }

            if (string.Equals(registeredId, id, StringComparison.Ordinal))
            {
                return;
            }

            _idsByHash.Remove(hashId);
            _collidedHashIds.Add(hashId);
        }
#endif
    }
}
