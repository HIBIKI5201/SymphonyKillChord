using UnityEngine;
using TMPro;
namespace KillChord.Runtime.View
{
    public class ScenarioView : MonoBehaviour
    {
        public void Initilize(ViewModel viewModel)
        {
            viewModel.ChangeChat += InputViewModel;
        }

        [SerializeField]
        private TMP_Text _chat;

        private void InputViewModel(string chat)
        {
            _chat.text = chat;
        }
    }
}
