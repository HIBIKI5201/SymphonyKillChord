using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace Mock.MusicBattle.UI
{
    public class DamageTextPool
    {
        public DamageTextPool(VisualElement root)
        {
            _root = root;
            _pool = new(
            createFunc: Create,
            actionOnGet: Get,
            actionOnRelease: Release
        );
        }

        public void ShowDamageText(float damage, Vector3 position)
        {
            DamageTextEntity entity = _pool.Get();
            entity.Show(damage, position);
        }

        private readonly VisualElement _root;

        private ObjectPool<DamageTextEntity> _pool;

        private DamageTextEntity Create()
        {
            DamageTextEntity entity = new();
            entity.Initialize(() => Release(entity), 2);
            _root.Add(entity);

            return entity;
        }

        private void Get(DamageTextEntity entity)
        {
            entity.style.visibility = Visibility.Visible;
        }

        private void Release(DamageTextEntity entity)
        {
            entity.style.visibility = Visibility.Hidden;
        }
    }
}
