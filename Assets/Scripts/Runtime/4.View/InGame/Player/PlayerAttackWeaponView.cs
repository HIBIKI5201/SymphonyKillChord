using UnityEngine;

namespace KillChord.Runtime.View.InGame.Player
{
    /// <summary>
    ///     攻撃BeatTypeに応じた武器モデル表示と攻撃SE再生を担当するViewクラス。
    /// </summary>
    public sealed class PlayerAttackWeaponView : MonoBehaviour
    {
        /// <summary>
        ///     拍子に応じた演出を再生します。
        /// </summary>
        /// <param name="beatType"> 拍子。 </param>
        public void Play(int beatType)
        {
            HideAllWeaponsImmediate();

            if (!TryGetDefinition(beatType, out PlayerAttackWeaponConfig definition))
            {
                Debug.LogError($"BeatType {beatType} に対応する武器設定が見つかりませんでした。", this);
                return;
            }

            if (definition.WeaponItem == null)
            {
                Debug.LogError($"BeatType {beatType} の武器Viewが未設定です。", this);
                return;
            }

            _currentWeaponView = definition.WeaponItem;
            _currentWeaponView.Play();
        }

        /// <summary>
        ///     表示中の武器を非表示にする。
        ///     攻撃アニメーション終了時のAnimation Eventから呼び出す。
        /// </summary>
        public void HideCurrentWeapon()
        {
            _currentWeaponView?.HideWeapon();
            _currentWeaponView = null;
        }

        /// <summary>
        ///     全武器を非表示にする。
        /// </summary>
        public void HideAllWeapons()
        {
            if (_definitions == null)
            {
                _currentWeaponView = null;
                return;
            }

            for (int i = 0; i < _definitions.Length; i++)
            {
                if (_definitions[i].WeaponItem == null)
                {
                    continue;
                }

                _definitions[i].WeaponItem?.HideWeapon();
            }

            _currentWeaponView = null;
        }

        /// <summary>
        ///     全武器をフェードを挟まずに即座に非表示にする。
        /// </summary>
        public void HideAllWeaponsImmediate()
        {
            if (_definitions == null)
            {
                _currentWeaponView = null;
                return;
            }
            for (int i = 0; i < _definitions.Length; i++)
            {
                if (_definitions[i].WeaponItem == null)
                {
                    continue;
                }
                _definitions[i].WeaponItem?.HideWeaponImmediate();
            }
            _currentWeaponView = null;
        }

        [SerializeField, Tooltip("BeatTypeごとの武器表示と攻撃SE設定。")]
        private PlayerAttackWeaponConfig[] _definitions;

        private WeaponItemView _currentWeaponView;

        /// <summary>
        ///     初期状態では武器を表示しないため、全武器を即座に非表示にする。
        /// </summary>
        private void Awake()
        {
            HideAllWeaponsImmediate();
        }

        /// <summary>
        ///     無効化中はMotionの更新が見えないため、フェードを挟まず全武器を非表示にする。
        /// </summary>
        private void OnDisable()
        {
            HideAllWeaponsImmediate();
        }

        /// <summary>
        ///     BeatTypeに対応する武器設定を取得する。
        /// </summary>
        private bool TryGetDefinition(int beatType, out PlayerAttackWeaponConfig definition)
        {
            if (_definitions == null)
            {
                definition = default;
                return false;
            }

            for (int i = 0; i < _definitions.Length; i++)
            {
                if (_definitions[i].BeatType != beatType)
                {
                    continue;
                }

                definition = _definitions[i];
                return true;
            }

            definition = default;
            return false;
        }
    }
}
