using Mock.MusicBattle.Basis;
using System.Collections.Generic;
using Mock.MusicBattle.Character;
using Unity.Cinemachine;
using UnityEngine;
using Mock.MusicBattle.Battle;
using CriWare;
using System;
using Mock.MusicBattle.MusicSync;

namespace Mock.MusicBattle.Player
{
    /// <summary>
    ///     プレイヤーの動作と状態を管理するクラス。
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerManager : MonoBehaviour, ICharacter
    {
        #region Publicイベント
        /// <summary> プレイヤーが攻撃を行ったときに発火するイベント。 </summary>
        public event Action<float> OnAttacked;
        #endregion

        #region パブリックプロパティ
        /// <summary> プレイヤーのピボット位置を取得します。 </summary>
        public Vector3 Pivot => _pivotTransform.position;
        /// <summary> プレイヤーのHealthEntityを取得します。 </summary>
        public HealthEntity HealthEntity => _healthEntity;
        #endregion

        #region Publicメソッド
        /// <summary>
        ///     プレイヤーマネージャーを初期化します。
        /// </summary>
        /// <param name="inputBuffer">入力バッファ。</param>
        /// <param name="cinemachineCamera">シネマシーンカメラ。</param>
        /// <param name="lockOnManager">ロックオンマネージャー。</param>
        /// <param name="musicSync">音楽同期マネージャー。</param>
        public void Init(InputBuffer inputBuffer, CinemachineCamera cinemachineCamera,
            LockOnManager lockOnManager, MusicSyncManager musicSync)
        {
            _inputBuffer = inputBuffer;
            _lockOnManager = lockOnManager;
            Rigidbody rb = GetComponent<Rigidbody>();
            _animController = GetComponent<PlayerAnimationController>();
            _healthEntity = new HealthEntity(_playerStatus.MaxHealth);
            _playerAttacker = new PlayerAttacker(_playerStatus, _config, this, musicSync);
            _playerMover = new PlayerMover(_playerStatus, rb, transform, cinemachineCamera.transform);
            _musicSyncManager = musicSync;
            InputEventRegister(_inputBuffer);
        }

        /// <summary>
        ///     プレイヤーにダメージを与えます。
        /// </summary>
        /// <param name="damage">与えるダメージ量。</param>
        public void TakeDamage(float damage) => _healthEntity.TakeDamage(damage);
        #endregion

        #region インスペクター表示フィールド
        [Header("データ")]
        /// <summary> プレイヤーのステータス。 </summary>
        [SerializeField, Tooltip("プレイヤーのステータス。")]
        private PlayerStatus _playerStatus;
        /// <summary> プレイヤーの設定。 </summary>
        [SerializeField, Tooltip("プレイヤーの設定。")]
        private PlayerConfig _config;
        [SerializeField, Tooltip("拍子情報")]
        private SignatureDatabase _signatureDatabase;
        [Header("オブジェクト情報")]
        /// <summary> プレイヤーのピボット位置。 </summary>
        [SerializeField, Tooltip("プレイヤーのピボット位置。")]
        private Transform _pivotTransform;
        /// <summary> 銃声のCriAtomSource。 </summary>
        [SerializeField, Tooltip("銃声のCriAtomSource。")]
        private CriAtomSource _gunSoundSource;
        #endregion

        #region プライベートフィールド
        /// <summary> プレイヤーのヘルスエンティティ。 </summary>
        private HealthEntity _healthEntity;
        /// <summary> 入力バッファ。 </summary>
        private InputBuffer _inputBuffer;
        /// <summary> ロックオンマネージャー。 </summary>
        private LockOnManager _lockOnManager;
        /// <summary> プレイヤーの移動処理。 </summary>
        private PlayerMover _playerMover;
        /// <summary> プレイヤーの攻撃処理。 </summary>
        private PlayerAttacker _playerAttacker;
        /// <summary> プレイヤーのアニメーションコントローラー。 </summary>
        private PlayerAnimationController _animController;
        /// <summary> 音楽同期マネージャー。 </summary>
        private MusicSyncManager _musicSyncManager;
        /// <summary> 現在の入力ベクトル。 </summary>
        private Vector2 _input;
        /// <summary> 現在の速度ベクトル。 </summary>
        private Vector3 _velocity;
        /// <summary> 地面に接触しているコリジョンを管理するハッシュセット。 </summary>
        private readonly HashSet<Collision> _hitGrounds = new();
        #endregion

        #region Unityライフサイクルメソッド
        /// <summary>
        ///     オブジェクトが無効になったときに呼び出されます。
        ///     入力イベントの登録を解除します。
        /// </summary>
        private void OnDisable()
        {
            if(_inputBuffer != null)
            InputEventUnregister(_inputBuffer);
        }

        /// <summary>
        ///     フレームごとに呼び出されます。
        ///     プレイヤーの移動とアニメーションを更新します。
        /// </summary>
        private void Update()
        {
            if (_playerMover != null)
            {
                _velocity = _playerMover.CalcPlayerVelocityByInputDirection(_input);
                _animController?.MoveVelocity(_velocity.magnitude);
                _playerMover.SetPlayerVelocity(_velocity);
                _playerMover.Update();
            }
        }

        /// <summary>
        ///     固定フレームレートで呼び出されます。
        ///     プレイヤーの物理的な移動を更新します。
        /// </summary>
        private void FixedUpdate()
        {
            if (_playerMover != null)
            {
                _playerMover.FixedUpdate();
            }
        }

        /// <summary>
        ///     他のコライダーとの衝突が始まったときに呼び出されます。
        ///     地面との接触を判定し、接地フラグを更新します。
        /// </summary>
        /// <param name="collision">衝突情報。</param>
        private void OnCollisionEnter(Collision collision)
        {
            if (_playerMover != null)
                if (collision.contacts.Length == 0) { return; }

            // 衝突面の法線ベクトルを取得して、地面との接触かどうかを判定する。
            Vector3 contactNormal = collision.contacts[0].normal;
            if (Vector3.Dot(contactNormal, Vector3.up) > 0.5f)
            {
                _hitGrounds.Add(collision);
                _playerMover.SetIsGround(0 < _hitGrounds.Count);
            }
        }

        /// <summary>
        ///     他のコライダーとの衝突が終了したときに呼び出されます。
        ///     地面との接触がなくなった場合に接地フラグを更新します。
        /// </summary>
        /// <param name="collision">衝突情報。</param>
        private void OnCollisionExit(Collision collision)
        {
            if (_hitGrounds.Remove(collision))
            {
                // 地面との接触がなくなった場合、接地フラグを更新する。
                _playerMover.SetIsGround(0 < _hitGrounds.Count);
            }
        }
        #endregion

        #region Privateメソッド
        /// <summary>
        ///     入力イベントを登録します。
        /// </summary>
        /// <param name="inputBuffer">入力バッファ。</param>
        private void InputEventRegister(InputBuffer inputBuffer)
        {
            if (inputBuffer == null)
            {
                Debug.LogError($"{nameof(InputBuffer)} がnullです。");
                return;
            }
            inputBuffer.MoveAction.Performed += OnInputMove;
            inputBuffer.MoveAction.Canceled += OnInputMoveCancel;
            inputBuffer.AttackAction.Started += OnInputAttack;
            _healthEntity.OnDeath += OnDeathAction;
        }

        /// <summary>
        ///     入力イベントの登録を解除します。
        /// </summary>
        /// <param name="inputBuffer">入力バッファ。</param>
        private void InputEventUnregister(InputBuffer inputBuffer)
        {
            inputBuffer.MoveAction.Performed -= OnInputMove;
            inputBuffer.MoveAction.Canceled -= OnInputMoveCancel;
            inputBuffer.AttackAction.Started -= OnInputAttack;
            _healthEntity.OnDeath -= OnDeathAction;
        }

        /// <summary>
        ///     移動入力があったときに呼び出されます。
        /// </summary>
        /// <param name="input">入力ベクトル。</param>
        private void OnInputMove(Vector2 input)
        {
            _input = input;
        }

        /// <summary>
        ///     移動入力がキャンセルされたときに呼び出されます。
        /// </summary>
        /// <param name="input">入力ベクトル。</param>
        private void OnInputMoveCancel(Vector2 input)
        {
            _input = Vector2.zero;
        }

        /// <summary>
        ///     攻撃入力があったときに呼び出されます。
        /// </summary>
        /// <param name="input">入力値（未使用）。</param>
        private void OnInputAttack(float input)
        {
            if (_playerAttacker != null)
            {
                ICharacter target = _lockOnManager.LockOnTarget;
                float signature = _musicSyncManager.GetInputTimeSignature();
                _playerAttacker.Attack(target, signature);
                if (_gunSoundSource != null)
                {
                    _gunSoundSource.cueName = _signatureDatabase.GetSeCueNameBySignature(signature);
                    _gunSoundSource.Play();
                }

                _playerAttacker.CheckPatternMatch(out float _);

                OnAttacked?.Invoke(signature);
                _playerMover.OnAttack(_playerAttacker.MoveLockTask);
            }
        }

        /// <summary>
        ///     プレイヤーが死亡したときに呼び出されます。
        /// </summary>
        private void OnDeathAction()
        {
            Debug.Log("Player Dead");
            gameObject.SetActive(false);
        }
        #endregion

        #region デバッグ
        private void OnValidate()
        {
            Debug.Assert(_playerStatus != null, "プレイヤーステータスがありません。", this);
            Debug.Assert(_config != null, "プレイヤーのコンフィグがありません。", this);
        }
        #endregion
    }
}


