using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace Research.SaveSystem
{
    /// <summary>
    ///     動作確認用画面のViewクラス
    /// </summary>
    public class TestHomeView : MonoBehaviour
    {
        /// <summary>
        ///     初期化メソッド。
        /// </summary>
        /// <param name="saveSystemdata"></param>
        /// <param name="savePlayerData"></param>
        /// <param name="saveOutGameData"></param>
        /// <param name="loadSystemDataController"></param>
        /// <param name="loadPlayerDataController"></param>
        /// <param name="loadOutGameDataController"></param>
        public void Init(SaveSystemDataController saveSystemdata, SavePlayerDataController savePlayerData, SaveOutGameDataController saveOutGameData,
            LoadSystemDataController loadSystemDataController, LoadPlayerDataController loadPlayerDataController, LoadOutGameDataController loadOutGameDataController)
        {
            _saveSystemdata = saveSystemdata;
            _savePlayerData = savePlayerData;
            _saveOutGameData = saveOutGameData;

            _loadSystemData = loadSystemDataController;
            _loadPlayerData = loadPlayerDataController;
            _loadOutGameData = loadOutGameDataController;

            EventBus<EOnSaveStart>.Register(OnSaveStart);
            EventBus<EOnSaveEnd>.Register(OnSaveEnd);
            EventBus<EOnSaveError>.Register(OnSaveError);
            EventBus<EOnLoadStart>.Register(OnLoadStart);
            EventBus<EOnLoadEnd>.Register(OnLoadEnd);
            EventBus<EOnLoadError>.Register(OnLoadError);
        }

        /// <summary>
        ///     システム設定のSaveボタン押下時の処理
        /// </summary>
        public void OnSaveSystemDataButtonClicked()
        {
            _saveSystemdata.Save(GetSystemDataDto());
        }
        /// <summary>
        ///     システム設定のLoadボタン押下時の処理
        /// </summary>
        public void OnLoadSystemDataButtonClicked()
        {
            _loadSystemData.Load(SetSystemData);
        }
        /// <summary>
        ///     プレイヤー情報のSaveボタン押下時の処理
        /// </summary>
        public void OnSavePlayerDataButtonClicked()
        {
            _savePlayerData.Save(GetPlayerDataDto());
        }
        /// <summary>
        ///     プレイヤー情報のLoadボタン押下時の処理
        /// </summary>
        public void OnLoadPlayerDataButtonClicked()
        {
            _loadPlayerData.Load(SetPlayerData);
        }

        /// <summary>
        ///     アウトゲーム情報のSaveボタン押下時の処理
        /// </summary>
        public void OnSaveOutGameDataButtonClicked()
        {
            _saveOutGameData.Save(GetOutGameDataDto());
        }

        /// <summary>
        ///     アウトゲーム情報のLoadボタン押下時の処理
        /// </summary>
        public void OnLoadOutGameDataButtonClicked()
        {
            _loadOutGameData.Load(SetOutGameData);
        }

        [SerializeField, Tooltip("UI遮蔽物")]
        private GameObject _shelter;
        [SerializeField, Tooltip("UI遮蔽物の文字")]
        private Text _shelterText;
        [SerializeField, Tooltip("エラーメッセージエリア")]
        private GameObject _errPanel;
        [SerializeField, Tooltip("エラーメッセージ文字")]
        private Text _errText;

        [SerializeField, Tooltip("")]
        private Slider _masterVolume;
        [SerializeField, Tooltip("")]
        private Slider _bgmVolume;
        [SerializeField, Tooltip("")]
        private Slider _seVolume;
        [SerializeField, Tooltip("")]
        private InputField _inputSkills;
        [SerializeField, Tooltip("")]
        private Toggle[] _chkboxStageUnlock;
        [SerializeField, Tooltip("")]
        private Toggle[] _chkboxSkillUnlock;

        private SaveSystemDataController _saveSystemdata;
        private SavePlayerDataController _savePlayerData;
        private SaveOutGameDataController _saveOutGameData;

        private LoadSystemDataController _loadSystemData;
        private LoadPlayerDataController _loadPlayerData;
        private LoadOutGameDataController _loadOutGameData;

        #region ライフサイクル
        private void OnDestroy()
        {
            EventBus<EOnSaveStart>.Unregister(OnSaveStart);
            EventBus<EOnSaveEnd>.Unregister(OnSaveEnd);
            EventBus<EOnSaveError>.Unregister(OnSaveError);
            EventBus<EOnLoadStart>.Unregister(OnLoadStart);
            EventBus<EOnLoadEnd>.Unregister(OnLoadEnd);
            EventBus<EOnLoadError>.Unregister(OnLoadError);
        }
        #endregion

        #region プライベートメソッド
        private SystemDataDto GetSystemDataDto()
        {
            SystemDataDto systemData = new();
            systemData.MasterVolume = _masterVolume.value;
            systemData.BgmVolume = _bgmVolume.value;
            systemData.SeVolume = _seVolume.value;
            return systemData;
        }
        private PlayerDataDto GetPlayerDataDto()
        {
            PlayerDataDto playerData = new();
            playerData.EquippedSkills = _inputSkills.text.Split(',').Select(int.Parse).ToList();
            return playerData;
        }
        private OutGameDataDto GetOutGameDataDto()
        {
            OutGameDataDto outGameData = new();
            outGameData.StageUnlock = GetStageUnlock();
            outGameData.SkillUnlock = GetSkillUnlock();
            return outGameData;
        }

        private HashSet<int> GetStageUnlock()
        {
            HashSet<int> progress = new();
            for (int i = 0; i < _chkboxStageUnlock.Length; i++)
            {
                if (_chkboxStageUnlock[i].isOn)
                {
                    progress.Add(_chkboxStageUnlock[i].GetComponent<StageData>().Id);
                }
            }
            return progress;
        }

        private HashSet<int> GetSkillUnlock()
        {
            HashSet<int> skills = new HashSet<int>();
            for (int i = 0; i < _chkboxSkillUnlock.Length; i++)
            {
                if (_chkboxSkillUnlock[i].isOn)
                {
                    skills.Add(_chkboxSkillUnlock[i].GetComponent<SkillData>().Id);
                }
            }
            return skills;
        }

        private void SetSystemData(SystemDataDto dto)
        {
            _masterVolume.value = dto.MasterVolume;
            _bgmVolume.value = dto.BgmVolume;
            _seVolume.value = dto.SeVolume;
        }

        private void SetPlayerData(PlayerDataDto dto)
        {
            _inputSkills.text = string.Join(',', dto.EquippedSkills);
        }

        private void SetOutGameData(OutGameDataDto dto)
        {
            HashSet<int> skills = dto.SkillUnlock;
            for (int i = 0; i < _chkboxSkillUnlock.Length; i++)
            {
                int id = _chkboxSkillUnlock[i].GetComponent<SkillData>().Id;
                if (skills.Contains(id))
                {
                    _chkboxSkillUnlock[i].isOn = true;
                }
                else
                {
                    _chkboxSkillUnlock[i].isOn = false;
                }
            }

            HashSet<int> stages = dto.StageUnlock;
            for (int i = 0; i < _chkboxStageUnlock.Length; i++)
            {
                int id = _chkboxStageUnlock[i].GetComponent<StageData>().Id;
                if (stages.Contains(id))
                {
                    _chkboxStageUnlock[i].isOn = true;
                }
                else
                {
                    _chkboxStageUnlock[i].isOn = false;
                }
            }
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

        private void OnSaveError(EOnSaveError eventParam)
        {
            _errPanel.SetActive(true);
            _errText.text = eventParam.ErrorMessage;
            Invoke(nameof(HideErrorMessage), 3);
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

        private void OnLoadError(EOnLoadError eventParam)
        {
            _errPanel.SetActive(true);
            _errText.text = eventParam.ErrorMessage;
            Invoke(nameof(HideErrorMessage), 3);
        }

        private void HideErrorMessage()
        {
            _errPanel.SetActive(false);
        }
        #endregion
    }
}