using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace Mock.MusicBattle.UI
{
    public class DamageTextPool
    {
        public void ShowDamageText(float damage, Vector3 position)
        {
            DamageTextEntity entity = _pool.Get();
            entity.BindData(damage, position);
        }

        private ObjectPool<DamageTextEntity> _pool = new(
            createFunc: Create,
            actionOnGet: Get,
            actionOnRelease: Release
        );

        private static DamageTextEntity Create()
        {
            DamageTextEntity entity = new();
            entity.Initialize(() => Release(entity));
            return entity;
        }

        private static void Get(DamageTextEntity entity)
        {
            entity.style.visibility = Visibility.Visible;
        }

        private static void Release(DamageTextEntity entity)
        {
            entity.style.visibility = Visibility.Hidden;
        }
    }
}
