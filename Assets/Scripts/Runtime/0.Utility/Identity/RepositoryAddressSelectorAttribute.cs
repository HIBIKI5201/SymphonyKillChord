using UnityEngine;

namespace KillChord.Runtime.Utility.Identity
{
    /// <summary>
    ///     互換維持用の旧属性です。新規コードではSourceDataAddressAttributeを使用します。
    /// </summary>
    [System.Obsolete("Use SourceDataAddressAttribute instead.")]
    public sealed class RepositoryAddressSelectorAttribute : PropertyAttribute
    {
    }
}
