using KillChord.Runtime.Adaptor.InGame.Enemy;
using LitMotion;
using LitMotion.Extensions;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     ダメージ数値を表示するクラス。
    /// </summary>
    public class DamageNumberView : MonoBehaviour
    {
        /// <summary>
        ///     ダメージ数値の表示を開始する。
        /// </summary>
        /// <param name="dTO">ダメージ数値DTO</param>
        /// <param name="completed">表示完了時のコールバック</param>
        public void Play(in DamageNumberDTO dTO, Action<DamageNumberView> completed)
        {
            ResetMotion();
            _completed = completed;

            if (_damageText == null)
            {
                Debug.LogError("[DamageNumberView] TMP_Text が未設定です。", this);
                return;
            }

            _damageText.SetText("{0}", Mathf.CeilToInt(dTO.Damage));

            ApplyRandomPosition();
            ApplyStyle(dTO.Type);
            PlayMovement();
            PlayFade();
        }

        /// <summary>
        ///     プールに戻すために初期化する。
        /// </summary>
        public void ResetView()
        {
            ResetMotion();
            _completed = null;
        }

        [SerializeField, Tooltip("ダメージ数値のテキスト")]
        private TMP_Text _damageText;

        [SerializeField, Tooltip("ダメージ数値の背景画像")]
        private Image _backGroundImage;

        [SerializeField, Tooltip("ダメージ種類ごとの表示設定")]
        private DamageNumberStyle[] _styles;

        [SerializeField, Tooltip("ダメージ数値の移動方向")]
        private DamageNumberExitType _exitType;

        [SerializeField, Tooltip("ダメージ演出の継続時間")]
        private float _duration;

        [SerializeField, Tooltip("ダメージ数値の移動距離")]
        private float _moveDistance;

        [SerializeField, Tooltip("ダメージ数値のランダム表示位置の最小値")]
        private Vector2 _randomPositionMin;

        [SerializeField, Tooltip("ダメージ数値のランダム表示位置の最大値")]
        private Vector2 _randomPositionMax;

        [SerializeField, Tooltip("イージングタイプ")]
        private Ease _easeType = Ease.OutQuad;

        private Action<DamageNumberView> _completed;
        private MotionHandle _movementHandle;
        private MotionHandle _fadeHandle;
        private MotionHandle _backgroundFadeHandle;

        private void OnDestroy()
        {
            ResetMotion();
            _completed = null;
        }

        /// <summary>
        ///     ダメージ数値の表示位置をランダムにずらす。
        /// </summary>
        private void ApplyRandomPosition()
        {
            float randomX = UnityEngine.Random.Range(_randomPositionMin.x, _randomPositionMax.x);
            float randomY = UnityEngine.Random.Range(_randomPositionMin.y, _randomPositionMax.y);

            Vector3 position = transform.localPosition;
            position.x += randomX;
            position.y += randomY;
            transform.localPosition = position;
        }

        /// <summary>
        ///     ダメージ種類に応じた表示スタイルを適用する。
        /// </summary>
        /// <param name="type">ダメージ種類</param>
        private void ApplyStyle(DamageNumberType type)
        {
            DamageNumberStyle style = FindStyle(type);

            if (style == null)
            {
                Debug.LogError($"[DamageNumberView] ダメージ種類 {type} に対応するスタイルが設定されていません。", this);
                HideBackground();
                return;
            }

            _damageText.color = style.TextColor;

            if (_backGroundImage == null)
            {
                return;
            }

            Sprite backgroundSprite = style.BackGroundSprite;

            // 通常時等表示しない場合は背景画像を非表示にする。
            bool hasBackground = backgroundSprite != null;

            _backGroundImage.sprite = backgroundSprite;
            _backGroundImage.enabled = hasBackground;

            if (hasBackground)
            {
                Color color = _backGroundImage.color;
                color.a = 1f;
                _backGroundImage.color = color;
            }
        }

        /// <summary>
        ///     背景画像を非表示にする。
        /// </summary>
        private void HideBackground()
        {
            if (_backGroundImage == null)
            {
                return;
            }

            _backGroundImage.sprite = null;
            _backGroundImage.enabled = false;
        }

        /// <summary>
        ///     ダメージ種類に対応するスタイルを取得する。
        /// </summary>
        /// <param name="type">ダメージ種類</param>
        /// <returns>対応するダメージスタイル、存在しない場合は null</returns>
        private DamageNumberStyle FindStyle(DamageNumberType type)
        {
            if (_styles == null)
            {
                return null;
            }

            for (int i = 0; i < _styles.Length; i++)
            {
                DamageNumberStyle style = _styles[i];

                if (style != null && style.Type == type)
                {
                    return style;
                }
            }
            return null;
        }

        /// <summary>
        ///     ダメージ数値の移動演出を再生する。
        /// </summary>
        private void PlayMovement()
        {
            switch (_exitType)
            {
                case DamageNumberExitType.MoveUp:
                    PlayVecticalMovement(_moveDistance);
                    break;

                case DamageNumberExitType.MoveDown:
                    PlayVecticalMovement(-_moveDistance);
                    break;

                case DamageNumberExitType.Stay:
                    break;

                default:
                    Debug.LogError($"[DamageNumberView] 不正な ExitType が設定されています。: {_exitType}", this);
                    break;
            }
        }

        /// <summary>
        ///    ダメージ数値の縦方向の移動演出を再生する。
        /// </summary>
        /// <param name="distance">移動距離</param>
        private void PlayVecticalMovement(float distance)
        {
            float startY = transform.localPosition.y;
            float endY = startY + distance;

            _movementHandle = LMotion.Create(startY, endY, _duration)
                .WithEase(_easeType)
                .Bind(value =>
                {
                    Vector3 pos = transform.localPosition;
                    pos.y = value;
                    transform.localPosition = pos;
                });
        }

        /// <summary>
        ///     ダメージ数値のフェードアウト演出を再生する。
        /// </summary>
        private void PlayFade()
        {
            _fadeHandle = LMotion.Create(_damageText.color.a, 0f, _duration)
                .WithEase(_easeType)
                .WithOnComplete(Complete)
                .BindToColorA(_damageText);

            if (_backGroundImage == null || !_backGroundImage.enabled)
            {
                return;
            }

            // 背景画像のフェードアウト演出
            _backgroundFadeHandle = LMotion.Create(_backGroundImage.color.a, 0f, _duration)
                .WithEase(_easeType)
                .BindToColorA(_backGroundImage);
        }

        /// <summary>
        ///     ダメージ数値の演出をリセットする。
        /// </summary>
        private void ResetMotion()
        {
            _movementHandle.TryCancel();
            _fadeHandle.TryCancel();
            _backgroundFadeHandle.TryCancel();
        }

        /// <summary>
        ///     ダメージ数値の演出が完了したことを通知する。
        /// </summary>
        private void Complete()
        {
            Action<DamageNumberView> completed = _completed;
            _completed = null;

            completed?.Invoke(this);
        }
    }
}