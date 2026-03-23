Shader "Custom/SilToon"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}

        [Header(OutLine)]

        _ZOffset("Z Offset",Range(0,0.01)) = 0
        [Toggle] _IsSmoothNormal("Is Smooth Normal", Float) = 0
         _OutlineWidthLit("OutLine Width Lit", Float) = 0
         _OutlineWidthShadow("OutLine Width Shadow", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "MAIN"
            Tags { "LightMode" = "UniversalForward" } 
            Cull Back

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


    }
}
