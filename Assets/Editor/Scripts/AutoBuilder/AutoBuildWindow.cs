using CodiceApp;
using DevelopProducts.TicketSystem;
using KillChord.Editor.Utility;
using UnityEditor;
using UnityEngine;

namespace KillChord.Editor.AutoBuilder
{
    public class AutoBuildWindow : EditorWindow
    {
        [MenuItem(ToolConst.WINDOW_PATH + "AutoBuilder")]
        public static void ShowWindow()
        {
            GetWindow<MasterTicketWindow>("AutoBuilder");
        }

        private void OnEnable()
        {
        }

        private void OnGUI()
        {

        }
    }
}