using System;
using KillChord.Runtime.View;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Setting
{
    /// <summary>
    ///     オーディオ設定画面の表示構成を保持するConfigです。
    /// </summary>
    [CreateAssetMenu(menuName = "KillChord/Settings/Audio")]
    public class AudioConfig : ScriptableObject
    {
        /// <summary>
        ///     オーディオ設定用スライダーを生成します。
        /// </summary>
        /// <param name="document"> 設定画面のUI Documentです。 </param>
        /// <param name="model"> オーディオ設定値です。 </param>
        /// <param name="parent"> スライダーの生成先です。 </param>
        /// <returns> 生成に成功した場合はtrue。 </returns>
        public bool Build(UIDocument document, AudioSettingData model, Transform parent)
        {
            if (_sliderPrefab == null || document == null || model == null || parent == null)
            {
                Debug.LogError($"[{nameof(AudioConfig)}] オーディオ設定画面の生成に必要な参照が不足しています。", this);
                return false;
            }

            float[] settings = model.Settings;
            int titleCount = _audioSettingTitle?.Length ?? 0;
            int settingCount = settings?.Length ?? 0;
            if (titleCount != settingCount)
            {
                Debug.LogError(
                    $"[{nameof(AudioConfig)}] 設定タイトル数と設定値数が一致しません。TitleCount={titleCount}, SettingCount={settingCount}",
                    this);
                return false;
            }

            for (int i = 0; i < settingCount; i++)
            {
                int index = i;
                CreateSlider(document, parent,
                    _audioSettingTitle[index],
                    () => model.Get(index),
                    value => model.Set(index, value));
            }

            return true;
        }

        [SerializeField, Tooltip("オーディオ設定項目に使用するスライダーPrefabです。")]
        private SettingSlider _sliderPrefab;

        [SerializeField, Tooltip("オーディオ設定値と同じ順序で表示する項目名です。")]
        private string[] _audioSettingTitle;

        /// <summary>
        ///     オーディオ設定用スライダーを一件生成します。
        /// </summary>
        /// <param name="document"> 設定画面のUI Documentです。 </param>
        /// <param name="parent"> スライダーの生成先です。 </param>
        /// <param name="title"> 表示する設定名です。 </param>
        /// <param name="getter"> 現在値の取得処理です。 </param>
        /// <param name="setter"> 設定値の更新処理です。 </param>
        private void CreateSlider(
            UIDocument document,
            Transform parent,
            string title,
            Func<float> getter,
            Action<float> setter)
        {
            SettingSlider slider = Instantiate(_sliderPrefab, parent);

            slider.Create(document, Category.Audio, title);
            slider.Bind(getter, setter);
        }
    }
}

