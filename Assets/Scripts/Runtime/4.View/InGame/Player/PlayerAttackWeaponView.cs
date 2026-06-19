using UnityEngine;

namespace KillChord.Runtime.View.InGame.Player
{
    /// <summary>
    ///    攻撃BeatTypeに応じた武器モデル表示と攻撃SE再生を担当するViewクラス。
    /// </summary>
    public class PlayerAttackWeaponView : MonoBehaviour
    {
        public void Play(int beattype)
        {
            HideAllWeapons();

            if (!TryGetDefinition(beattype, out PlayerAttackWeaponConfig definition))
            {
                Debug.LogError($"BeatType {beattype} に対応する武器設定が見つかりませんでした。");
                return;
            }

            _currentWeaponModel = definition.WeaponModel;

            if (_currentWeaponModel != null)
            {
                _currentWeaponModel.SetActive(true);
            }

            PlayAttackSound(definition);
        }

        /// <summary>
        ///     表示中の武器を非表示にする。
        ///     攻撃アニメーション終了時のAnimation Eventから呼び出す。
        /// </summary>
        public void HideCurrentWeapon()
        {
            if (_currentWeaponModel == null)
            {
                return;
            }

            _currentWeaponModel.SetActive(false);
            _currentWeaponModel = null;
        }

        /// <summary>
        ///     全武器を非表示にする。
        /// </summary>
        public void HideAllWeapons()
        {
            if (_definitions == null)
            {
                _currentWeaponModel = null;
                return;
            }

            for (int i = 0; i < _definitions.Length; i++)
            {
                if (_definitions[i].WeaponModel == null)
                {
                    continue;
                }

                _definitions[i].WeaponModel.SetActive(false);
            }

            _currentWeaponModel = null;
        }

        [SerializeField, Tooltip("BeatTypeごとの武器表示と攻撃SE設定。")]
        private PlayerAttackWeaponConfig[] _definitions;

        private GameObject _currentWeaponModel;

        private void Awake()
        {
            HideAllWeapons();
        }

        private void OnDisable()
        {
            HideAllWeapons();
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

        /// <summary>
        ///     攻撃SEを再生する。
        /// </summary>
        private void PlayAttackSound(PlayerAttackWeaponConfig definition)
        {
            if (definition.AttackSoundSource == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(definition.CueName))
            {
                definition.AttackSoundSource.Play();
                return;
            }

            definition.AttackSoundSource.Play(definition.CueName);
        }
    }
}
