using UnityEngine;
using UnityEditor;

namespace Cibbi.ToonyStandard
{
    public class TSGradientEditor : EditorWindow
    {

        [MenuItem("Tools/Cibbi/TS Gradient Editor")]
        private static TSGradientEditor CreateBaseWindow()
        {
            TSGradientEditor window = EditorWindow.GetWindow<TSGradientEditor>();
            window.titleContent = new GUIContent("TS Gradient Editor");
            window.minSize = new Vector2(400, 190);
            window.maxSize = new Vector2(400, 190);
            return window;
        }

        GradientEditor gradientEditor;
        bool firstCycle = true;

        private void Start()
        {
            gradientEditor = new GradientEditor();
        }

        void OnGUI()
        {
            // If this is the first iteration since the window is opened, do the needed initializzations
            if (firstCycle)
            {
                Start();
                firstCycle = false;
            }

            if (gradientEditor.DrawGUI())
            {
                Repaint();
            }

            if (GUILayout.Button("Save Gradient"))
            {
                string path = TSFunctions.GetSelectedPathOrFallback();
                path = EditorUtility.SaveFilePanel("Save Ramp", path, "gradient", "png");
                if (path.Length != 0)
                {
                    gradientEditor.SaveGradient(path);
                }
            }
        }
    }
}