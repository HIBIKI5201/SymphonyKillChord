using KillChord.Runtime.Adaptor.InGame.Skill;
using KillChord.Runtime.Domain.OutGame.SkillBuild;
using KillChord.Runtime.View.Persistent.Music;
using SymphonyFrameWork.System.ServiceLocate;
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

        private EquipmentBgmService _service;

        /// <summary>
        ///     ServiceLocatorから各依存が登録されるまで待機してEquipmentBgmServiceを初期化する。
        /// </summary>
        private async void Start()
        {
            MusicPlayer musicPlayer = await ServiceLocator.GetInstanceAsync<MusicPlayer>();
            SkillController skillController = await ServiceLocator.GetInstanceAsync<SkillController>();
            SkillBuildDefinition skillBuild = await ServiceLocator.GetInstanceAsync<SkillBuildDefinition>();

            _service = new EquipmentBgmService(musicPlayer, _table, skillController, skillBuild);
            ServiceLocator.RegisterInstance(_service);
        }

        /// <summary>
        ///     破棄時にServiceLocatorからEquipmentBgmServiceの登録を解除し、イベント購読を解除する。
        /// </summary>
        private void OnDestroy()
        {
            _service?.Dispose();
            ServiceLocator.UnregisterInstance<EquipmentBgmService>();
        }
    }
}
