using System.Threading.Tasks;
namespace KillChord.Runtime.Adaptor
{
    public interface IScenalioView
    {
        public ValueTask PlayTypewriterAsync(string text);
        public ValueTask PLayFadeInAsync(float duration);
        public ValueTask PlayFadeOutAsync(float duration);
    }
}
