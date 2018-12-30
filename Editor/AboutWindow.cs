#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;

public class ToonyStandardAboutWindow : EditorWindow {

	private bool firstCycle=true;

    Texture2D icon;

    static void Init()
    {
        UnityEditor.EditorWindow window = GetWindow(typeof(ToonyStandardAboutWindow));
        window.position = new Rect(0, 0, 250, 80);
        window.Show();
    }


    public static void ShowWindow() 
    {
        EditorWindow.GetWindow(typeof(ToonyStandardAboutWindow));
    }

    void OnGUI()
    {
		if(firstCycle)
		{
			Start();
		}
		GUILayout.Label(icon,GUILayout.Width(icon.width),GUILayout.Height(icon.height));
		GUILayout.Space(25);
		GUILayout.Label("Toony standard shader by Cibbi (Cibbi#9450)");
		GUILayout.Label("Logo by LambdaDelta (LambdaDelta#9848)");
		
    }

	public void Start()
	{
		string[] icons = AssetDatabase.FindAssets("ToonyStandardLogo t:Texture2D", null);
		if (icons.Length>0) 
        {
            string [] pieces=AssetDatabase.GUIDToAssetPath(icons[0]).Split('/');
            ArrayUtility.RemoveAt(ref pieces,pieces.Length-1);
            string path=string.Join("/",pieces);
			icon=AssetDatabase.LoadAssetAtPath<Texture2D>(path+"/ToonyStandardLogo.png");  
		}
		firstCycle=false;
	}

}

#endif