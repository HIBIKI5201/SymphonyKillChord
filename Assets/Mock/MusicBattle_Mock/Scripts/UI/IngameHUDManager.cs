using Mock.MusicBattle.Battle;
using Mock.MusicBattle.Character;
using Mock.MusicBattle.MusicSync;
using SymphonyFrameWork.Utility;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mock.MusicBattle.UI
{
    /// <summary>
    ///     インゲームのHUDを管理するクラス。
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    [RequireComponent(typeof(UIDocument))]
    public class IngameHUDManager : MonoBehaviour
    {
        #region Publicメソッド
        /// <summary>
        ///     HUDマネージャーを初期化します。
        /// </summary>
        /// <param name="musicBuffer">音楽バッファ。</param>
        public void Initialize(IMusicBuffer musicBuffer)
        {
            _musicBuffer = musicBuffer;
        }

        /// <summary>
        ///     プレイヤーのヘルスバーを初期化します。
        /// </summary>
        /// <param name="healthEntity">プレイヤーのHealthEntity。</param>
        public async void InitializePlayerHealthBar(HealthEntity healthEntity)
        {
            await SymphonyTask.WaitUntil(() => _isInitialized, destroyCancellationToken);
            _playerHealthBar?.BindData(healthEntity);
        }

        /// <summary>
        ///     敵のヘルスバーを追加します。
        /// </summary>
        /// <param name="healthEntity">敵のHealthEntity。</param>
        /// <param name="transform">敵のTransform。</param>
        /// <returns>追加されたEnemyHealthBarインスタンス。</returns>
        public async Task<EnemyHealthBar> AddEnemyHealthBar(HealthEntity healthEntity, Transform transform)
        {
            await SymphonyTask.WaitUntil(() => _root != null, destroyCancellationToken);

            // 敵のヘルスバーを生成して初期化する。
            EnemyHealthBar enemyHealthBar = new EnemyHealthBar();
            _root.Add(enemyHealthBar);
            enemyHealthBar.BindData(healthEntity, transform);

            return enemyHealthBar;
        }

        /// <summary>
        ///     ロックオンカーソルを初期化します。
        /// </summary>
        /// <param name="lockOnManager">ロックオンマネージャー。</param>
        public async void InitializeLockOnCursor(LockOnManager lockOnManager)
        {
            await SymphonyTask.WaitUntil(() => _isInitialized, destroyCancellationToken);

            lockOnManager.OnTargetLocked += _lockOnCursor.RegisterTarget;
            _lockOnCursor.RegisterCallback<DetachFromPanelEvent>(
                evt => lockOnManager.OnTargetLocked -= _lockOnCursor.RegisterTarget);
        }

        /// <summary>
        ///     ダメージテキストを表示します。
        /// </summary>
        /// <param name="damage">与えられたダメージ量。</param>
        /// <param name="position">ダメージテキストを表示するワールド座標。</param>
        public void ShowDamageText(float damage, Vector3 position)
        {
            _damageTextPool.ShowDamageText(damage, position);
        }

        /// <summary>
        ///     ノーツを生成します。
        /// </summary>
        /// <param name="measure">ノーツを表示する拍。</param>
        /// <param name="signature">ノーツの色を決定する拍子。</param>
        public void CreateNote(float measure, float signature)
        {
            Color color = GetMeasureColor(signature);
            _musicSyncStaffNotation.CreateNotes(measure, color);
        }
        #endregion

        #region インスペクター表示フィールド
        /// <summary> 拍子と色の対応データ配列。 </summary>
        [SerializeField, Tooltip("拍子と色の対応データ配列。")]
        private SignatureColorData[] _signatureColorDatas;
        #endregion

        #region プライベートフィールド
        /// <summary> UIドキュメント。 </summary>
        private UIDocument _document;
        /// <summary> ルートVisualElement。 </summary>
        private VisualElement _root;
        /// <summary> プレイヤーのヘルスバーUI。 </summary>
        private PlayerHealthBar _playerHealthBar;
        /// <summary> ダメージテキストのプール。 </summary>
        private DamageTextPool _damageTextPool;
        /// <summary> ロックオンカーソルUI。 </summary>
        private LockOnCursor _lockOnCursor;
        /// <summary> 音楽同期の五線譜UI。 </summary>
        private MusicSyncStaffNotation _musicSyncStaffNotation;
        /// <summary> 音楽バッファインターフェース。 </summary>
        private IMusicBuffer _musicBuffer;
        /// <summary> 初期化が完了したかどうかを示すフラグ。 </summary>
        private bool _isInitialized = false;
        #endregion

        #region Unityライフサイクルメソッド
        /// <summary>
        ///     スクリプトインスタンスがロードされたときに呼び出されます。
        ///     UI要素の初期化とバインドを行います。
        /// </summary>
        private void Start()
        {
            if (TryGetComponent(out _document))
            {
                _root = _document.rootVisualElement;
                if (_root == null)
                {
                    Debug.LogError("rootVisualElement の取得に失敗しました。");
                    return;
                }

                _playerHealthBar = new PlayerHealthBar();
                _musicSyncStaffNotation = new MusicSyncStaffNotation();
                _lockOnCursor = new LockOnCursor();
                _damageTextPool = new(_root);
                _root.Add(_playerHealthBar);
                _root.Add(_musicSyncStaffNotation);
                _root.Add(_lockOnCursor);

                _isInitialized = true;
            }
        }

        /// <summary>
        ///     フレームごとに呼び出されます。
        ///     音楽同期UIとロックオンカーソルの位置を更新します。
        /// </summary>
        private void Update()
        {
            if (_musicBuffer == null || _musicSyncStaffNotation == null) { return; }

            _musicSyncStaffNotation.Update(Time.deltaTime, (float)(_musicBuffer.CurrentBeat / 4d));
            _lockOnCursor?.UpdatePosition();
        }
        #endregion

        #region プライベートStruct定義
        /// <summary>
        ///     拍子と色の関連付けデータ。
        /// </summary>
        [Serializable]
        private struct SignatureColorData
        {
            /// <summary> 拍子を取得します。 </summary>
            public float Signature => _signature;
            /// <summary> 色を取得します。 </summary>
            /// <summary> 色を取得します。 </summary>
            public Color Color => _color;

            /// <summary> 拍子。 </summary>
            [SerializeField, Tooltip("拍子。")]
            private float _signature;
            /// <summary> 色。 </summary>
            [SerializeField, Tooltip("色。")]
            private Color _color;
        }
        #endregion

        #region Privateメソッド
        /// <summary>
        ///     指定された拍子に対応する色を取得します。
        /// </summary>
        /// <param name="signature">拍子。</param>
        /// <returns>対応する色。見つからない場合は黒を返します。</returns>
        private Color GetMeasureColor(float signature)
        {
            int s = Mathf.RoundToInt(signature);

            foreach (var data in _signatureColorDatas)
            {
                if (data.Signature == s)
                {
                    return data.Color;
                }
            }

            return Color.black;
        }
        #endregion
    }
}
