using KillChord.Editor.SourceDataProvider.Wiki;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KillChord.Editor.Inspectors.SourceData
{
    /// <summary>
    ///     バトルシーンのNavMeshとスポーンポイントを描画します。
    /// </summary>
    internal static class BattleSceneMapRenderer
    {
        /// <summary>
        ///     シーン名からマップ情報を読み込み、NavMeshとスポーンポイントを描画します。
        /// </summary>
        /// <param name="sceneName"> 描画対象のシーン名です。 </param>
        /// <param name="highlightedHashIds"> 強調表示するスポーンポイントのハッシュIDです。 </param>
        /// <param name="onSpawnPointClicked"> スポーンポイントがクリックされたときの処理です。 </param>
        /// <returns> マップ情報を描画できた場合はtrueを返します。 </returns>
        public static bool Draw(
            string sceneName,
            HashSet<int> highlightedHashIds = null,
            Action<BattleSceneDataReader.SpawnPointInfo> onSpawnPointClicked = null)
        {
            if (!BattleSceneDataReader.TryRead(sceneName, out BattleSceneDataReader.BattleSceneMapData mapData, out string error))
            {
                EditorGUILayout.HelpBox(error, MessageType.Warning);
                return false;
            }

            if (!string.IsNullOrEmpty(error))
            {
                EditorGUILayout.HelpBox(error, MessageType.Warning);
            }

            return Draw(mapData, highlightedHashIds, onSpawnPointClicked);
        }

        /// <summary>
        ///     読み込み済みのマップ情報からNavMeshとスポーンポイントを描画します。
        /// </summary>
        /// <param name="mapData"> 描画するバトルシーンのマップ情報です。 </param>
        /// <param name="highlightedHashIds"> 強調表示するスポーンポイントのハッシュIDです。 </param>
        /// <param name="onSpawnPointClicked"> スポーンポイントがクリックされたときの処理です。 </param>
        /// <returns> マップ情報を描画できた場合はtrueを返します。 </returns>
        public static bool Draw(
            BattleSceneDataReader.BattleSceneMapData mapData,
            HashSet<int> highlightedHashIds = null,
            Action<BattleSceneDataReader.SpawnPointInfo> onSpawnPointClicked = null)
        {
            if (mapData == null)
            {
                return false;
            }

            if (mapData.SpawnPoints.Count == 0 && !mapData.HasNavMesh)
            {
                EditorGUILayout.HelpBox("スポーンポイント・NavMeshともにありません。", MessageType.None);
                return true;
            }

            if (!TryCalculateBounds(mapData, out Rect worldBoundsXZ))
            {
                EditorGUILayout.HelpBox("マップ範囲を計算できません。", MessageType.None);
                return true;
            }

            Rect canvasRect = GUILayoutUtility.GetRect(
                CANVAS_MIN_WIDTH,
                CANVAS_HEIGHT,
                GUILayout.ExpandWidth(true),
                GUILayout.Height(CANVAS_HEIGHT));
            EditorGUI.DrawRect(canvasRect, BACKGROUND_COLOR);

            CoordinateMapper mapper = new(worldBoundsXZ, canvasRect, CANVAS_PADDING);

            if (mapData.HasNavMesh)
            {
                DrawNavMesh(mapData.NavMesh, mapper);
            }

            DrawSpawnPoints(mapData.SpawnPoints, mapper, highlightedHashIds, onSpawnPointClicked);

            return true;
        }

        private const float CANVAS_HEIGHT = 420f;
        private const float CANVAS_MIN_WIDTH = 320f;
        private const float CANVAS_PADDING = 24f;
        private const float MIN_BOUNDS_SIZE = 1f;
        private const float SPAWN_POINT_BUTTON_SIZE = 14f;
        private const float SPAWN_POINT_LABEL_WIDTH = 96f;
        private const float SPAWN_POINT_LABEL_HEIGHT = 14f;

        private static readonly Color BACKGROUND_COLOR = new(0.14f, 0.14f, 0.16f, 1f);
        private static readonly Color NAVMESH_COLOR = new(0.35f, 0.55f, 0.9f, 0.35f);
        private static readonly Color SPAWN_POINT_COLOR = new(0.78f, 0.58f, 0.88f, 1f);
        private static readonly Color HIGHLIGHTED_SPAWN_POINT_COLOR = new(1f, 0.78f, 0.28f, 1f);

        /// <summary>
        ///     マップ情報を描画するためのXZ平面上の範囲を計算します。
        /// </summary>
        /// <param name="mapData"> 範囲を計算するマップ情報です。 </param>
        /// <param name="boundsXZ"> 計算したXZ平面上の範囲です。 </param>
        /// <returns> 範囲を計算できた場合はtrueを返します。 </returns>
        private static bool TryCalculateBounds(BattleSceneDataReader.BattleSceneMapData mapData, out Rect boundsXZ)
        {
            bool hasPoint = false;
            float minX = float.MaxValue, maxX = float.MinValue;
            float minZ = float.MaxValue, maxZ = float.MinValue;

            foreach (BattleSceneDataReader.SpawnPointInfo sp in mapData.SpawnPoints)
            {
                Expand(sp.SpawnPosition, ref minX, ref maxX, ref minZ, ref maxZ);
                Expand(sp.EntryPosition, ref minX, ref maxX, ref minZ, ref maxZ);
                hasPoint = true;
            }

            if (mapData.HasNavMesh)
            {
                Vector3[] vertices = mapData.NavMesh.Vertices;
                for (int i = 0; i < vertices.Length; i++)
                {
                    Expand(vertices[i], ref minX, ref maxX, ref minZ, ref maxZ);
                    hasPoint = true;
                }
            }

            if (!hasPoint)
            {
                boundsXZ = default;
                return false;
            }

            if (maxX - minX < MIN_BOUNDS_SIZE)
            {
                float centerX = (minX + maxX) * 0.5f;
                minX = centerX - MIN_BOUNDS_SIZE * 0.5f;
                maxX = centerX + MIN_BOUNDS_SIZE * 0.5f;
            }

            if (maxZ - minZ < MIN_BOUNDS_SIZE)
            {
                float centerZ = (minZ + maxZ) * 0.5f;
                minZ = centerZ - MIN_BOUNDS_SIZE * 0.5f;
                maxZ = centerZ + MIN_BOUNDS_SIZE * 0.5f;
            }

            boundsXZ = new Rect(minX, minZ, maxX - minX, maxZ - minZ);
            return true;
        }

        /// <summary>
        ///     ワールド座標を含むようにXZ平面上の範囲を拡張します。
        /// </summary>
        /// <param name="worldPosition"> 範囲に含めるワールド座標です。 </param>
        /// <param name="minX"> X座標の最小値です。 </param>
        /// <param name="maxX"> X座標の最大値です。 </param>
        /// <param name="minZ"> Z座標の最小値です。 </param>
        /// <param name="maxZ"> Z座標の最大値です。 </param>
        private static void Expand(Vector3 worldPosition, ref float minX, ref float maxX, ref float minZ, ref float maxZ)
        {
            minX = Mathf.Min(minX, worldPosition.x);
            maxX = Mathf.Max(maxX, worldPosition.x);
            minZ = Mathf.Min(minZ, worldPosition.z);
            maxZ = Mathf.Max(maxZ, worldPosition.z);
        }

        /// <summary>
        ///     NavMeshの三角形をキャンバスへ描画します。
        /// </summary>
        /// <param name="navMesh"> 描画するNavMesh情報です。 </param>
        /// <param name="mapper"> ワールド座標をキャンバス座標へ変換するマッパーです。 </param>
        private static void DrawNavMesh(BattleSceneDataReader.NavMeshMapData navMesh, CoordinateMapper mapper)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            Vector3[] vertices = navMesh.Vertices;
            int[] indices = navMesh.Indices;
            Handles.color = NAVMESH_COLOR;
            for (int i = 0; i + 2 < indices.Length; i += 3)
            {
                Vector3 p0 = mapper.ToCanvas(vertices[indices[i]]);
                Vector3 p1 = mapper.ToCanvas(vertices[indices[i + 1]]);
                Vector3 p2 = mapper.ToCanvas(vertices[indices[i + 2]]);
                Handles.DrawAAConvexPolygon(p0, p1, p2);
            }
        }

        /// <summary>
        ///     スポーンポイントのボタンとラベルをキャンバスへ描画します。
        /// </summary>
        /// <param name="spawnPoints"> 描画するスポーンポイント情報です。 </param>
        /// <param name="mapper"> ワールド座標をキャンバス座標へ変換するマッパーです。 </param>
        /// <param name="highlightedHashIds"> 強調表示するスポーンポイントのハッシュIDです。 </param>
        /// <param name="onSpawnPointClicked"> スポーンポイントがクリックされたときの処理です。 </param>
        private static void DrawSpawnPoints(
            IReadOnlyList<BattleSceneDataReader.SpawnPointInfo> spawnPoints,
            CoordinateMapper mapper,
            HashSet<int> highlightedHashIds,
            Action<BattleSceneDataReader.SpawnPointInfo> onSpawnPointClicked)
        {
            Color previousBackgroundColor = GUI.backgroundColor;
            foreach (BattleSceneDataReader.SpawnPointInfo sp in spawnPoints)
            {
                Vector2 canvasPosition = mapper.ToCanvas(sp.SpawnPosition);
                Rect buttonRect = new(
                    canvasPosition.x - SPAWN_POINT_BUTTON_SIZE * 0.5f,
                    canvasPosition.y - SPAWN_POINT_BUTTON_SIZE * 0.5f,
                    SPAWN_POINT_BUTTON_SIZE,
                    SPAWN_POINT_BUTTON_SIZE);

                bool isHighlighted = highlightedHashIds != null && highlightedHashIds.Contains(sp.HashId);
                GUI.backgroundColor = isHighlighted ? HIGHLIGHTED_SPAWN_POINT_COLOR : SPAWN_POINT_COLOR;
                if (GUI.Button(buttonRect, new GUIContent(string.Empty, sp.Id), EditorStyles.miniButton))
                {
                    onSpawnPointClicked?.Invoke(sp);
                }

                Rect labelRect = new(
                    canvasPosition.x + SPAWN_POINT_BUTTON_SIZE * 0.5f,
                    canvasPosition.y - SPAWN_POINT_LABEL_HEIGHT * 0.5f,
                    SPAWN_POINT_LABEL_WIDTH,
                    SPAWN_POINT_LABEL_HEIGHT);
                GUI.Label(labelRect, sp.Id, EditorStyles.miniLabel);
            }

            GUI.backgroundColor = previousBackgroundColor;
        }

        /// <summary>
        ///     ワールド座標を描画キャンバスの座標へ変換します。
        /// </summary>
        private readonly struct CoordinateMapper
        {
            /// <summary>
            ///     座標変換に使用する描画範囲とスケールを初期化します。
            /// </summary>
            /// <param name="worldBoundsXZ"> XZ平面上のワールド範囲です。 </param>
            /// <param name="canvasRect"> 描画先のキャンバス範囲です。 </param>
            /// <param name="padding"> キャンバス内側の余白です。 </param>
            public CoordinateMapper(Rect worldBoundsXZ, Rect canvasRect, float padding)
            {
                _worldBoundsXZ = worldBoundsXZ;
                _canvasRect = canvasRect;

                float availableWidth = Mathf.Max(1f, canvasRect.width - padding * 2f);
                float availableHeight = Mathf.Max(1f, canvasRect.height - padding * 2f);
                float scaleX = availableWidth / worldBoundsXZ.width;
                float scaleZ = availableHeight / worldBoundsXZ.height;
                _scale = Mathf.Min(scaleX, scaleZ);
            }

            /// <summary>
            ///     ワールド座標をキャンバス座標へ変換します。
            /// </summary>
            /// <param name="worldPosition"> 変換するワールド座標です。 </param>
            /// <returns> 変換後のキャンバス座標です。 </returns>
            public Vector3 ToCanvas(Vector3 worldPosition)
            {
                float drawnWidth = _worldBoundsXZ.width * _scale;
                float drawnHeight = _worldBoundsXZ.height * _scale;
                float offsetX = _canvasRect.x + (_canvasRect.width - drawnWidth) * 0.5f;
                float offsetY = _canvasRect.y + (_canvasRect.height - drawnHeight) * 0.5f;

                float x = offsetX + (worldPosition.x - _worldBoundsXZ.xMin) * _scale;
                float y = offsetY + (_worldBoundsXZ.yMax - worldPosition.z) * _scale;
                return new Vector3(x, y, 0f);
            }

            private readonly Rect _worldBoundsXZ;
            private readonly Rect _canvasRect;
            private readonly float _scale;
        }
    }
}
