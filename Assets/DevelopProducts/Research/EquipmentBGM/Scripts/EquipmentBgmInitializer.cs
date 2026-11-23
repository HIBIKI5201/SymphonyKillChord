using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.Domain.OutGame.SkillBuild;
using KillChord.Runtime.View.Persistent.Music;
using SymphonyFramework.Services;
using UnityEngine;

namespace DevelopProducts.EquipmentBGM
{
    /// <summary>
    ///     EquipmentBGMモジュールの初期化を行うクラス。
    ///     ServiceLocatorから各依存を取得してEquipmentBgmServiceを生成し登録する。
    /// </summary>
    public class EquipmentBgmInitializer : MonoBehaviour
    {
        [SerializeField, Tooltip("スキルIDとCRIセレクターラベル名の対応表。")]
        private BgmSelectorLabelTable _table;
        [SerializeField, Tooltip("シーケンスを1ステップ進める間隔（小節数）。")]
        private int _measuresPerStep = 1;

        /// <summary> 生成したEquipmentBgmService。 </summary>
        private EquipmentBgmService _service;
        /// <summary> 拍情報を参照する音楽同期状態。 </summary>
        private MusicSyncState _musicSyncState;

        /// <summary>
        ///     ServiceLocatorから各依存が登録されるまで待機してEquipmentBgmServiceを初期化する。
        /// </summary>
        private async void Start()
        {
            MusicPlayer musicPlayer = await Partitura.GetAsync<MusicPlayer>();
            SkillBuildDefinition skillBuild = await Partitura.GetAsync<SkillBuildDefinition>();
            _musicSyncState = await Partitura.GetAsync<MusicSyncState>();

            _service = new EquipmentBgmService(musicPlayer, _table, skillBuild, _measuresPerStep);
            Partitura.Register(_service, ServiceLifetimeEnum.Persistent);
        }

        /// <summary>
        ///     毎フレーム、現在の拍をServiceに渡して小節の切り替えを駆動する。
        /// </summary>
        private void Update()
        {
            if (_service == null || _musicSyncState == null) return;
            _service.Tick(_musicSyncState.CurrentBeat);
        }

        /// <summary>
        ///     破棄時にServiceLocatorからEquipmentBgmServiceの登録を解除する。
        /// </summary>
        private void OnDestroy()
        {
            Partitura.Unregister<EquipmentBgmService>();
        }
    }
}
