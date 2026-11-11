using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace KillChord.Editor.MeshMerge
{
    /// <summary>
    ///     複数のSkinnedMeshRendererのメッシュを、スキニング結果を変えずに1つのメッシュへ結合します。
    /// </summary>
    public static class SkinnedMeshCombiner
    {
        /// <summary>
        ///     指定したRendererのメッシュを1つのスキンメッシュへ結合します。
        ///     頂点は各メッシュの元の座標のまま並べ、ボーンは(Transform, バインドポーズ)の組で統合します。
        /// </summary>
        /// <param name="sources"> 結合元のRendererです。 </param>
        /// <param name="meshName"> 結合後のメッシュ名です。 </param>
        /// <param name="bones"> 結合後のメッシュに対応するボーン配列です。 </param>
        /// <returns> 結合したメッシュです。結合できない場合はnullです。 </returns>
        public static Mesh Combine(IReadOnlyList<SkinnedMeshRenderer> sources, string meshName, out Transform[] bones)
        {
            bones = null;
            CombineBuffer buffer = new();

            foreach (SkinnedMeshRenderer source in sources)
            {
                if (!TryAppend(buffer, source))
                {
                    return null;
                }
            }

            bones = buffer.Bones.ToArray();
            return BuildMesh(buffer, meshName);
        }

        private const int UV_CHANNEL_COUNT = 4;
        private const int MAX_UINT16_VERTEX_COUNT = 65535;

        /// <summary>
        ///     1つのRendererのメッシュを結合バッファへ追加します。
        /// </summary>
        private static bool TryAppend(CombineBuffer buffer, SkinnedMeshRenderer source)
        {
            Mesh mesh = source.sharedMesh;
            int vertexCount = mesh.vertexCount;
            int offset = buffer.Positions.Count;

            // 頂点属性が足りないメッシュは、結合後に頂点数がずれるため扱わない。
            if (mesh.GetBonesPerVertex().Length != vertexCount)
            {
                Debug.LogError($"[{nameof(SkinnedMeshCombiner)}] スキンウェイトが無いメッシュは結合できません: {mesh.name}", source);
                return false;
            }

            int[] boneRemap = BuildBoneRemap(buffer, source);
            if (boneRemap == null)
            {
                return false;
            }

            AppendAttributes(buffer, mesh, vertexCount);
            AppendSkin(buffer, mesh, boneRemap);

            foreach (int index in mesh.GetTriangles(0))
            {
                buffer.Triangles.Add(index + offset);
            }

            buffer.EncapsulateBounds(mesh.bounds);
            return true;
        }

        /// <summary>
        ///     メッシュ固有のボーン番号を、結合後のボーン番号へ変換する表を作ります。
        /// </summary>
        private static int[] BuildBoneRemap(CombineBuffer buffer, SkinnedMeshRenderer source)
        {
            Transform[] sourceBones = source.bones;
            Matrix4x4[] bindPoses = source.sharedMesh.bindposes;
            int[] remap = new int[sourceBones.Length];

            for (int i = 0; i < sourceBones.Length; i++)
            {
                if (sourceBones[i] == null)
                {
                    Debug.LogError($"[{nameof(SkinnedMeshCombiner)}] 未設定のボーンがあるため結合できません: {source.name}", source);
                    return null;
                }

                (Transform, Matrix4x4) key = (sourceBones[i], bindPoses[i]);
                if (!buffer.BoneIndices.TryGetValue(key, out int index))
                {
                    index = buffer.Bones.Count;
                    buffer.Bones.Add(sourceBones[i]);
                    buffer.BindPoses.Add(bindPoses[i]);
                    buffer.BoneIndices.Add(key, index);
                }
                remap[i] = index;
            }
            return remap;
        }

        /// <summary>
        ///     頂点・法線・接線・UV・頂点カラーを結合バッファへ追加します。
        /// </summary>
        private static void AppendAttributes(CombineBuffer buffer, Mesh mesh, int vertexCount)
        {
            buffer.Positions.AddRange(mesh.vertices);

            Vector3[] normals = mesh.normals;
            buffer.Normals.AddRange(normals.Length == vertexCount ? normals : new Vector3[vertexCount]);

            Vector4[] tangents = mesh.tangents;
            buffer.HasTangents |= tangents.Length == vertexCount;
            buffer.Tangents.AddRange(tangents.Length == vertexCount ? tangents : new Vector4[vertexCount]);

            Color[] colors = mesh.colors;
            bool hasColors = colors.Length == vertexCount;
            buffer.HasColors |= hasColors;
            for (int i = 0; i < vertexCount; i++)
            {
                buffer.Colors.Add(hasColors ? colors[i] : Color.white);
            }

            List<Vector2> uvs = new();
            for (int channel = 0; channel < UV_CHANNEL_COUNT; channel++)
            {
                uvs.Clear();
                mesh.GetUVs(channel, uvs);
                bool hasUv = uvs.Count == vertexCount;
                buffer.HasUvs[channel] |= hasUv;
                buffer.Uvs[channel].AddRange(hasUv ? uvs : new List<Vector2>(new Vector2[vertexCount]));
            }
        }

        /// <summary>
        ///     ボーンウェイトをボーン番号の変換後に結合バッファへ追加します。
        /// </summary>
        private static void AppendSkin(CombineBuffer buffer, Mesh mesh, int[] boneRemap)
        {
            buffer.BonesPerVertex.AddRange(mesh.GetBonesPerVertex());
            foreach (BoneWeight1 weight in mesh.GetAllBoneWeights())
            {
                BoneWeight1 remapped = weight;
                remapped.boneIndex = boneRemap[weight.boneIndex];
                buffer.Weights.Add(remapped);
            }
        }

        /// <summary>
        ///     結合バッファからメッシュを生成します。
        /// </summary>
        private static Mesh BuildMesh(CombineBuffer buffer, string meshName)
        {
            Mesh mesh = new() { name = meshName };
            if (buffer.Positions.Count > MAX_UINT16_VERTEX_COUNT)
            {
                mesh.indexFormat = IndexFormat.UInt32;
            }

            mesh.SetVertices(buffer.Positions);
            mesh.SetNormals(buffer.Normals);
            if (buffer.HasTangents)
            {
                mesh.SetTangents(buffer.Tangents);
            }
            if (buffer.HasColors)
            {
                mesh.SetColors(buffer.Colors);
            }
            for (int channel = 0; channel < UV_CHANNEL_COUNT; channel++)
            {
                if (buffer.HasUvs[channel])
                {
                    mesh.SetUVs(channel, buffer.Uvs[channel]);
                }
            }
            mesh.SetTriangles(buffer.Triangles, 0);
            mesh.bindposes = buffer.BindPoses.ToArray();

            using NativeArray<byte> bonesPerVertex = new(buffer.BonesPerVertex.ToArray(), Allocator.Temp);
            using NativeArray<BoneWeight1> weights = new(buffer.Weights.ToArray(), Allocator.Temp);
            mesh.SetBoneWeights(bonesPerVertex, weights);

            mesh.bounds = buffer.Bounds;
            return mesh;
        }

        /// <summary>
        ///     結合中の頂点データを保持するバッファです。
        /// </summary>
        private sealed class CombineBuffer
        {
            public readonly List<Vector3> Positions = new();
            public readonly List<Vector3> Normals = new();
            public readonly List<Vector4> Tangents = new();
            public readonly List<Color> Colors = new();
            public readonly List<Vector2>[] Uvs = { new(), new(), new(), new() };
            public readonly bool[] HasUvs = new bool[UV_CHANNEL_COUNT];
            public readonly List<int> Triangles = new();
            public readonly List<byte> BonesPerVertex = new();
            public readonly List<BoneWeight1> Weights = new();
            public readonly List<Transform> Bones = new();
            public readonly List<Matrix4x4> BindPoses = new();
            public readonly Dictionary<(Transform, Matrix4x4), int> BoneIndices = new();
            public Bounds Bounds;
            public bool HasTangents;
            public bool HasColors;

            /// <summary>
            ///     境界を拡張します。最初の1回は境界そのものを採用します。
            /// </summary>
            public void EncapsulateBounds(Bounds bounds)
            {
                if (!_hasBounds)
                {
                    Bounds = bounds;
                    _hasBounds = true;
                    return;
                }
                Bounds.Encapsulate(bounds);
            }

            private bool _hasBounds;
        }
    }
}
