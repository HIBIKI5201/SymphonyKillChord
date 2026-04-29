using System.Collections.Generic;
using UnityEditor;

namespace KillChord.Editor.AutoBuilder
{
    public class AutoBuilderProvider : SettingsProvider
    {
        public AutoBuilderProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }


    }
}