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
        public void Initialize(SaveGame saveGame, SaveLoadEvents saveLoadEvents)
        {
            _saveGame = saveGame;
            _saveLoadEvents = saveLoadEvents;
            _saveLoadEvents.OnSaveStart += ActivateShelter;
            _saveLoadEvents.OnSaveEnd += DeactivateShelter;
        }
        /// <summary>
        ///     Saveボタン押下処理。
        /// </summary>
        public void OnSaveGameButtonClick()
        {
            KillChordGameData newData = new();
            SetSaveData(newData);
            _saveGame.SaveGameAsync(newData);
        }

        #region ライフサイクル
        private void OnDestroy()
        {
            if (_saveLoadEvents == null) return;
            _saveLoadEvents.OnSaveStart -= ActivateShelter;
            _saveLoadEvents.OnSaveEnd -= DeactivateShelter;
        }
        #endregion

        [SerializeField, Tooltip("UI遮蔽物")]
        private GameObject _shelter;
        [SerializeField, Tooltip("UI遮蔽物の文字")]
        private Text _shelterText;
        [SerializeField, Tooltip("")]
        private InputField _inputGold;
        [SerializeField, Tooltip("")]
        private InputField _inputHpMax;
        [SerializeField, Tooltip("")]
        private InputField _inputAttack;
        [SerializeField, Tooltip("")]
        private InputField _inputCritRate;
        [SerializeField, Tooltip("")]
        private InputField _inputCritScale;
        [SerializeField, Tooltip("")]
        private InputField _inputEquipments;
        [SerializeField, Tooltip("")]
        private InputField _inputSkills;
        [SerializeField, Tooltip("")]
        private InputField _inputMissionProgress;
        [SerializeField, Tooltip("")]
        private Toggle[] _chkboxMission;
        [SerializeField, Tooltip("")]
        private Toggle[] _chkboxEquipment;
        [SerializeField, Tooltip("")]
        private Toggle[] _chkboxSkill;

        private SaveGame _saveGame;
        private SaveLoadEvents _saveLoadEvents;
        #region プライベートメソッド
        /// <summary>
        ///     画面の入力からセーブデータを設定する。
        /// </summary>
        /// <param name="newData"></param>
        private void SetSaveData(KillChordGameData newData)
        {
            ReadPlayerStatus(newData);
            ReadMissionProgress(newData);
            ReadMissionUnlock(newData);
            ReadEquipmentUnlock(newData);
            ReadSkillUnlock(newData);
        }
        #endregion

        #region デバッグ用
        private void ReadPlayerStatus(KillChordGameData newData)
        {
            // デバッグ用処理
            newData.Gold = long.Parse(_inputGold.text);
            newData.HpMax = float.Parse(_inputHpMax.text);
            newData.Attack = float.Parse(_inputAttack.text);
            newData.CritRate = float.Parse(_inputCritRate.text);
            newData.CritScale = float.Parse(_inputCritScale.text);
            newData.Equipments = _inputEquipments.text.Split(',').Select(int.Parse).ToList();
            newData.Skills = _inputSkills.text.Split(',').Select(int.Parse).ToList();
        }
        private void ReadMissionProgress(KillChordGameData newData)
        {
            List<int> progress = new List<int>();
            int[] missionIds = _inputMissionProgress.text.Split(',').Select(int.Parse).ToArray();
            for (int i = 0; i < missionIds.Length; i++)
            {
                progress.Add(missionIds[i]);
            }
            newData.MissionProgress = progress;
        }

        private void ReadMissionUnlock(KillChordGameData newData)
        {
            List<int> missions = new List<int>();
            for (int i = 0; i < _chkboxMission.Length; i++)
            {
                if (_chkboxMission[i].isOn)
                {
                    missions.Add(_chkboxMission[i].GetComponent<MissionData>().Id);
                }
            }
            newData.MissionUnlock = missions;
        }
        private void ReadEquipmentUnlock(KillChordGameData newData)
        {
            List<int> equipments = new List<int>();
            for (int i = 0; i < _chkboxEquipment.Length; i++)
            {
                if (_chkboxEquipment[i].isOn)
                {
                    equipments.Add(_chkboxEquipment[i].GetComponent<EquipmentData>().Id);
                }
            }
            newData.EquipmentUnlock = equipments;
        }
        private void ReadSkillUnlock(KillChordGameData newData)
        {
            List<int> skills = new List<int>();
            for (int i = 0; i < _chkboxSkill.Length; i++)
            {
                if (_chkboxSkill[i].isOn)
                {
                    skills.Add(_chkboxSkill[i].GetComponent<SkillData>().Id);
                }
            }
            newData.SkillUnlock = skills;
        }
        private void ActivateShelter()
        {
            _shelter.SetActive(true);
            _shelterText.text = "Saving...";
        }

        private void DeactivateShelter()
        {
            _shelter.SetActive(false);
        }
        #endregion

    }
}