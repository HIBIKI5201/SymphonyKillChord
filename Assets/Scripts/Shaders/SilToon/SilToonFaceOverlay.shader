Shader "Custom/SilToon/FaceOverlay"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        [MainColor] _ColorLit("Lit Color",Color) = (1, 1, 1, 1)
        _ColorMiddle("Middle Color",Color) = (1, 1, 1, 1)
        _ColorShadow("Shadow Color",Color) = (1, 1, 1, 1)
        _Alpha("Alpha",Range(0,1)) = 0.5
        [Toggle(_ISFORFACE_ON)] _IsForFace("Is For Face", Float) = 0
        _FaceUp("Face Up", Vector, 3) = (0,1,0)

        [Header(Normal)]
        [Normal] _NormalMap("Normal Map", 2D) = "black"{}
        _NormalMapIntensity("Intensity",Float) = 0


        [Header(Fresnel)]
        _FresnelBackLight("Back Light Intensity",Float) = 8
        _FresnelFrontRimLight("Front Rim Light Intensity",Float) = 4
        _FresnelBackRimLight("Back Rim Light Intensity",Float) = 0.5

        [Header(PerspectiveRemoval)]
        _PerspectiveRemovalRatio("Perspective Removal", Range(0,1)) = 0
        _PerspectiveRemovalRadius("Radius",Float) = 1
        _Head("HeadPosition", Vector,3) = (0,0,0)

        [Header(RenderState)]
        [IntRange] _StencilRef ("Stencil ID", Range(0, 255)) = 1

        // 用途ごとにビットを分けるためのマスク。bit0:目の透け / bit1:顔領域(FakeShadow用)
        // 既定値は bit0 のみ。255にすると未設定のマテリアルが他機能のビットまで読み書きして壊すため、
        // 「自分の用途以外には触らない」側を初期値にしている。
        [IntRange] _StencilReadMask ("Stencil Read Mask", Range(0, 255)) = 1
        [IntRange] _StencilWriteMask ("Stencil Write Mask", Range(0, 255)) = 1

        [Enum(UnityEngine.Rendering.StencilOp)]
        _StencilPass ("Stencil Pass Op", Float) = 0
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "MAIN Eye Flash"
            Tags { "LightMode" = "UniversalForwardOnly" } 
            Cull Back

            Blend SrcAlpha OneMinusSrcAlpha

            ZWrite Off
            ZTest Always
            Stencil{
                Ref [_StencilRef]
                ReadMask [_StencilReadMask]
                WriteMask [_StencilWriteMask]

                Comp Equal
                Pass [_StencilPass]
            }

            HLSLPROGRAM

                #pragma vertex vert
                #pragma fragment frag
                #pragma shader_feature_local _NORMALMAP
                #pragma shader_feature_local_fragment _ISFORFACE_ON
                #pragma shader_feature_local_vertex _PERSPECTIVE_REMOVAL_ON
                #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
                #include "Assets/Scripts/Shaders/SilToon/HLSL/Fragment/ThroughFragment.hlsl"

            ENDHLSL
        }
    }
    CustomEditor "DevelopProducts.ToonShader.SilToonFaceOverlayGUI"
}
