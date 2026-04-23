using UnityEditor;
using UnityEngine;

namespace DevelopProducts.TicketSystem
{
    [FilePath("ProjectSettings/TicketWindowSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public class TicketSystemSettings : ScriptableSingleton<TicketSystemSettings>
    {
        public string gasUrl;
        public string apyKey;
        
        public void Save()
        {
            Save(false);
        }
    
    }
}
