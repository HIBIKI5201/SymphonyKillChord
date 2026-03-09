using UnityEditor;
using UnityEngine;

namespace DevelopProducts.ToonShader.Editor
{
    public class OutLineInjectorWindow : EditorWindow
    {
        public static void Open()
        {
            OutLineInjectorWindow window = GetWindow<OutLineInjectorWindow>();
            window.titleContent = new GUIContent("Outline Injector");
            window.Show();
        }

        private void OnGUI()
        {
            
        }
    }
}