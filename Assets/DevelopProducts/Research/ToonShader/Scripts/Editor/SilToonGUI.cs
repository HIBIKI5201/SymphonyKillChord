using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace DevelopProducts.ToonShader
{
    public class SilToonGUI : ShaderGUI
    {
        // Foldout states (static to persist across selections)
        static bool showBase = true;
        static bool showNormal = true;
        static bool showFresnel = true;
        static bool showOutline = true;
        static bool showPerspective = true;
        static bool showRenderState = false;

        private static class Styles
        {
            public static GUIStyle header = new GUIStyle("ShurikenModuleTitle")
            {
                fontStyle = FontStyle.Bold,
                fixedHeight = 20,
                contentOffset = new Vector2(20, -2)
            };

            public static GUIStyle background = new GUIStyle("HelpBox")
            {
                padding = new RectOffset(10, 10, 5, 5)
            };

            public static Color headerColor = new Color(0.2f, 0.4f, 0.6f, 1.0f);
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
        {
            // ===== Banner / Header =====
            DrawBanner();

            // ===== Property Discovery =====
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

            // ===== Sections =====

            DrawSection("Base & Lighting", ref showBase, () =>
            {
                materialEditor.TexturePropertySingleLine(new GUIContent("Base Map & Lit Color", "ベーステクスチャと明るい部分の色"), baseMap, colorLit);

                EditorGUI.indentLevel++;
                materialEditor.ShaderProperty(colorMiddle, new GUIContent("Middle Color", "中間色 (LitとShadowの間)"));
                materialEditor.ShaderProperty(colorShadow, new GUIContent("Shadow Color", "影色"));
                EditorGUI.indentLevel--;

                EditorGUILayout.Space(5);
                materialEditor.ShaderProperty(isForFace, new GUIContent("Face Mode", "顔用シェーディングを有効化"));
                if (isForFace.floatValue == 1)
                {
                    EditorGUI.indentLevel++;
                    materialEditor.ShaderProperty(faceUp, new GUIContent("Face Up Direction", "顔の上方向ベクトル (ワールド空間)"));
                    EditorGUI.indentLevel--;
                }
            });

            DrawSection("Normal Mapping", ref showNormal, () =>
            {
                materialEditor.TexturePropertySingleLine(new GUIContent("Normal Map", "法線マップ"), normalMap);
                if (normalMap.textureValue != null)
                {
                    EditorGUI.indentLevel++;
                    materialEditor.ShaderProperty(normalIntensity, new GUIContent("Intensity", "法線の適用強度"));
                    EditorGUI.indentLevel--;
                }
            });

            DrawSection("Fresnel & Rim Light", ref showFresnel, () =>
            {
                materialEditor.ShaderProperty(fresnelBack, new GUIContent("Back Light Intensity", "背面からの回り込み光強度"));
                materialEditor.ShaderProperty(fresnelFront, new GUIContent("Front Rim Intensity", "正面エッジのリムライト強度"));
                materialEditor.ShaderProperty(fresnelBackRim, new GUIContent("Back Rim Intensity", "背面エッジのリムライト強度"));
            });

            DrawSection("Outline Settings", ref showOutline, () =>
            {
                materialEditor.ShaderProperty(outlineColor, new GUIContent("Color", "アウトラインの色"));
                materialEditor.ShaderProperty(zOffset, new GUIContent("Z Offset", "アウトラインの奥行きオフセット (めり込み防止)"));
                materialEditor.ShaderProperty(smoothNormal, new GUIContent("Smooth Normal", "スムーズ法線を使用して境界を滑らかにする"));

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Outline Width", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                materialEditor.ShaderProperty(outlineWidthLit, new GUIContent("Width (Lit)", "明部の太さ"));
                materialEditor.ShaderProperty(outlineWidthShadow, new GUIContent("Width (Shadow)", "影部の太さ"));
                EditorGUI.indentLevel--;
            });

            DrawSection("Perspective Removal", ref showPerspective, () =>
            {
                materialEditor.ShaderProperty(perspectiveRatio, new GUIContent("Ratio", "透視除去(パース抜き)の強度"));
                materialEditor.ShaderProperty(perspectiveRadius, new GUIContent("Radius", "効果が及ぶ半径"));
                materialEditor.VectorProperty(head, "Head Position (World)");
            });

            DrawSection("Render State & Stencil", ref showRenderState, () =>
            {
                EditorGUILayout.LabelField("Presets", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("通常 (Default)"))
                {
                    SetStencil(
                        materialEditor,
                        stencilRef,
                        1,
                        stencilComp,
                        CompareFunction.Disabled,
                        stencilPass,
                        StencilOp.Keep,
                        stencilFail,
                        StencilOp.Keep,
                        2000);
                }
                if (GUILayout.Button("透過処理（目・眉毛）"))
                {
                    SetStencil(
                        materialEditor,
                        stencilRef,
                        1,
                        stencilComp,
                        CompareFunction.Always,
                        stencilPass,
                        StencilOp.Replace,
                        stencilFail,
                        StencilOp.Keep,
                        2005
                    );
                }
                if (GUILayout.Button("透過処理（髪）"))
                {
                    SetStencil(
                        materialEditor,
                        stencilRef,
                        1,
                        stencilComp,
                        CompareFunction.Always,
                        stencilPass,
                        StencilOp.Keep,
                        stencilFail,
                        StencilOp.Zero,
                        2010
                    );
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(5);

                materialEditor.ShaderProperty(stencilRef, new GUIContent("Stencil ID", "ステンシル参照値 (0-255)"));
                materialEditor.ShaderProperty(stencilComp, new GUIContent("Compare Function", "ステンシル比較関数"));
                materialEditor.ShaderProperty(stencilPass, new GUIContent("Pass Operation", "ステンシル成功時の処理"));
                materialEditor.ShaderProperty(stencilFail, new GUIContent("Fail Operation", "ステンシル失敗時の処理"));
            });

            // ===== Footer =====
            EditorGUILayout.Space(15);
            EditorGUILayout.BeginVertical(Styles.background);
            {
                materialEditor.RenderQueueField();
                materialEditor.EnableInstancingField();
                materialEditor.DoubleSidedGIField();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("SilToon v1.0.1", EditorStyles.centeredGreyMiniLabel);
        }

        // ===== Helper Methods =====

        private MaterialProperty Find(string name, MaterialProperty[] props)
        {
            return FindProperty(name, props);
        }

        private void DrawBanner()
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 30);
            rect.xMin -= 20;
            rect.xMax += 20;

            EditorGUI.DrawRect(rect, new Color(0.15f, 0.15f, 0.15f, 1));

            Rect accentRect = new Rect(rect.x, rect.y, 4, rect.height);
            EditorGUI.DrawRect(accentRect, Styles.headerColor);

            GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = Color.white }
            };

            EditorGUI.LabelField(new Rect(rect.x + 15, rect.y, rect.width, rect.height), "SilToon Shader", labelStyle);
            EditorGUILayout.Space(10);
        }

        private void DrawSection(string title, ref bool state, System.Action drawer)
        {
            EditorGUILayout.Space(5);

            Rect rect = EditorGUILayout.GetControlRect(true, 20);
            EditorGUI.DrawRect(new Rect(rect.x - 3, rect.y, rect.width + 6, rect.height), new Color(0.2f, 0.2f, 0.2f, 1));

            state = EditorGUI.Foldout(rect, state, title, true, Styles.header);

            if (state)
            {
                EditorGUILayout.BeginVertical(Styles.background);
                drawer();
                EditorGUILayout.EndVertical();
            }
        }

        private void SetStencil(MaterialEditor editor, MaterialProperty pRef, float vRef, MaterialProperty pComp, CompareFunction vComp, MaterialProperty pPass, StencilOp vPass, MaterialProperty pFail, StencilOp vFail, int queue)
        {
            editor.RegisterPropertyChangeUndo("Set Stencil Template");
            pRef.floatValue = vRef;
            if (pComp != null) pComp.floatValue = (float)vComp;
            if (pPass != null) pPass.floatValue = (float)vPass;
            if (pFail != null) pFail.floatValue = (float)vFail;

            foreach (var target in editor.targets)
            {
                if (target is Material mat)
                {
                    mat.renderQueue = queue;
                }
            }
        }
    }
}
