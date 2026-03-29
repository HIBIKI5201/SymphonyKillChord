using UnityEditor;
using UnityEngine;

namespace DevelopProducts.ToonShader
{
    public class SilToonGUI : ShaderGUI
    {
        // Foldout状態（staticで保持）
        static bool showBase = true;
        static bool showNormal = true;
        static bool showFresnel = true;
        static bool showOutline = true;
        static bool showPerspective = true;
        static bool showRenderState = true;

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
        {
            // ===== プロパティ取得 =====
            MaterialProperty baseMap = Find("_BaseMap", props);
            MaterialProperty colorLit = Find("_ColorLit", props);
            MaterialProperty colorMiddle = Find("_ColorMiddle", props);
            MaterialProperty colorShadow = Find("_ColorShadow", props);
            MaterialProperty isForFace = Find("_IsForFace", props);
            MaterialProperty faceUp = Find("_FaceUp", props);

            MaterialProperty normalMap = Find("_NormalMap", props);
            MaterialProperty normalIntensity = Find("_NormalMapIntensity", props);

            MaterialProperty fresnelBack = Find("_FresnelBackLight", props);
            MaterialProperty fresnelFront = Find("_FresnelFrontRimLight", props);
            MaterialProperty fresnelBackRim = Find("_FresnelBackRimLight", props);

            MaterialProperty outlineColor = Find("_OutlineColor", props);
            MaterialProperty zOffset = Find("_ZOffset", props);
            MaterialProperty smoothNormal = Find("_IsSmoothNormal", props);
            MaterialProperty outlineWidthLit = Find("_OutlineWidthLit", props);
            MaterialProperty outlineWidthShadow = Find("_OutlineWidthShadow", props);

            MaterialProperty perspectiveRatio = Find("_PerspectiveRemovalRatio", props);
            MaterialProperty perspectiveRadius = Find("_PerspectiveRemovalRadius", props);
            MaterialProperty head = Find("_Head", props);

            MaterialProperty stencilRef = Find("_StencilRef", props);
            MaterialProperty stencilComp = Find("_StencilComp", props);
            MaterialProperty stencilPass = Find("_StencilPass", props);
            MaterialProperty stencilFail = Find("_StencilFail", props);

            // ===== UI描画 =====

            DrawGroup("Base / Lighting", ref showBase, () =>
            {
                materialEditor.TexturePropertySingleLine(
                    new GUIContent("Base Map", "ベーステクスチャ"),
                    baseMap,
                    colorLit
                );

                materialEditor.ShaderProperty(colorMiddle, new GUIContent("Middle Color", "中間色"));
                materialEditor.ShaderProperty(colorShadow, new GUIContent("Shadow Color", "影色"));

                materialEditor.ShaderProperty(isForFace, new GUIContent("Face Mode", "顔用シェーディングを有効化"));

                if (isForFace.floatValue == 1)
                {
                    EditorGUI.indentLevel++;
                    materialEditor.ShaderProperty(faceUp, new GUIContent("Face Up", "顔の上方向ベクトル"));
                    EditorGUI.indentLevel--;
                }
            });

            DrawGroup("Normal", ref showNormal, () =>
            {
                materialEditor.TexturePropertySingleLine(
                    new GUIContent("Normal Map", "法線マップ"),
                    normalMap
                );

                materialEditor.ShaderProperty(normalIntensity, new GUIContent("Intensity", "法線の強さ"));
            });

            DrawGroup("Fresnel / LimLight", ref showFresnel, () =>
            {
                materialEditor.ShaderProperty(fresnelBack, new GUIContent("Back Light", "背面リムライト強度"));
                materialEditor.ShaderProperty(fresnelFront, new GUIContent("Front Rim", "正面リムライト強度"));
                materialEditor.ShaderProperty(fresnelBackRim, new GUIContent("Back Rim", "背面リム強度"));
            });

            DrawGroup("Outline", ref showOutline, () =>
            {
                materialEditor.ShaderProperty(outlineColor, new GUIContent("Color", "アウトライン色"));
                materialEditor.ShaderProperty(zOffset, new GUIContent("Z Offset", "奥行き補正"));

                materialEditor.ShaderProperty(smoothNormal, new GUIContent("Smooth Normal", "スムーズ法線を使用"));

                materialEditor.ShaderProperty(outlineWidthLit, new GUIContent("Width Lit", "明部の太さ"));
                materialEditor.ShaderProperty(outlineWidthShadow, new GUIContent("Width Shadow", "影部の太さ"));
            });

            DrawGroup("Perspective Removal", ref showPerspective, () =>
            {
                materialEditor.ShaderProperty(perspectiveRatio, new GUIContent("Ratio", "透視除去の強さ"));
                materialEditor.ShaderProperty(perspectiveRadius, new GUIContent("Radius", "影響範囲"));
                materialEditor.ShaderProperty(head, new GUIContent("Head Position", "基準位置"));
            });

            DrawGroup("Render State", ref showRenderState, () =>
            {
                materialEditor.ShaderProperty(stencilRef, new GUIContent("Stencil ID", "ステンシル参照値"));
                materialEditor.ShaderProperty(stencilComp, new GUIContent("Compare", "比較関数"));
                materialEditor.ShaderProperty(stencilPass, new GUIContent("Pass", "成功時処理"));
                materialEditor.ShaderProperty(stencilFail, new GUIContent("Fail", "失敗時処理"));
            });

            EditorGUILayout.Space(10);

            materialEditor.RenderQueueField();
            materialEditor.EnableInstancingField();
            materialEditor.DoubleSidedGIField();
        }

        // ===== ヘルパー =====

        MaterialProperty Find(string name, MaterialProperty[] props)
        {
            return FindProperty(name, props);
        }

        void DrawGroup(string title, ref bool state, System.Action drawer)
        {
            EditorGUILayout.Space(5);

            state = EditorGUILayout.BeginFoldoutHeaderGroup(state, title);

            if (state)
            {
                EditorGUI.indentLevel++;
                drawer();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}
