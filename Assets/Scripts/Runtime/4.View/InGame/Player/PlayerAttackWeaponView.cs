using System;
using System.Threading;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Player
{
    /// <summary>
    ///    攻撃BeatTypeに応じた武器モデル表示と攻撃SE再生を担当するViewクラス。
    /// </summary>
    public class PlayerAttackWeaponView : MonoBehaviour
    {
        /// <summary>
        ///     拍子に応じた演出を再生します。
        /// </summary>
        /// <param name="beatType"> 拍子。 </param>
        /// <param name="clipSeconds"> クリップの長さ。 </param>
        public void Play(int beatType, float clipSeconds)
        {
            CancelPlayingWeapon();
            HideAllWeapons();

            if (!TryGetDefinition(beatType, out PlayerAttackWeaponConfig definition))
            {
                Debug.LogError($"BeatType {beatType} に対応する武器設定が見つかりませんでした。");
                return;
            }

            if (definition.WeaponItem == null)
            {
                Debug.LogError($"BeatType {beatType} の武器Viewが未設定です。", this);
                return;
            }

            _currentWeaponView = definition.WeaponItem;
            _cts = new CancellationTokenSource();

            PlayWeaponAsync(_currentWeaponView, clipSeconds, _cts.Token);
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

        [SerializeField, Tooltip("BeatTypeごとの武器表示と攻撃SE設定。")]
        private PlayerAttackWeaponConfig[] _definitions;

        private WeaponItemView _currentWeaponView;
        private CancellationTokenSource _cts;

        private void Awake()
        {
            HideAllWeapons();
        }

        private void OnDisable()
        {
            CancelPlayingWeapon();
            HideAllWeapons();
        }

        /// <summary>
        ///     武器に応じた演出を再生する。
        /// </summary>
        /// <param name="weaponItemView"> 再生させるItemView。 </param>
        /// <param name="clipSeconds"> クリップの長さ。 </param>
        /// <param name="ct"> CancellationToken。 </param>
        /// <returns></returns>
        private async Awaitable PlayWeaponAsync(WeaponItemView weaponItemView, float clipSeconds, CancellationToken ct)
        {
            try
            {
                //TODO : アニメーション時間が短すぎて表示がわかりにくいため、今が+2秒いれている。
                await weaponItemView.PlayAsync(clipSeconds + 2, ct);
            }
            catch (OperationCanceledException)
            {
                weaponItemView.HideWeapon();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
                weaponItemView.HideWeapon();
            }
            finally
            {
                if (_currentWeaponView == weaponItemView)
                {
                    _currentWeaponView = null;
                }
            }
        }

        /// <summary>
        ///     再生中の武器表示をキャンセルします。
        /// </summary>
        private void CancelPlayingWeapon()
        {
            if (_cts == null)
            {
                return;
            }

            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
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
