using System;
using UnityEngine;

namespace DevelopProducts.ToonShader
{
    public readonly ref struct HeadNFaceDTO
    {
        public HeadNFaceDTO(Span<Material> materials, int shaderIDHeadPosition, int shaderIDHeadUp)
        {
            Materials = materials;
            ShaderIDHeadPosition = shaderIDHeadPosition;
            ShaderIDHeadUp = shaderIDHeadUp;
        }

        public readonly ReadOnlySpan<Material> Materials;
        public readonly int ShaderIDHeadPosition;
        public readonly int ShaderIDHeadUp;
    }

    [System.Serializable]
    public sealed class HeadNFace
    {
        [SerializeField] private Transform _head;
        [SerializeField] private Vector3 _offset;
        [SerializeField] private Axis _faceUp;
        [SerializeField] private bool _showGizmos;

        public void Update(in HeadNFaceDTO dto)
        {
            if (_head == null)
                return;

            Vector3 position = _head.position + _head.rotation * _offset;
            Vector3 headUp = _head.rotation * AxisToVector3(_faceUp);

            for (int i = 0; i < dto.Materials.Length; i++)
            {
                dto.Materials[i].SetVector(dto.ShaderIDHeadPosition, position);
                dto.Materials[i].SetVector(dto.ShaderIDHeadUp, headUp);
            }
        }
        public void OnDrawGizmos()
        {
            if (!_showGizmos) return;

            if (_head == null) return;

            Vector3 up = _head.rotation * AxisToVector3(_faceUp);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(_head.position, _head.position + up / 2);

            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(_head.position + _head.rotation * _offset, 0.1f);
        }
        private static Vector3 AxisToVector3(Axis axis)
        {
            return axis switch
            {
                Axis.X => Vector3.right,
                Axis.Y => Vector3.up,
                Axis.Z => Vector3.forward,
                Axis.NegX => Vector3.left,
                Axis.NegY => Vector3.down,
                Axis.NegZ => Vector3.back,
                _ => throw new ArgumentOutOfRangeException(nameof(axis), axis, null)
            };
        }
        private enum Axis
        {
            X,
            Y,
            Z,
            NegX,
            NegY,
            NegZ
        }
    }
}
