using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Research.SaveSystem
{
    public class LoadViewController : MonoBehaviour
    {
        /// <summary>
        ///     初期化処理。
        /// </summary>
        /// <param name="loadGame"></param>
        /// <param name="saveLoadEvents"></param>
        public void Initialize(LoadGame loadGame)
        {
            _loadGame = loadGame;
            EventBus<EOnLoadStart>.Register(OnLoadStart);
            EventBus<EOnLoadEnd>.Register(OnLoadEnd);
        }
        /// <summary>
        ///     ロードボタン押下時の処理。
        /// </summary>
        public void OnLoadButtonClick()
        {
            _loadGame.LoadGameAsync(SetupSaveData);
        }

        #region ライフサイクル
        private void OnDestroy()
        {
            EventBus<EOnLoadStart>.Unregister(OnLoadStart);
            EventBus<EOnLoadEnd>.Unregister(OnLoadEnd);
        }
        #endregion

        [SerializeField, Tooltip("")]
        private GameObject _shelter;
        [SerializeField, Tooltip("")]
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

        private LoadGame _loadGame;

        /// <summary>
        ///     ロードしたデータを画面に設定する。
        /// </summary>
        /// <param name="saveData"></param>
        private void SetupSaveData(KillChordGameData saveData)
        {
            // デバッグ用処理
            SetupPlayerStatus(saveData);
            SetupStoryProgress(saveData);
            SetupEquipmentUnlock(saveData);
            SetupSkillUnlock(saveData);
        }

        #region デバッグ用
        private void SetupPlayerStatus(KillChordGameData saveData)
        {
            _inputEquipments.text = string.Join(',', saveData.PlayerData.Equipment);
            _inputSkills.text = string.Join(',', saveData.PlayerData.Skill);
        }

        private void SetupStoryProgress(KillChordGameData saveData)
        {
            HashSet<int> stories = saveData.OutGameData.StoryProgress;
            for (int i = 0; i < _chkboxStoryProgress.Length; i++)
            {
                int id = _chkboxStoryProgress[i].GetComponent<StoryProgress>().Id;
                if (stories.Contains(id))
                {
                    _chkboxStoryProgress[i].isOn = true;
                }
                else
                {
                    _chkboxStoryProgress[i].isOn = false;
                }
            }
        }
        private void SetupEquipmentUnlock(KillChordGameData saveData)
        {
            HashSet<int> equipments = saveData.OutGameData.EquipmentUnlock;
            for (int i = 0; i < _chkboxEquipment.Length; i++)
            {
                int id = _chkboxEquipment[i].GetComponent<EquipmentData>().Id;
                if (equipments.Contains(id))
                {
                    _chkboxEquipment[i].isOn = true;
                }
                else
                {
                    _chkboxEquipment[i].isOn = false;
                }
            }
        }
        private void SetupSkillUnlock(KillChordGameData saveData)
        {
            HashSet<int> skills = saveData.OutGameData.SkillUnlock;
            for (int i = 0; i < _chkboxSkill.Length; i++)
            {
                int id = _chkboxSkill[i].GetComponent<SkillData>().Id;
                if (skills.Contains(id))
                {
                    _chkboxSkill[i].isOn = true;
                }
                else
                {
                    _chkboxSkill[i].isOn = false;
                }
            }
        }

        private void OnLoadStart(EOnLoadStart eventParam)
        {
            _shelter.SetActive(true);
            _shelterText.text = "Loading...";
        }

        private void OnLoadEnd(EOnLoadEnd eventParam)
        {
            _shelter.SetActive(false);
        }
        #endregion
    }
}