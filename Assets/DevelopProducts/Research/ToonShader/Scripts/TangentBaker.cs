using System.Collections.Generic;
using Unity.Collections;
using UnityEditor;
using UnityEngine;

public static class TangentBaker
{
    [MenuItem("Tools/Get All Project Meshes")]
    public static void GetAllProjectMeshes()
    {
        string[] guids = AssetDatabase.FindAssets("t:Mesh");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);

            if (mesh != null)
            {
                Debug.Log($"Asset Mesh: {mesh.name} at {path}");
                BakeMesh(mesh);
            }
        }
    }

    public static void BakeMesh(Mesh mesh)
    {
        if (mesh == null) return;

        var vertices = mesh.vertices;
        var normals = mesh.normals;

        if (!(mesh.vertices.Length == mesh.normals.Length && mesh.vertices.Length == mesh.tangents.Length))
        {
            Debug.LogError($"ignore:{mesh.name}, vertices:{mesh.vertices.Length}, normals:{mesh.normals.Length}, tangents:{mesh.tangents.Length}", mesh);
            return;
        }


        // 同位置の頂点をグループ化してスムース法線を計算
        NativeArray<Vector3> smoothNormals = new NativeArray<Vector3>(normals, Allocator.Temp);
        CalcSmoothNormals(mesh, ref smoothNormals);

        // Tangent のXYZにスムース法線を格納（Wは符号なので1固定）
        var uv = new Vector2[vertices.Length];

        CalcVectorToTangetToUV(mesh, smoothNormals, ref uv);

        mesh.uv4 = uv; // ← channel1に書き込み

        Debug.Log($"[SmoothNormal] Baked: {mesh.name}", mesh);
    }
    private static void CalcSmoothNormals(Mesh mesh, ref NativeArray<Vector3> smoothNormals)
    {
        _posToSmoothNormal.Clear();
        var vertices = mesh.vertices;
        var normals = mesh.normals;

        var posToNormals = _posToSmoothNormal;
        for (int i = 0; i < vertices.Length; i++)
        {
            var v = vertices[i];
            if (!posToNormals.ContainsKey(v))
                posToNormals[v] = Vector3.zero;
            posToNormals[v] += normals[i];
        }

        for (int i = 0; i < vertices.Length; i++)
        {
            smoothNormals[i] = posToNormals[vertices[i]].normalized;
        }
    }

    private static void CalcVectorToTangetToUV(Mesh mesh, in NativeArray<Vector3> smoothNormals, ref Vector2[] result)
    {
        var normals = mesh.normals;
        var tangents = mesh.tangents;
        var uv = result;

        for (int i = 0; i < normals.Length; i++)
        {
            var N = normals[i];
            var T = (Vector3)tangents[i];
            var B = Vector3.Cross(N, T) * tangents[i].w;

            // タンジェント空間に変換
            var sn = smoothNormals[i];
            var snTS = new Vector3(
                Vector3.Dot(sn, T),
                Vector3.Dot(sn, B),
                Vector3.Dot(sn, N)
            );
            snTS = snTS.normalized;

            // XYだけ格納（Zはシェーダーで復元）
            uv[i] = new Vector2(snTS.x, snTS.y);
        }
    }

    private static Dictionary<Vector3, Vector3> _posToSmoothNormal = new();
}