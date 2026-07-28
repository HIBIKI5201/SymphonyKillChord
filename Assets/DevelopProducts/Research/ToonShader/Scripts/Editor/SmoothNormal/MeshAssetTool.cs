#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class MeshAssetTool : EditorWindow
{
    private const string MENU_ROOT = "Tools/MeshAssetTool/";
    private static string SaveDir => GetSaveDir();

    private static Dictionary<Mesh, Mesh> _cache = new();
    private static List<Object> _objectsList = new();


    [MenuItem(MENU_ROOT + "①Generate Mesh Assets")]
    static void GeneratePhase()
    {
        EnsureSaveDir();

        int created = 0;
        int skipped = 0;

        foreach (var mf in GetAllMeshFilters())
        {
            if (mf.sharedMesh == null) continue;
            Mesh result = CreateMeshAsset(mf.sharedMesh);
            if (result != null) created++;
            else skipped++;
        }
        foreach (var mf in GetAllSkinnedMeshRenderer())
        {
            if (mf.sharedMesh == null) continue;
            Mesh result = CreateMeshAsset(mf.sharedMesh);
            if (result != null) created++;
            else skipped++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Generate完了 → 生成: {created}個 / スキップ: {skipped}個", Selection.activeObject);
    }

    [MenuItem(MENU_ROOT + "②Apply Mesh Assets")]
    static void ApplyPhase()
    {
        _objectsList.Clear();
        var filter = Selection.activeGameObject.GetComponentsInChildren<MeshFilter>(true);
        foreach (var item in filter)
        {
            if (item.sharedMesh == null) continue;
            _objectsList.Add(item);
        }
        var renderer = Selection.activeGameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        foreach (var item in renderer)
        {
            if (item.sharedMesh == null) continue;
            _objectsList.Add(item);
        }
        Undo.RecordObjects(_objectsList.ToArray(), "モデル置き換え");


        // 同一Meshの重複処理を防ぐキャッシュ
        _cache.Clear();
        int applied = 0;
        int missing = 0;

        foreach (var mf in filter)
        {
            if (mf.sharedMesh == null) continue;

            Mesh original = mf.sharedMesh;

            if (!_cache.TryGetValue(original, out Mesh loaded))
            {
                loaded = LoadMeshAsset(original);
                _cache[original] = loaded;
            }

            if (loaded != null)
            {
                mf.sharedMesh = loaded;
                applied++;
            }
            else
            {
                Debug.LogWarning($"assetが見つかりません: {original.name} / まずGenerateを実行してください");
                missing++;
            }
        }
        foreach (var mf in renderer)
        {
            if (mf.sharedMesh == null) continue;

            Mesh original = mf.sharedMesh;

            if (!_cache.TryGetValue(original, out Mesh loaded))
            {
                loaded = LoadMeshAsset(original);
                _cache[original] = loaded;
            }

            if (loaded != null)
            {
                mf.sharedMesh = loaded;
                applied++;
            }
            else
            {
                Debug.LogWarning($"assetが見つかりません: {original.name} / まずGenerateを実行してください");
                missing++;
            }
        }

        Debug.Log($"Apply完了 → 適用: {applied}個 / 未発見: {missing}個");
    }

    static Mesh CreateMeshAsset(Mesh originalMesh)
    {
        string assetPath = GetAssetPath(originalMesh);

        if (AssetDatabase.LoadAssetAtPath<Mesh>(assetPath) != null)
        {
            Debug.Log($"スキップ（既存）: {assetPath}");
            return null;
        }

        Mesh newMesh = Object.Instantiate(originalMesh);
        newMesh.name = originalMesh.name;
        TangentBaker.BakeMesh(newMesh);

        AssetDatabase.CreateAsset(newMesh, assetPath);
        Debug.Log($"生成: {assetPath}");
        return newMesh;
    }
    static Mesh LoadMeshAsset(Mesh originalMesh)
    {
        string assetPath = GetAssetPath(originalMesh);
        return AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
    }

    static IEnumerable<MeshFilter> GetAllMeshFilters()
    {
        GameObject root = Selection.activeGameObject;
        if (root == null)
        {
            Debug.LogError("ルートGameObjectを選択してください");
            yield break;
        }

        foreach (var mf in root.GetComponentsInChildren<MeshFilter>(true))
            yield return mf;
    }
    static IEnumerable<SkinnedMeshRenderer> GetAllSkinnedMeshRenderer()
    {
        GameObject root = Selection.activeGameObject;
        if (root == null)
        {
            Debug.LogError("ルートGameObjectを選択してください");
            yield break;
        }

        foreach (var mf in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            yield return mf;
    }

    static string GetSaveDir()
    {
        GameObject root = Selection.activeGameObject;
        if (root == null) return "Assets/Meshes";

        GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(root);
        string fbxPath = source != null ? AssetDatabase.GetAssetPath(source) : "";

        if (string.IsNullOrEmpty(fbxPath))
            return "Assets/Meshes";

        string dir = Path.GetDirectoryName(fbxPath);
        string name = Path.GetFileNameWithoutExtension(fbxPath);
        return Path.Combine(dir, name + "_SmoothNormals");
    }

    static void EnsureSaveDir()
    {
        string dir = SaveDir;
        if (AssetDatabase.IsValidFolder(dir)) return;

        string parent = Path.GetDirectoryName(dir);
        string folderName = Path.GetFileName(dir);
        AssetDatabase.CreateFolder(parent, folderName);
        Debug.Log($"フォルダ作成: {dir}");
    }

    static string GetAssetPath(Mesh mesh)
    {
        return Path.Combine(SaveDir, mesh.name + ".asset");
    }
}
#endif