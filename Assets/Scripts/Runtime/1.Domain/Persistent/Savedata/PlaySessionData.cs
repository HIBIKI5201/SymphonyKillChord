using KillChord.Runtime.Utility.Identity;
using SymphonyFrameWork.System.SaveSystem;
using System;
using UnityEngine;

namespace KillChord.Runtime.Domain.Persistent.Savedata
{
    /// <summary>
    ///     最後にいた画面と選択中のステージを保持し、次回起動時の再開地点を表すセーブデータ。
    ///     <para>
    ///         常駐シーンはこのデータを読み込んで最初に開くシーンを決める。
    ///         エディタではセーブデータ管理ウィンドウからこのデータを書き換えると、任意のステージから開始できる。
    ///     </para>
    /// </summary>
    [Serializable]
    public sealed class PlaySessionData : SaveDataContent
    {
        /// <summary> 再開する画面の種類。 </summary>
        public PlaySessionScene Scene => _scene;

        /// <summary> 再開するステージのID。ステージを伴わない画面では0。 </summary>
        public int StageId => _stageId.Id;

        /// <summary> タイトルから開始したオープニングチュートリアルのシナリオを再開する場合はtrue。 </summary>
        public bool IsOpeningTutorialScenario => _isOpeningTutorialScenario;

        /// <summary> 再開する画面が記録されている場合はtrue。 </summary>
        public bool HasSession => _scene != PlaySessionScene.None;

        /// <summary>
        ///     アウトゲームを再開地点として記録します。
        /// </summary>
        /// <returns> 記録内容が変わった場合はtrue。 </returns>
        public bool RecordOutGame()
        {
            return Apply(PlaySessionScene.OutGame, 0, false);
        }

        /// <summary>
        ///     シナリオステージの開始地点を再開地点として記録します。
        /// </summary>
        /// <param name="stageId"> 再開するシナリオステージのID。 </param>
        /// <param name="isOpeningTutorialScenario"> オープニングチュートリアルのシナリオの場合はtrue。 </param>
        /// <returns> 記録内容が変わった場合はtrue。 </returns>
        public bool RecordScenario(int stageId, bool isOpeningTutorialScenario)
        {
            ValidateStageId(stageId);
            return Apply(PlaySessionScene.Scenario, stageId, isOpeningTutorialScenario);
        }

        /// <summary>
        ///     バトルステージの開始地点を再開地点として記録します。
        /// </summary>
        /// <param name="stageId"> 再開するバトルステージのID。 </param>
        /// <returns> 記録内容が変わった場合はtrue。 </returns>
        public bool RecordInGame(int stageId)
        {
            ValidateStageId(stageId);
            return Apply(PlaySessionScene.InGame, stageId, false);
        }

        /// <summary>
        ///     再開地点を消去し、次回はタイトルから始まるようにします。
        /// </summary>
        /// <returns> 記録内容が変わった場合はtrue。 </returns>
        public bool Clear()
        {
            return Apply(PlaySessionScene.None, 0, false);
        }

        [SerializeField, Tooltip("再開する画面。Noneならタイトルから始まる。")]
        private PlaySessionScene _scene = PlaySessionScene.None;

        [SerializeField, Tooltip("再開するステージ。ScenarioならシナリオステージのID、InGameならバトルステージのIDを選ぶ。")]
        [SourceDataCollection("StageAsset")]
        private DataID _stageId;

        [SerializeField, Tooltip("タイトルから始めるオープニングチュートリアルのシナリオとして再開する場合はオンにする。")]
        private bool _isOpeningTutorialScenario;

        /// <summary>
        ///     ステージIDが有効か検証します。
        /// </summary>
        /// <param name="stageId"> 検証するステージID。 </param>
        private static void ValidateStageId(int stageId)
        {
            if (stageId == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(stageId), stageId, "ステージIDに0は使用できません。");
            }
        }

        /// <summary>
        ///     記録内容を差し替えます。
        /// </summary>
        /// <param name="scene"> 再開する画面。 </param>
        /// <param name="stageId"> 再開するステージのID。 </param>
        /// <param name="isOpeningTutorialScenario"> オープニングチュートリアルのシナリオの場合はtrue。 </param>
        /// <returns> 記録内容が変わった場合はtrue。 </returns>
        private bool Apply(PlaySessionScene scene, int stageId, bool isOpeningTutorialScenario)
        {
            if (_scene == scene
                && _stageId.Id == stageId
                && _isOpeningTutorialScenario == isOpeningTutorialScenario)
            {
                return false;
            }

            _scene = scene;
            _stageId = DataID.FromHash(stageId);
            _isOpeningTutorialScenario = isOpeningTutorialScenario;
            return true;
        }
    }
}
