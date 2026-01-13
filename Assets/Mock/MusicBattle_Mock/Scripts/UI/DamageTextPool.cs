using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace Mock.MusicBattle.UI
{
    /// <summary>
    ///     ダメージテキストのオブジェクトプールを管理するクラス。
    /// </summary>
    public class DamageTextPool
    {
        #region コンストラクタ
        /// <summary>
        ///     <see cref="DamageTextPool"/>クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="root">ダメージテキストを追加するルートVisualElement。</param>
        public DamageTextPool(VisualElement root)
        {
            _root = root;
            _pool = new(
            createFunc: Create,
            actionOnGet: Get,
            actionOnRelease: Release
        );
        }
        #endregion

        #region Publicメソッド
        /// <summary>
        ///     ダメージテキストをプールから取得し、指定されたダメージ量と位置で表示します。
        /// </summary>
        /// <param name="damage">表示するダメージ量。</param>
        /// <param name="position">ダメージテキストを表示するワールド座標。</param>
        public void ShowDamageText(float damage, Vector3 position)
        {
            // プールから取得して表示する。
            DamageTextEntity entity = _pool.Get();
            entity.Show(damage, position);
        }
        #endregion

        #region プライベートフィールド
        /// <summary> ダメージテキストを追加するルートVisualElement。 </summary>
        private readonly VisualElement _root;
        /// <summary> ダメージテキストエンティティのオブジェクトプール。 </summary>
        private ObjectPool<DamageTextEntity> _pool;
        #endregion

        #region Privateメソッド
        /// <summary>
        ///     プールが新しいインスタンスを必要とするときに呼び出されます。
        ///     新しいDamageTextEntityを生成し、初期化します。
        /// </summary>
        /// <returns>生成されたDamageTextEntity。</returns>
        private DamageTextEntity Create()
        {
            DamageTextEntity entity = new();
            entity.Initialize(() => Release(entity), 2);
            _root.Add(entity);

            return entity;
        }

        /// <summary>
        ///     プールからDamageTextEntityが取得されるときに呼び出されます。
        ///     テキストエンティティを可視状態にします。
        /// </summary>
        /// <param name="entity">取得されたDamageTextEntity。</param>
        private void Get(DamageTextEntity entity)
        {
            entity.style.visibility = Visibility.Visible;
        }

        /// <summary>
        ///     DamageTextEntityがプールに返却されるときに呼び出されます。
        ///     テキストエンティティを非可視状態にします。
        /// </summary>
        /// <param name="entity">返却されたDamageTextEntity。</param>
        private void Release(DamageTextEntity entity)
        {
            entity.style.visibility = Visibility.Hidden;
        }
        #endregion
    }
}

