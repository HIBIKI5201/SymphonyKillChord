using KillChord.Editor.Utility;
using UnityEditor;

namespace KillChord.Editor.TicketSystem
{
    [FilePath(ProviderConst.USER_SETTINGS_PATH + nameof(TicketSystemSettings) + ProviderConst.ASSET_EXT,
        FilePathAttribute.Location.ProjectFolder)]
    public class TicketSystemSettings : ScriptableSingleton<TicketSystemSettings>
    {
        public string GasUrl;
        public string ApiKey;
        public string UserName;

        public void Save()
        {
            Save(false);
        }
    }
}