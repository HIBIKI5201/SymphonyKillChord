using System.Threading.Tasks;
using KillChord.Runtime.Adaptor;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace KillChord.Runtime.View
{
    public class ScenalioView : MonoBehaviour, IScenalioView
    {
        public async ValueTask PLayFadeInAsync(float duration)
        {
            await FadeBackGround(0f, 1f, duration);
        }

        public async ValueTask PlayFadeOutAsync(float duration)
        {
            await FadeBackGround(1f, 0f, duration);
        }

        public async ValueTask PlayTypewriterAsync(string text)
        {
            if (_text == null) return;

            _text.text = "";
            foreach (char c in text)
            {
                _text.text += c;
                await Task.Delay(30);
            }
        }

        [SerializeField] private TMP_Text _text;
        [SerializeField] private Image _backGround;

        private async ValueTask FadeBackGround(float startAlpha, float endAlpha, float duration)
        {
            if (_backGround == null) return;
            float elapsed = 0f;
            Color color = _backGround.color;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                color.a = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
                _backGround.color = color;
                await Task.Yield();
            }
            Color result = _backGround.color;
            result.a = 0f;
            _backGround.color = result;
        }
    }
}
