using KillChord.Develop.Composition.InGame.SkillEffect;
using KillChord.Runtime.Adaptor.InGame.Skill.Effect;
using KillChord.Runtime.Composition.InGame.Skill.Effect;
using KillChord.Runtime.Utility.Identity;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Develop.View.InGame.SkillEffect
{
    /// <summary>
    ///     デモシーンでスキルエフェクトを手動再生し、配置方式の挙動を確認するクラス。
    /// </summary>
    public sealed class SkillEffectDemoController : MonoBehaviour
    {
        [SerializeField, Tooltip("初期化完了を待機するデモ用ブートです。")]
        private SkillEffectDemoBoot _demoBoot;

        [SerializeField, Tooltip("プレイヤー役のTransformです。")]
        private Transform _playerTransform;

        [SerializeField, Tooltip("対象役のTransformです。")]
        private Transform _targetTransform;

        [SerializeField, SourceDataCollection("Skill")]
        [Tooltip("デモで再生するスキルIDの一覧です。")]
        private DataID[] _demoSkillIds;

        [SerializeField, Tooltip("対象を旋回させて追従型の挙動を確認するかです。")]
        private bool _movesTarget = true;

        [SerializeField, Tooltip("対象の旋回半径です。")]
        private float _targetOrbitRadius = 4f;

        [SerializeField, Tooltip("対象の旋回速度です。")]
        private float _targetOrbitSpeed = 60f;

        /// <summary>
        ///     初期化完了を待って再生用サービスを取得します。
        /// </summary>
        private async void Start()
        {
            CacheSkillLabels();

            if (_demoBoot != null)
            {
                await _demoBoot.WaitForInitializationAsync();
            }

            if (!ServiceLocator.TryGetInstance(out SkillEffectModuleContainer moduleContainer))
            {
                Debug.LogError(
                    $"[{nameof(SkillEffectDemoController)}] {nameof(SkillEffectModuleContainer)} が取得できませんでした。",
                    this);
                return;
            }

            _skillEffectPlayer = moduleContainer.SkillEffectPlayer;
        }

        /// <summary>
        ///     対象を旋回させます。
        /// </summary>
        private void Update()
        {
            if (!_movesTarget || _targetTransform == null || _playerTransform == null)
            {
                return;
            }

            // 追従型のエフェクトが対象へ付いてくるかを確認するため、対象を円周上で動かす。
            _orbitAngleDegrees = Mathf.Repeat(_orbitAngleDegrees + (_targetOrbitSpeed * Time.deltaTime), FULL_TURN_DEGREES);
            float radians = _orbitAngleDegrees * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(radians), 0f, Mathf.Sin(radians)) * _targetOrbitRadius;
            _targetTransform.position = _playerTransform.position + offset;
        }

        /// <summary>
        ///     デモ操作用のボタンを描画します。
        /// </summary>
        private void OnGUI()
        {
            if (_skillEffectPlayer == null)
            {
                GUI.Label(new Rect(GUI_MARGIN, GUI_MARGIN, GUI_WIDTH, GUI_LINE_HEIGHT), "初期化待機中...");
                return;
            }

            float y = GUI_MARGIN;
            GUI.Label(new Rect(GUI_MARGIN, y, GUI_WIDTH, GUI_LINE_HEIGHT), "スキルエフェクトデモ");
            y += GUI_LINE_HEIGHT;

            _movesTarget = GUI.Toggle(new Rect(GUI_MARGIN, y, GUI_WIDTH, GUI_LINE_HEIGHT), _movesTarget, "対象を旋回させる");
            y += GUI_LINE_HEIGHT;

            if (GUI.Button(new Rect(GUI_MARGIN, y, GUI_WIDTH, GUI_LINE_HEIGHT), "再生中を全停止"))
            {
                _skillEffectPlayer.StopAll();
            }

            y += GUI_LINE_HEIGHT + GUI_MARGIN;

            if (_demoSkillIds == null)
            {
                return;
            }

            for (int i = 0; i < _demoSkillIds.Length; i++)
            {
                int skillId = _demoSkillIds[i].Id;
                if (skillId == 0)
                {
                    continue;
                }

                if (GUI.Button(new Rect(GUI_MARGIN, y, GUI_WIDTH, GUI_LINE_HEIGHT), _skillLabels[i]))
                {
                    PlaySkillEffect(skillId);
                }

                y += GUI_LINE_HEIGHT;
            }
        }

        /// <summary>
        ///     ボタンに表示する文字列を一度だけ構築してキャッシュします。
        /// </summary>
        private void CacheSkillLabels()
        {
            if (_demoSkillIds == null)
            {
                _skillLabels = System.Array.Empty<string>();
                return;
            }

            // 文字列IDの逆引きはプロジェクト全アセットのロードを伴い、実行時には到底使えないため数値IDを表示する。
            _skillLabels = new string[_demoSkillIds.Length];
            for (int i = 0; i < _demoSkillIds.Length; i++)
            {
                _skillLabels[i] = "Skill " + _demoSkillIds[i].Id + " を再生";
            }
        }

        /// <summary>
        ///     指定スキルのエフェクトを再生します。
        /// </summary>
        /// <param name="skillId"> 再生するスキルのIDです。 </param>
        private void PlaySkillEffect(int skillId)
        {
            _skillEffectPlayer.PlaySkillEffect(skillId, CreateContext());
        }

        /// <summary>
        ///     現在のデモ状態からエフェクトContextを生成します。
        /// </summary>
        /// <returns> 生成したContextです。 </returns>
        private SkillEffectContext CreateContext()
        {
            Vector3 playerPosition = _playerTransform != null ? _playerTransform.position : Vector3.zero;
            Vector3 targetPosition = _targetTransform != null ? _targetTransform.position : playerPosition;
            Vector3 direction = targetPosition - playerPosition;
            if (direction.sqrMagnitude <= MINIMUM_SQR_MAGNITUDE)
            {
                direction = Vector3.forward;
            }

            return new SkillEffectContext(
                _playerTransform,
                _targetTransform,
                targetPosition,
                direction.normalized);
        }

        private const float FULL_TURN_DEGREES = 360f;
        private const float MINIMUM_SQR_MAGNITUDE = 0.0001f;
        private const float GUI_MARGIN = 10f;
        private const float GUI_WIDTH = 220f;
        private const float GUI_LINE_HEIGHT = 24f;

        private ISkillEffectPlayer _skillEffectPlayer;
        private string[] _skillLabels = System.Array.Empty<string>();
        private float _orbitAngleDegrees;
    }
}
