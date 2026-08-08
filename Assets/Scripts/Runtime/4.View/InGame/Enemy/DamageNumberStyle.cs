using KillChord.Runtime.Adaptor.InGame.Enemy;
using System;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     ダメージ数値のスタイルを表すクラス。
    /// </summary>
    [Serializable]
    public class DamageNumberStyle
    {
        // <summary> ダメージ数値の種類を取得します。 </summary>
        public DamageNumberType Type => _type;

        // <summary> ダメージ数値の色を取得します。 </summary>
        public Color TextColor => _textColor;

        // <summary> ダメージ数値の背景スプライトを取得します。 </summary>
        public Sprite BackGroundSprite => _backGroundSprite;

        [SerializeField, Tooltip("ダメージ数値の種類")]
        private DamageNumberType _type;

        [SerializeField, Tooltip("ダメージ数値の色")]
        private Color _textColor;

        [SerializeField, Tooltip("ダメージ数値の背景スプライト")]
        private Sprite _backGroundSprite;
    }
}
