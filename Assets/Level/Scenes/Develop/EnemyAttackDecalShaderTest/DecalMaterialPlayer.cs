using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

public class DecalMaterialPlayer : MonoBehaviour
{
    [SerializeField] private string _key;
    [SerializeField] private Material _material;

    private MotionHandle _handle;
    void Start()
    {
        _handle = LMotion.Create(-0.1f, 1.1f, 2.2f)
            .WithLoops(-1, LoopType.Flip)
            .WithDelay(0.1f)
         .WithEase(Ease.Linear)
         .BindToMaterialFloat(_material, _key);
    }
    private void OnDestroy()
    {
        _handle.TryCancel();
    }
}
