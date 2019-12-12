using UnityEngine;
using UnityEditor;

namespace Cibbi.ToonyStandard
{
    public class TSTexturePacker : EditorWindow
    {

        [MenuItem("Tools/Cibbi/TS Texture packer")]
        private static TSTexturePacker CreateBaseWindow()
        {
            TSTexturePacker window = EditorWindow.GetWindow<TSTexturePacker>();
            window.titleContent = new GUIContent("TS Texture Packer");
            window.minSize = new Vector2(400, 230);
            window.maxSize = new Vector2(400, 230);
            return window;
        }

        TexturePacker packer;
        bool firstCycle = true;

        private void Start()
        {
            packer = new TexturePacker(TexturePacker.Resolution.M_512x512, new string[] { "Texture 1", "Texture 2", "Texture 3", "Texture 4" }, TSFunctions.GetSelectedPathOrFallback() + "Packed.png");
            packer.drawInternalConfirmButton = false;
        }

        void OnGUI()
        {
            // If this is the first iteration since the window is opened, do the needed initializzations
            if (firstCycle)
            {
                Start();
                firstCycle = false;
            }

            packer.DrawGUI();

            if (GUILayout.Button("Save Packed Texture"))
            {
                string path = TSFunctions.GetSelectedPathOrFallback();
                path = EditorUtility.SaveFilePanel("Save Texture", path, "Packed", "png");
                if (path.Length != 0)
                {
                    packer.PackTexture(path);
                }
            }
        }
    }
}