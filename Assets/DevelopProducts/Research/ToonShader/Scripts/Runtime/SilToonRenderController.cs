using UnityEngine;

namespace DevelopProducts.ToonShader
{
    [ExecuteInEditMode]
    public sealed class SilToonRenderController : MonoBehaviour
    {
        [SerializeField] private Material[] _materials;

        [Header("Character Renderer")]
        [SerializeField] private MeshRenderer[] _meshrenderers;
        [SerializeField] private SkinnedMeshRenderer[] _skinnedMeshRenderers;

        [Header("Head & Face")]
        [SerializeField] private HeadNFace _headNFace;


        private static readonly int _idHeadPosition = Shader.PropertyToID("_Head");
        private static readonly int _idHeadUp = Shader.PropertyToID("_FaceUp");

        [ContextMenu("SetUpRenderer")]
        public void SetUpRenderer()
        {
            _meshrenderers = GetComponentsInChildren<MeshRenderer>(true);
            _skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);
        }

        private void Start()
        {
        }

        // Update is called once per frame
        private void Update()
        {
            if (_headNFace == null) return;
            if (_materials == null || _materials.Length == 0) return;

            _headNFace.Update(new(
                _materials,
                _idHeadPosition,
                _idHeadUp
            ));
        }
        private void OnDrawGizmos()
        {
            if (_headNFace == null) return;
            _headNFace.OnDrawGizmos();
        }
    }
}
