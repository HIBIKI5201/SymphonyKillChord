using UnityEngine;

namespace Mock.MusicBattle.Character
{
    /// <summary> キャラクターの共通事項。 </summary>
    public interface ICharacter
    {
        public GameObject gameObject { get; }
        public Vector3 Pivot { get; }

        public void TakeDamage(float damage);
    }
}
