using KillChord.Runtime.Adaptor.InGame.Mission;
using System;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Mission
{
    /// <summary>
    ///     説明ポップアップの表示に合わせて、スマートフォン用の全画面攻撃判定を切り替えるデコレータです。
    /// </summary>
    public sealed class MobileAttackAreaPopupViewDecorator : IMissionStepPopupView
    {
        /// <summary>
        ///     ポップアップViewと全画面攻撃判定を結合します。
        /// </summary>
        /// <param name="inner"> 実際にポップアップを表示するViewです。 </param>
        /// <param name="attackArea"> ポップアップ表示中に有効化する全画面攻撃判定です。 </param>
        public MobileAttackAreaPopupViewDecorator(
            IMissionStepPopupView inner,
            MobileFullScreenAttackAreaView attackArea)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _attackArea = attackArea ?? throw new ArgumentNullException(nameof(attackArea));
        }

        /// <inheritdoc />
        public void Show(Sprite image)
        {
            _inner.Show(image);
            _attackArea.Show();
        }

        /// <inheritdoc />
        public void Hide()
        {
            _inner.Hide();
            _attackArea.Hide();
        }

        private readonly IMissionStepPopupView _inner;
        private readonly MobileFullScreenAttackAreaView _attackArea;
    }
}
