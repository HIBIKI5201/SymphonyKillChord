using Mock.MusicBattle.Character;
using Mock.MusicBattle.MusicSync;
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
        public void Initialize(IMusicBuffer musicBuffer)
        {
            _musicBuffer = musicBuffer;
        }

        /// <summary>
        ///     プレイヤーのヘルスバーを初期化する。
        /// </summary>
        /// <param name="healthEntity"></param>
        public void InitializePlayerHealthBar(HealthEntity healthEntity) =>
            _playerHealthBar?.BindData(healthEntity);

        /// <summary>
        ///     敵のヘルスバーを追加する。
        /// </summary>
        /// <param name="healthEntity"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public async Task<EnemyHealthBar> AddEnemyHealthBar(HealthEntity healthEntity, Transform transform)
        {
            while (_root == null) await Awaitable.NextFrameAsync();

            // 敵のヘルスバーを生成して初期化する。
            EnemyHealthBar enemyHealthBar = new EnemyHealthBar();
            _root.Add(enemyHealthBar);
            enemyHealthBar.BindData(healthEntity, transform);

            return enemyHealthBar;
        }

        /// <summary>
        ///     ダメージテキストを表示する。
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="position"></param>
        public void ShowDamageText(float damage, Vector3 position)
        {
            _damageTextPool.ShowDamageText(damage, position);
        }

        public void CreateNote(float measure, float signature)
        {
            Color color = GetMeasureColor(signature);
            _musicSyncStaffNotation.CreateNotes(measure, color);
        }

        [SerializeField]
        private SignetureColorData[] _measureColorDatas;

            
        private UIDocument _document;
        private VisualElement _root;

        private PlayerHealthBar _playerHealthBar;
        private DamageTextPool _damageTextPool;
        private MusicSyncStaffNotation _musicSyncStaffNotation;
        private IMusicBuffer _musicBuffer;

        [Serializable]
        private struct SignetureColorData
        {
            public float Signeture => _measure;
            public Color Color => _color;

            [SerializeField]
            private float _measure;
            [SerializeField]
            private Color _color;
        }

        private void Start()
        {
            if (TryGetComponent(out _document))
            {
                _root = _document.rootVisualElement;
                if (_root == null)
                {
                    Debug.LogError("rootVisualElement の取得に失敗しました");
                    return;
                }

                _playerHealthBar = new PlayerHealthBar();
                _musicSyncStaffNotation = new MusicSyncStaffNotation();
                _damageTextPool = new(_root);
                _root.Add(_playerHealthBar);
                _root.Add(_musicSyncStaffNotation);
            }
        }

        private void Update()
        {
            if (_musicBuffer == null || _musicSyncStaffNotation == null) { return; }

            _musicSyncStaffNotation.Update(Time.deltaTime, (float)(_musicBuffer.CurrentBeat / 4d));
        }

        private Color GetMeasureColor(float signature)
        {
            int s = Mathf.RoundToInt(signature);

            foreach (var data in _measureColorDatas)
            {
                if (data.Signeture == s)
                {
                    return data.Color;
                }
            }

            return Color.black;
        }
    }
}