using Mock.MusicBattle.Character;
using Mock.MusicBattle.MusicSync;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Mock.MusicBattle.Player
{
    /// <summary>
    ///    プレイヤーの攻撃処理をする。
    /// </summary>
    public class PlayerAttacker
    {
        #region コンストラクタ
        /// <summary>
        ///    コンストラクタ。
        /// </summary>
        public PlayerAttacker(PlayerStatus status, PlayerConfig config, PlayerManager player, MusicSyncManager musicSyncManager)
        {
            _status = status;
            _config = config;
            _player = player;
            _musicSyncManager = musicSyncManager;
        }
        #endregion

        #region プロパティ
        public Task MoveLockTask => _moveLockTask;
        /// <summary> 攻撃硬直が有効かどうか </summary>
        public bool IsMoveLock =>
            _moveLockTask != null && !_moveLockTask.IsCompleted;
        #endregion

        #region Publicメソッド
        /// <summary>
        ///     指定されたターゲットに攻撃を行います。
        /// </summary>
        /// <param name="target">攻撃対象。</param>
        /// <param name="signature">攻撃の威力を決定する拍子。</param>
        /// <returns> 攻撃が成功したかどうか。 </returns>
        public bool Attack(ICharacter target, float signature)
        {
            if (target == null) { return false; }

            Vector3 origin = _player.transform.position + Vector3.up * HEIGHT_RAY;

            #region デバッグ用
            AttackGizmoLine(origin, (target.Pivot - origin).normalized);
            #endregion

            if (!CanAttackTarget(origin, target))
            {
                Debug.Log("Attack target not found or not reachable.");
                return false;
            }

            float attackPower = _status.AttackPower * 4 / signature;
            target.TakeDamage(attackPower);

            // MusicSyncのSignature履歴を取得し、特定のパターンと一致するかチェックする。
            for (int i = 0; i < _status.SpecialAttackPatterns.Length; i++)
            {
                RythemPatternData data = _status.SpecialAttackPatterns[i];
                if (_musicSyncManager.IsMatchInputTimeSignature(data))
                {
                    Debug.Log($"MusicSync Signature Pattern Matched! Pattern: {string.Join(", ", data.SignaturePattern.ToArray())}");
                }
            }

            _moveLockTask = PostAttackMoveLockAsync();
            return true;
        }
        #endregion

        #region 定数
        /// <summary> レイキャストの高さオフセット。 </summary>
        private const float HEIGHT_RAY = 0.7f;
        #endregion

        #region プライベートフィールド
        /// <summary> プレイヤーのマネージャクラス。 </summary>
        private readonly PlayerManager _player;
        /// <summary> プレイヤーのステータス。 </summary>
        private readonly PlayerStatus _status;
        /// <summary> プレイヤーの設定。 </summary>
        private readonly PlayerConfig _config;
        /// <summary> 音楽同期システムのマネージャ。 </summary>
        private readonly MusicSyncManager _musicSyncManager;

        private Task _moveLockTask;
        #endregion

        #region Privateメソッド
        /// <summary>
        ///     指定されたターゲットが攻撃可能かどうかを判定します。
        /// </summary>
        /// <param name="origin">レイキャストの開始点。</param>
        /// <param name="target">確認するターゲット。</param>
        /// <returns>攻撃可能な場合はtrue、それ以外はfalse。</returns>
        private bool CanAttackTarget(Vector3 origin, ICharacter target)
        {
            if (Physics.Raycast(origin, (target.Pivot - origin).normalized,
                    out RaycastHit hitInfo,
                    _status.AttackRange, ~_config.IgnoreAttackLayer))
            {
                Rigidbody rb = hitInfo.collider.attachedRigidbody;
                Debug.Log($"Hit: {hitInfo.collider.name} {rb?.name}");

                return (rb?.GetComponent<ICharacter>() ?? null) == target;
            }

            return false;
        }

        private async Task PostAttackMoveLockAsync()
        {
            try
            {
                float d = _status.PostAttackMoveLockDuration;
                CancellationToken token = _player.destroyCancellationToken;
                await Awaitable.WaitForSecondsAsync(d, token);
            }
            catch (OperationCanceledException) { return; }
        }
        #endregion

        #region デバッグ
        private void AttackGizmoLine(Vector3 origin, Vector3 direction)
        {
            Vector3 s = origin;
            Vector3 e = origin + direction * _status.AttackRange;
            Debug.DrawLine(s, e, Color.red, 2);
        }
        #endregion
    }
}