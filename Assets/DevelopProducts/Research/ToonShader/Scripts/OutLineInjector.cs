using UnityEngine;

public class OutLineInjector : MonoBehaviour
{
    [SerializeField] private Material _outline;

    [ContextMenu("Inject")]
    private void Inject()
    {
        var components = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (var component in components)
        {
            TangentBaker.BakeMesh(component.GetComponent<SkinnedMeshRenderer>().sharedMesh);
            var materials = component.materials;

            bool hasOutline = false;
            foreach (var item in materials)
            {
                if (item == _outline)
                {
                    hasOutline = true;
                    break;
                }
            }
            if (!hasOutline)
                continue;
            var newMaterials = new Material[materials.Length + 1];
            for (int i = 0; i < materials.Length; i++)
            {
                newMaterials[i] = materials[i];
            }
            newMaterials[^1] = _outline;
        }
    }
}
