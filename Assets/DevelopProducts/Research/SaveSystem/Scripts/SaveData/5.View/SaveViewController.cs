using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace Research.SaveSystem
{
    /// <summary>
    ///     画面入力からセーブデータを処理するコンポーネント。
    /// </summary>
    public class SaveViewController : MonoBehaviour
    {
        /// <summary>
        ///     初期化処理。
        /// </summary>
        /// <param name="saveData"></param>
        /// <param name="saveGame"></param>
        /// <param name="saveLoadEvents"></param>
        public void Initialize(ISaveService saveGame)
        {
            _saveGame = saveGame;
            EventBus<EOnSaveStart>.Register(OnSaveStart);
            EventBus<EOnSaveEnd>.Register(OnSaveEnd);
        }
        /// <summary>
        ///     Saveボタン押下処理。
        /// </summary>
        public void OnSaveGameButtonClick()
        {
            KillChordGameData newData = new();
            SetSaveData(newData);
            _saveGame.Save(newData);
        }

        #region ライフサイクル
        private void OnDestroy()
        {
            EventBus<EOnSaveStart>.Unregister(OnSaveStart);
            EventBus<EOnSaveEnd>.Unregister(OnSaveEnd);
        }
        #endregion

        [SerializeField, Tooltip("UI遮蔽物")]
        private GameObject _shelter;
        [SerializeField, Tooltip("UI遮蔽物の文字")]
        private Text _shelterText;
        [SerializeField, Tooltip("")]
        private InputField _inputEquipments;
        [SerializeField, Tooltip("")]
        private InputField _inputSkills;
        [SerializeField, Tooltip("")]
        private Toggle[] _chkboxStoryProgress;
        [SerializeField, Tooltip("")]
        private Toggle[] _chkboxEquipment;
        [SerializeField, Tooltip("")]
        private Toggle[] _chkboxSkill;

        private ISaveService _saveGame;

        #region プライベートメソッド
        /// <summary>
        ///     画面の入力からセーブデータを設定する。
        /// </summary>
        /// <param name="newData"></param>
        private void SetSaveData(KillChordGameData newData)
        {
            ReadPlayerStatus(newData);
            ReadStoryProgress(newData);
            ReadEquipmentUnlock(newData);
            ReadSkillUnlock(newData);
        }
        #endregion

        #region デバッグ用
        private void ReadPlayerStatus(KillChordGameData newData)
        {
            // デバッグ用処理
            newData.PlayerData.Equipment = _inputEquipments.text.Split(',').Select(int.Parse).ToList();
            newData.PlayerData.Skill = _inputSkills.text.Split(',').Select(int.Parse).ToList();
        }
        private void ReadStoryProgress(KillChordGameData newData)
        {
            HashSet<int> progress = new();
            for (int i = 0; i < _chkboxStoryProgress.Length; i++)
            {
                if (_chkboxStoryProgress[i].isOn)
                {
                    progress.Add(_chkboxStoryProgress[i].GetComponent<StoryProgress>().Id);
                }
            }
            newData.OutGameData.StoryProgress = progress;
        }
        private void ReadEquipmentUnlock(KillChordGameData newData)
        {
            HashSet<int> equipments = new HashSet<int>();
            for (int i = 0; i < _chkboxEquipment.Length; i++)
            {
                if (_chkboxEquipment[i].isOn)
                {
                    equipments.Add(_chkboxEquipment[i].GetComponent<EquipmentData>().Id);
                }
            }
            newData.OutGameData.EquipmentUnlock = equipments;
        }
        private void ReadSkillUnlock(KillChordGameData newData)
        {
            HashSet<int> skills = new HashSet<int>();
            for (int i = 0; i < _chkboxSkill.Length; i++)
            {
                if (_chkboxSkill[i].isOn)
                {
                    skills.Add(_chkboxSkill[i].GetComponent<SkillData>().Id);
                }
            }
            newData.OutGameData.SkillUnlock = skills;
        }
        private void OnSaveStart(EOnSaveStart eventParam)
        {
            _shelter.SetActive(true);
            _shelterText.text = "Saving...";
        }

        private void OnSaveEnd(EOnSaveEnd eventParam)
        {
            _shelter.SetActive(false);
        }
        #endregion

    }
}