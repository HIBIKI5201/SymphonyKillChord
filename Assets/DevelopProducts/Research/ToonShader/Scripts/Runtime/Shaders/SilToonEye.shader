Shader "Custom/SilToon/Eye"
{
    Properties
    {
        [IntRange] _StencilRef ("Stencil ID", Range(0, 255)) = 0
        // [Header(Fragment)}
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        [MainColor] _ColorLit("Lit Color",Color) = (1, 1, 1, 1)
        _ColorMiddle("Middle Color",Color) = (1, 1, 1, 1)
        _ColorShadow("Shadow Color",Color) = (1, 1, 1, 1)
        [Toggle] _IsForFace("Is For Face", Float) = 0
        _FaceUp("Face Up", Vector, 3) = (0,1,0)

        [Headedr(Normal)]
        [Normal] _NormalMap("Normal Map", 2D) = "black"{}
        _NormalMapIntensity("Intensity",Float) = 0


        [Header(Fresnel)]
        _FresnelBackLight("Back Light Intensity",Float) = 8
        _FresnelFrontRimLight("Front Rim Light Intensity",Float) = 4
        _FresnelBackRimLight("Back Rim Light Intensity",Float) = 0.5

        [Header(OutLine)]
        _OutlineColor("Color",Color) = (1, 1, 1, 1)
        _ZOffset("Z Offset",Range(0,0.01)) = 0
        [Toggle] _IsSmoothNormal("Is Smooth Normal", Float) = 0
        _OutlineWidthLit("OutLine Width Lit", Float) = 0
        _OutlineWidthShadow("OutLine Width Shadow", Float) = 0

        [Header(PerspectiveRemoval)]
        _PerspectiveRemovalRatio("Perspective Removal", Range(0,1)) = 0
        _PerspectiveRemovalRadius("Radius",Float) = 1
        _Head("HeadPosition", Vector,3) = (0,0,0)
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }


        Pass
        {
            Name "MAIN"
            Tags { "LightMode" = "UniversalForwardOnly" } 
            Cull Back

            ZTest LEqual
            Stencil{
                Ref [_StencilRef]


                Comp Always
                Pass Replace
                Fail Keep
                ZFail Keep
            }

            HLSLPROGRAM

                #pragma vertex vert
                #pragma fragment frag
                #include "Assets\DevelopProducts\Research\ToonShader\Scripts\Runtime\Shaders\HLSL\Fragment\Fragment.hlsl"

            ENDHLSL
        }

        Pass
        {
            Name "OUTLINE"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Cull Front

            HLSLPROGRAM

                #pragma vertex vert
                #pragma fragment frag
                #include "Assets/DevelopProducts/Research/ToonShader/Scripts/Runtime/Shaders/HLSL/OutLine/OutLine.hlsl"

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

                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"

            ENDHLSL
        }
    }
}
