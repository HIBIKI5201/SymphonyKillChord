using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace DevelopProducts.ToonShader
{
    public class SilToonGUI : ShaderGUI
    {
        // Foldout states (static to persist across selections)
        static bool showBase = true;
        static bool showFade = true;
        static bool showNormal = true;
        static bool showFresnel = true;
        static bool showPBR = true;
        static bool showSSS = true;
        static bool showOutline = true;
        static bool showSmears = true;
        static bool showPerspective = true;
        static bool showFakeShadow = true;
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
            MaterialProperty charShadowOn = Find("_CharShadowOn", props);

            MaterialProperty _fadeAlpha = Find("_FadeAlpha", props);
            MaterialProperty _fadeOn = Find("_FadeOn", props);

            MaterialProperty normalMap = Find("_NormalMap", props);
            MaterialProperty normalIntensity = Find("_NormalMapIntensity", props);

            MaterialProperty fresnelBack = Find("_FresnelBackLight", props);
            MaterialProperty fresnelFront = Find("_FresnelFrontRimLight", props);
            MaterialProperty fresnelBackRim = Find("_FresnelBackRimLight", props);

            MaterialProperty pbrOn = Find("_PBROn", props);
            MaterialProperty metallicMap = Find("_MetallicMap", props);
            MaterialProperty metallic = Find("_Metallic", props);
            MaterialProperty roughnessMap = Find("_RoughnessMap", props);
            MaterialProperty roughness = Find("_Roughness", props);
            MaterialProperty specularIntensity = Find("_SpecularIntensity", props);
            MaterialProperty envReflectionIntensity = Find("_EnvReflectionIntensity", props);

            MaterialProperty sssOn = Find("_SSSOn", props);
            MaterialProperty sssColor = Find("_SSSColor", props);
            MaterialProperty sssWrap = Find("_SSSWrap", props);
            MaterialProperty sssIntensity = Find("_SSSIntensity", props);
            MaterialProperty sssThickness = Find("_SSSThickness", props);
            MaterialProperty sssTransmissionPower = Find("_SSSTransmissionPower", props);

            MaterialProperty outlineColor = Find("_OutlineColor", props);
            MaterialProperty zOffset = Find("_ZOffset", props);
            MaterialProperty smoothNormal = Find("_IsSmoothNormal", props);
            MaterialProperty outlineWidthLit = Find("_OutlineWidthLit", props);
            MaterialProperty outlineWidthShadow = Find("_OutlineWidthShadow", props);

            MaterialProperty smearsOn = Find("_SmearsOn", props);
            MaterialProperty smearsPower = Find("_SmearsPower", props);
            MaterialProperty smearsDirection = Find("_SmearsDirection", props);

            MaterialProperty perspectiveRatio = Find("_PerspectiveRemovalRatio", props);
            MaterialProperty perspectiveRadius = Find("_PerspectiveRemovalRadius", props);
            MaterialProperty head = Find("_Head", props);

            MaterialProperty fakeShadowOn = Find("_FakeShadowOn", props);
            MaterialProperty fakeShadowColor = Find("_FakeShadowColor", props);
            MaterialProperty fakeShadowDistance = Find("_FakeShadowDistance", props);
            MaterialProperty fakeShadowDepthBias = Find("_FakeShadowDepthBias", props);

            MaterialProperty stencilRef = Find("_StencilRef", props);
            MaterialProperty stencilReadMask = Find("_StencilReadMask", props);
            MaterialProperty stencilWriteMask = Find("_StencilWriteMask", props);
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

                EditorGUILayout.Space(5);
                materialEditor.ShaderProperty(charShadowOn, new GUIContent("Character Self Shadow", "キャラ専用シャドウマップ(_CharShadowmap)によるセルフシャドウを受け取る"));

            });

            DrawSection("Fade", ref showFade, () =>
            {
                materialEditor.ShaderProperty(_fadeAlpha, new GUIContent("Fade Alpha", "アルファフェードの強度 (0-1)"));
                materialEditor.ShaderProperty(_fadeOn, new GUIContent("Fade On", "アルファフェードを有効化"));
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

            DrawSection("PBR", ref showPBR, () =>
            {
                materialEditor.ShaderProperty(pbrOn, new GUIContent("PBR On", "トゥーンランプにGGXスペキュラと環境反射を合成(装備・金属向け)"));
                if (pbrOn.floatValue == 1)
                {
                    EditorGUI.indentLevel++;
                    materialEditor.TexturePropertySingleLine(new GUIContent("Metalness", "金属度マップ(未設定時は黒=非金属)"), metallicMap, metallic);
                    materialEditor.TexturePropertySingleLine(new GUIContent("Roughness", "粗さマップ(未設定時は白=マット)"), roughnessMap, roughness);
                    materialEditor.ShaderProperty(specularIntensity, new GUIContent("Specular Intensity", "直接スペキュラの強度"));
                    materialEditor.ShaderProperty(envReflectionIntensity, new GUIContent("Env Reflection", "リフレクションプローブ反射の強度"));
                    EditorGUI.indentLevel--;
                }
            });

            DrawSection("SSS", ref showSSS, () =>
            {
                materialEditor.ShaderProperty(sssOn, new GUIContent("SSS On", "簡易サブサーフェス散乱を有効化"));
                if (sssOn.floatValue == 1)
                {
                    EditorGUI.indentLevel++;
                    materialEditor.ShaderProperty(sssColor, new GUIContent("SSS Color", "散乱色 (肌の暖色)"));
                    materialEditor.ShaderProperty(sssWrap, new GUIContent("Wrap", "明暗境界の滲み幅 (0-1)"));
                    materialEditor.ShaderProperty(sssIntensity, new GUIContent("Intensity", "SSS全体強度"));
                    materialEditor.ShaderProperty(sssThickness, new GUIContent("Thickness", "透過強度 (耳・指など薄い箇所)"));
                    materialEditor.ShaderProperty(sssTransmissionPower, new GUIContent("Transmission Power", "透過ローブのシャープさ"));
                    EditorGUI.indentLevel--;
                }
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

            DrawSection("Smears", ref showSmears, () =>
            {
                materialEditor.ShaderProperty(smearsOn, new GUIContent("Smears On", "アウトラインのスメアを有効化"));
                if (smearsOn.floatValue == 1)
                {
                    EditorGUI.indentLevel++;
                    materialEditor.ShaderProperty(smearsPower, new GUIContent("Power", "スメアの変位量"));
                    materialEditor.VectorProperty(smearsDirection, "Direction (World)");
                    EditorGUI.indentLevel--;
                }
            });

            DrawSection("Perspective Removal", ref showPerspective, () =>
            {
                materialEditor.ShaderProperty(perspectiveRatio, new GUIContent("Ratio", "透視除去(パース抜き)の強度"));
                materialEditor.ShaderProperty(perspectiveRadius, new GUIContent("Radius", "効果が及ぶ半径"));
                materialEditor.VectorProperty(head, "Head Position (World)");
            });

            DrawSection("Fake Shadow", ref showFakeShadow, () =>
            {
                materialEditor.ShaderProperty(fakeShadowOn, new GUIContent("Enable", "髪など、顔に落とす擬似影を描くマテリアルで有効化"));
                if (fakeShadowOn.floatValue == 1)
                {
                    EditorGUI.indentLevel++;
                    materialEditor.ShaderProperty(fakeShadowColor, new GUIContent("Color (Multiply)", "顔に乗算する色。暗いほど濃い影になる"));
                    materialEditor.ShaderProperty(fakeShadowDistance, new GUIContent("Distance", "ライトの進行方向へ頂点をずらす距離"));
                    materialEditor.ShaderProperty(fakeShadowDepthBias, new GUIContent("Depth Bias", "カメラ側への引き戻し量。影が欠ける場合に上げる"));
                    EditorGUI.indentLevel--;

                    EditorGUILayout.HelpBox("影を受ける顔マテリアル側で Stencil プリセット「顔（FakeShadow受け）」を設定してください。", MessageType.Info);
                }
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
                        stencilReadMask,
                        StencilBits.EyeThrough,
                        stencilWriteMask,
                        StencilBits.EyeThrough,
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
                        stencilReadMask,
                        StencilBits.EyeThrough,
                        stencilWriteMask,
                        StencilBits.EyeThrough,
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
                    // 髪が覆ったピクセルの顔領域ビットを落とし、FakeShadowが髪の上に乗らないようにする。
                    // 目の透け用ビット(bit0)はWriteMaskで保護される。
                    SetStencil(
                        materialEditor,
                        stencilRef,
                        1,
                        stencilReadMask,
                        StencilBits.EyeThrough,
                        stencilWriteMask,
                        StencilBits.FaceRegion,
                        stencilComp,
                        CompareFunction.Always,
                        stencilPass,
                        StencilOp.Zero,
                        stencilFail,
                        StencilOp.Keep,
                        2010
                    );
                }
                if (GUILayout.Button("顔（FakeShadow受け）"))
                {
                    SetStencil(
                        materialEditor,
                        stencilRef,
                        StencilBits.FaceRegion,
                        stencilReadMask,
                        StencilBits.FaceRegion,
                        stencilWriteMask,
                        StencilBits.FaceRegion,
                        stencilComp,
                        CompareFunction.Always,
                        stencilPass,
                        StencilOp.Replace,
                        stencilFail,
                        StencilOp.Keep,
                        2001
                    );
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(5);

                materialEditor.ShaderProperty(stencilRef, new GUIContent("Stencil ID", "ステンシル参照値 (0-255)"));
                materialEditor.ShaderProperty(stencilReadMask, new GUIContent("Read Mask", "比較に使うビット。bit0(1):目の透け / bit1(2):顔領域"));
                materialEditor.ShaderProperty(stencilWriteMask, new GUIContent("Write Mask", "書き込むビット。他用途のビットを壊さないよう限定する"));
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

            foreach (var target in materialEditor.targets)
            {
                if (target is Material mat)
                {
                    SyncKeywords(mat);
                }
            }
        }

        public override void ValidateMaterial(Material material)
        {
            SyncKeywords(material);
        }

        /// <summary>
        /// 未使用機能の計算をシェーダーバリアントごと省くため、
        /// マテリアルの設定値に合わせてshader_featureキーワードを同期する。
        /// </summary>
        private static void SyncKeywords(Material material)
        {
            SetKeyword(material, "_NORMALMAP",
                material.HasProperty("_NormalMap") && material.GetTexture("_NormalMap") != null);
            SetKeyword(material, "_ISFORFACE_ON",
                material.HasProperty("_IsForFace") && material.GetFloat("_IsForFace") > 0);
            SetKeyword(material, "_PERSPECTIVE_REMOVAL_ON",
                material.HasProperty("_PerspectiveRemovalRatio") && material.GetFloat("_PerspectiveRemovalRatio") > 0);
            SetKeyword(material, "_CHAR_SHADOW_ON",
                material.HasProperty("_CharShadowOn") && material.GetFloat("_CharShadowOn") > 0);
            SetKeyword(material, "_PBR_ON",
                material.HasProperty("_PBROn") && material.GetFloat("_PBROn") > 0);
        }

        private static void SetKeyword(Material material, string keyword, bool enabled)
        {
            if (material.IsKeywordEnabled(keyword) == enabled) return;

            if (enabled)
            {
                material.EnableKeyword(keyword);
            }
            else
            {
                material.DisableKeyword(keyword);
            }
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

        private void SetStencil(MaterialEditor editor, MaterialProperty pRef, float vRef, MaterialProperty pReadMask, int vReadMask, MaterialProperty pWriteMask, int vWriteMask, MaterialProperty pComp, CompareFunction vComp, MaterialProperty pPass, StencilOp vPass, MaterialProperty pFail, StencilOp vFail, int queue)
        {
            editor.RegisterPropertyChangeUndo("Set Stencil Template");
            pRef.floatValue = vRef;
            if (pReadMask != null) pReadMask.floatValue = vReadMask;
            if (pWriteMask != null) pWriteMask.floatValue = vWriteMask;
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
