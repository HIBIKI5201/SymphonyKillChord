using UnityEditor;
using UnityEngine;

namespace DevelopProducts.ToonShader
{
    public class SilToonEyeThroughGUI : ShaderGUI
    {
        // Foldout states
        static bool showBase = true;
        static bool showNormal = true;
        static bool showFresnel = true;
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

            public static Color headerColor = new Color(0.6f, 0.4f, 0.2f, 1.0f); // Different color for EyeThrough
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
            MaterialProperty alpha = Find("_Alpha", props);
            MaterialProperty isForFace = Find("_IsForFace", props);
            MaterialProperty faceUp = Find("_FaceUp", props);

            MaterialProperty normalMap = Find("_NormalMap", props);
            MaterialProperty normalIntensity = Find("_NormalMapIntensity", props);

            MaterialProperty fresnelBack = Find("_FresnelBackLight", props);
            MaterialProperty fresnelFront = Find("_FresnelFrontRimLight", props);
            MaterialProperty fresnelBackRim = Find("_FresnelBackRimLight", props);

            MaterialProperty perspectiveRatio = Find("_PerspectiveRemovalRatio", props);
            MaterialProperty perspectiveRadius = Find("_PerspectiveRemovalRadius", props);
            MaterialProperty head = Find("_Head", props);

            MaterialProperty stencilRef = Find("_StencilRef", props);
            MaterialProperty stencilPass = Find("_StencilPass", props);

            // ===== Sections =====

            DrawSection("Base & Transparency", ref showBase, () =>
            {
                materialEditor.TexturePropertySingleLine(new GUIContent("Base Map & Lit Color", "ベーステクスチャと明るい部分の色"), baseMap, colorLit);

                EditorGUI.indentLevel++;
                materialEditor.ShaderProperty(colorMiddle, new GUIContent("Middle Color", "中間色"));
                materialEditor.ShaderProperty(colorShadow, new GUIContent("Shadow Color", "影色"));
                materialEditor.ShaderProperty(alpha, new GUIContent("Alpha", "透過度"));
                EditorGUI.indentLevel--;

                EditorGUILayout.Space(5);
                materialEditor.ShaderProperty(isForFace, new GUIContent("Face Mode", "顔用シェーディングを有効化"));
                if (isForFace.floatValue == 1)
                {
                    EditorGUI.indentLevel++;
                    materialEditor.ShaderProperty(faceUp, new GUIContent("Face Up Direction", "顔の上方向ベクトル"));
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

            DrawSection("Perspective Removal", ref showPerspective, () =>
            {
                materialEditor.ShaderProperty(perspectiveRatio, new GUIContent("Ratio", "透視除去の強度"));
                materialEditor.ShaderProperty(perspectiveRadius, new GUIContent("Radius", "効果範囲"));
                materialEditor.VectorProperty(head, "Head Position (World)");
            });

            DrawSection("Render State & Stencil", ref showRenderState, () =>
            {
                materialEditor.ShaderProperty(stencilRef, new GUIContent("Stencil ID", "ステンシル参照値"));
                materialEditor.ShaderProperty(stencilPass, new GUIContent("Pass Operation", "成功時処理"));

                EditorGUILayout.HelpBox("This shader uses 'ZTest Always' and 'Comp Equal' for eye-through effect.", MessageType.Info);

                // ===== Footer =====
                EditorGUILayout.Space(15);
                EditorGUILayout.BeginVertical(Styles.background);
                {
                    materialEditor.RenderQueueField();
                    materialEditor.EnableInstancingField();
                    materialEditor.DoubleSidedGIField();
                }
                EditorGUILayout.EndVertical();
            });


            //EditorGUILayout.Space(5);
            //EditorGUILayout.LabelField("SilToon EyeThrough v1.0.0", EditorStyles.centeredGreyMiniLabel);
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

            EditorGUI.DrawRect(rect, new Color(0.18f, 0.15f, 0.12f, 1));

            Rect accentRect = new Rect(rect.x, rect.y, 4, rect.height);
            EditorGUI.DrawRect(accentRect, Styles.headerColor);

            GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = Color.white }
            };

            EditorGUI.LabelField(new Rect(rect.x + 15, rect.y, rect.width, rect.height), "SilToon EyeThrough", labelStyle);
            EditorGUILayout.Space(10);
        }

        private void DrawSection(string title, ref bool state, System.Action drawer)
        {
            EditorGUILayout.Space(5);

            Rect rect = EditorGUILayout.GetControlRect(true, 20);
            EditorGUI.DrawRect(new Rect(rect.x - 3, rect.y, rect.width + 6, rect.height), new Color(0.25f, 0.22f, 0.2f, 1));

            state = EditorGUI.Foldout(rect, state, title, true, Styles.header);

            if (state)
            {
                EditorGUILayout.BeginVertical(Styles.background);
                drawer();
                EditorGUILayout.EndVertical();
            }
        }
    }
}
