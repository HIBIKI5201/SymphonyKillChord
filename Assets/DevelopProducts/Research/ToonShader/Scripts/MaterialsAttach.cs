using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DevelopProducts.ToonShader.Editor
{
    public sealed class MaterialAttachs : MonoBehaviour
    {
        [ContextMenu("AttachMaterial")]
        public void Attach()
        {
            _renderers.Clear();
            _record.Clear();

            Material material = null;
            foreach (var item in _attachDestination)
            {
                material = item.Fragment;
                Material[] materials = { item.Fragment, item.Outline };
                foreach (var renderer in item.Renderer)
                {
                    if (_renderers.TryGetValue(renderer, out var result))
                    {
                        Debug.LogWarning($"{renderer.name}で適用マテリアルが競合しています。{result.name},{material.name}");
                        continue;
                    }
                    renderer.sharedMaterials = materials;
                    _record.Add(renderer);
                }
            }

            Undo.RecordObjects(_record.ToArray(), "Set Materials");
        }




        [SerializeField] private AttachDestination[] _attachDestination;
        private Dictionary<Object, Material> _renderers = new();
        private List<Object> _record = new();


        [System.Serializable]
        private struct AttachDestination
        {
            public readonly Material Fragment => _fragment;
            public readonly Material Outline => _outline;
            public readonly Renderer[] Renderer => _renderer;

            [SerializeField] private Material _fragment;
            [SerializeField] private Material _outline;
            [SerializeField] private Renderer[] _renderer;
        }
    }
}
