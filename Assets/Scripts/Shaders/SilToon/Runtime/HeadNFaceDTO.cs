using System;
using UnityEngine;

namespace DevelopProducts.ToonShader
{
    public readonly ref struct HeadNFaceDTO
    {
        public HeadNFaceDTO(
            Span<Material> materials,
            int shaderIDHeadPosition,
            int shaderIDHeadUp)
        {
            Materials = materials;
            ShaderIDHeadPosition = shaderIDHeadPosition;
            ShaderIDHeadUp = shaderIDHeadUp;
        }

        public readonly ReadOnlySpan<Material> Materials;
        public readonly int ShaderIDHeadPosition;
        public readonly int ShaderIDHeadUp;
    }
}
