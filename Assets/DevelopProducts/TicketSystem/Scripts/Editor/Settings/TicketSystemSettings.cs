using UnityEditor;

namespace DevelopProducts.TicketSystem
{
    [FilePath("UserSettings/TicketWindowSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public class TicketSystemSettings : ScriptableSingleton<TicketSystemSettings>
    {
        public string gasUrl;
        public string apiKey;
        public string userName;

        public void Save()
        {
            Save(false);
        }
    }
}