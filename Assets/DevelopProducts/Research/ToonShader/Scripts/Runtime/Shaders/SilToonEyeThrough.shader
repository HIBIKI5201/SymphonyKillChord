Shader "Custom/SilToon/EyeThrough"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        [MainColor] _ColorLit("Lit Color",Color) = (1, 1, 1, 1)
        _ColorMiddle("Middle Color",Color) = (1, 1, 1, 1)
        _ColorShadow("Shadow Color",Color) = (1, 1, 1, 1)
        _Alpha("Alpha",Range(0,1)) = 0.5
        [Toggle] _IsForFace("Is For Face", Float) = 0
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


                Comp Equal
                Pass [_StencilPass]
            }

            HLSLPROGRAM

                #pragma vertex vert
                #pragma fragment frag
                #include "Assets\DevelopProducts\Research\ToonShader\Scripts\Runtime\Shaders\HLSL\Fragment\ThroughFragment.hlsl"

            ENDHLSL
        }
    }
    CustomEditor "DevelopProducts.ToonShader.SilToonEyeThroughGUI"
}
