Shader "Custom/SilToon/Base"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        [MainColor] _ColorLit("Lit Color",Color) = (1, 1, 1, 1)
        _ColorMiddle("Middle Color",Color) = (1, 1, 1, 1)
        _ColorShadow("Shadow Color",Color) = (1, 1, 1, 1)
        [Toggle(_ISFORFACE_ON)] _IsForFace("Is For Face", Float) = 0
        _FaceUp("Face Up", Vector, 3) = (0,1,0)

        [Toggle(_CHAR_SHADOW_ON)] _CharShadowOn("Character Self Shadow", Float) = 0

         [Toggle(FADE_ON)] _FadeOn("Fade", Float) = 0
        _FadeAlpha("Fade Alpha",Range(0,1)) = 0

        [Header(Normal)]
        [Normal] _NormalMap("Normal Map", 2D) = "black"{}
        _NormalMapIntensity("Intensity",Float) = 0


        [Header(Fresnel)]
        _FresnelBackLight("Back Light Intensity",Float) = 8
        _FresnelFrontRimLight("Front Rim Light Intensity",Float) = 4
        _FresnelBackRimLight("Back Rim Light Intensity",Float) = 0.5

        [Header(PBR)]
        [Toggle(_PBR_ON)] _PBROn("PBR On", Float) = 0
        _MetallicMap("Metalness", 2D) = "black" {}
        _Metallic("Metallic Scale", Range(0, 1)) = 1
        _RoughnessMap("Roughness", 2D) = "white" {}
        _Roughness("Roughness Scale", Range(0, 1)) = 1
        _SpecularIntensity("Specular Intensity", Range(0, 4)) = 1
        _EnvReflectionIntensity("Env Reflection Intensity", Range(0, 4)) = 1

        [Header(SSS)]
        [Toggle(SSS_ON)] _SSSOn("SSS On", Float) = 0
        _SSSColor("SSS Color", Color) = (1.0, 0.4, 0.3, 1)
        _SSSWrap("Wrap", Range(0, 1)) = 0.3
        _SSSIntensity("Intensity", Range(0, 2)) = 0.5
        _SSSThickness("Thickness", Range(0, 1)) = 0.3
        _SSSTransmissionPower("Transmission Power", Range(1, 16)) = 4

        [Header(OutLine)]
        _OutlineColor("Color",Color) = (1, 1, 1, 1)
        _ZOffset("Z Offset",Range(0,0.1)) = 0
        [Toggle] _IsSmoothNormal("Is Smooth Normal", Float) = 0
        _OutlineWidthLit("OutLine Width Lit", Float) = 0
        _OutlineWidthShadow("OutLine Width Shadow", Float) = 0

        [Header(Smears)]
        [Toggle(SMEARS_ON)] _SmearsOn("Smears On",Float) = 0
        _SmearsPower ("Smears Power", Float) = 0.0
        _SmearsDirection ("Smears Direction", Vector,3) = (0, 1, 0)

        [Header(PerspectiveRemoval)]
        _PerspectiveRemovalRatio("Perspective Removal", Range(0,1)) = 0
        _PerspectiveRemovalRadius("Radius",Float) = 1
        _Head("HeadPosition", Vector,3) = (0,0,0)

        [Header(FakeShadow)]
        [Toggle(_FAKE_SHADOW_ON)] _FakeShadowOn("Fake Shadow On", Float) = 0
        _FakeShadowColor("Color (Multiply)", Color) = (0.6, 0.55, 0.6, 1)
        _FakeShadowDistance("Distance", Float) = 0.1
        _FakeShadowDepthBias("Depth Bias", Float) = 0.01

        [Header(RenderState)]

        [IntRange]
        _StencilRef ("Stencil ID", Range(0, 255)) = 1

        // 用途ごとにビットを分けるためのマスク。bit0:目の透け / bit1:顔領域(FakeShadow用)
        // 既定値は bit0 のみ。255にすると未設定のマテリアルが他機能のビットまで読み書きして壊すため、
        // 「自分の用途以外には触らない」側を初期値にしている。
        [IntRange]
        _StencilReadMask ("Stencil Read Mask", Range(0, 255)) = 1

        [IntRange]
        _StencilWriteMask ("Stencil Write Mask", Range(0, 255)) = 1

        [Enum(UnityEngine.Rendering.CompareFunction)]
        _StencilComp ("Stencil Comp", Float) = 8

        [Enum(UnityEngine.Rendering.StencilOp)] 
        _StencilPass ("Stencil Pass Op", Float) = 0

        [Enum(UnityEngine.Rendering.StencilOp)] 
        _StencilFail ("Stencil Fail Op", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "MAIN"
            Tags { "LightMode" = "UniversalForwardOnly" }
            Cull Back

            ZWrite On

            Stencil{
                Ref [_StencilRef]
                ReadMask [_StencilReadMask]
                WriteMask [_StencilWriteMask]

                Comp [_StencilComp] //Hair:Always, Eye:Always, EyeThrouth:Equal
                Pass [_StencilPass] //Hair:Keep, Eye:Replace, EyeThrouth: Zero Keep
                Fail [_StencilFail] //Hair:Zero, Eye:Keep, EyeThrouth: Keep

                ZFail Keep
            }

            HLSLPROGRAM

                #pragma vertex vert
                #pragma fragment frag

                #pragma multi_compile _ FADE_ON

                #pragma shader_feature_local _NORMALMAP
                #pragma shader_feature_local_fragment _ISFORFACE_ON
                #pragma shader_feature_local_fragment _CHAR_SHADOW_ON
                #pragma shader_feature_local_fragment SSS_ON
                #pragma shader_feature_local_fragment _PBR_ON
                #pragma shader_feature_local_vertex _PERSPECTIVE_REMOVAL_ON

                #pragma multi_compile_fragment _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
                #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
                #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW

                #pragma multi_compile           _ _CLUSTER_LIGHT_LOOP

                #include "Assets/Scripts/Shaders/SilToon/HLSL/Fragment/Fragment.hlsl"

            ENDHLSL
        }

        Pass
        {
            Name "OUTLINE"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Cull Front

            ZWrite On

            HLSLPROGRAM

                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile _ FADE_ON
                #pragma multi_compile_local _ SMEARS_ON
                #pragma shader_feature_local_vertex _PERSPECTIVE_REMOVAL_ON

                #pragma multi_compile_vertex _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
                #include "Assets/Scripts/Shaders/SilToon/HLSL/OutLine/OutLine.hlsl"

            ENDHLSL
        }

        Pass
        {
            Name "FAKE_SHADOW"
            // URP標準の不透明描画が拾わない独自タグ。RenderObjects機能で不透明描画後に明示的に実行する。
            Tags { "LightMode" = "SilToonFakeShadow" }

            // 髪カードは両面のことがあるため、シルエットを埋めるようCullしない
            Cull Off

            ZWrite Off
            // LEqualにすることで、頭の後ろ側の髪ポリゴンが顔より奥と判定され弾かれる。
            // (Alwaysだと後頭部の髪まで顔に落ちてしまう)
            ZTest LEqual

            // 乗算合成。顔の陰影を保ったまま暗くする
            Blend DstColor Zero

            // ステンシルの用途はStencilBits.csと対応する。ここは意味が固定なのでリテラルで持つ。
            //   Ref 2      : bit1 = 顔領域 (顔マテリアルが書き込む)
            //   ReadMask 6 : bit1(顔領域) と bit2(描画済みマーク) を見る
            //   WriteMask 4 / Pass Invert : 最初の1フラグメントだけ bit2 (FakeShadowDrawn) を立てる
            //   → 髪の重なりで同じピクセルが多重に暗くなるのを防ぐ
            Stencil{
                Ref 2
                ReadMask 6
                WriteMask 4

                Comp Equal
                Pass Invert
                // 「描画済みマーク」を消すとそのピクセルが再度描けてしまい多重に暗くなるため、
                // 失敗時は必ず現状維持にする
                Fail Keep
                ZFail Keep
            }

            HLSLPROGRAM

                #pragma vertex vert
                #pragma fragment frag

                #pragma multi_compile _ FADE_ON
                #pragma shader_feature_local _FAKE_SHADOW_ON

                #include "Assets/Scripts/Shaders/SilToon/HLSL/FakeShadow/FakeShadow.hlsl"

            ENDHLSL
        }

        Pass {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM

                #pragma vertex ShadowPassVertex
                #pragma fragment ShadowPassFragment
                #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"

            ENDHLSL
        }
    }
    CustomEditor "DevelopProducts.ToonShader.SilToonGUI"
}
