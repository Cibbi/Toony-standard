// Various constant functions used around

using UnityEditor;
using UnityEngine;
using System;
using System.Collections;

namespace Cibbi.ToonyStandard
{
    public static class TSFunctions
    {
        /// <summary>
        /// Sets a keyword state
        /// </summary>
        /// <param name="m">Material</param>
        /// <param name="keyword">The keyword that is being toggled</param>
        /// <param name="state">Toggle value</param>
        public static void SetKeyword(Material m, string keyword, bool state)
        {
            if (state)
                m.EnableKeyword(keyword);
            else
                m.DisableKeyword(keyword);
        }

        /// <summary>
        /// Converts a boolean to a float value
        /// </summary>
        /// <param name="boolean">Boolean value</param>
        /// <returns>Returns 1 if true, 0 if false</returns>
        public static float floatBoolean(bool boolean)
        {
            if (boolean)
                return 1;
            else
                return 0;
        }
        /// <summary>
        /// Converts a float to a boolean value
        /// </summary>
        /// <param name="floatBool">Float value</param>
        /// <returns>Returns false if the float is 0, true in any other case</returns>
        public static bool BooleanFloat(float floatBool)
        {
            if (floatBool != 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Draws a line, method gently provided by this random dude here https://forum.unity.com/threads/horizontal-line-in-editor-window.520812/#post-3534861
        /// Is slightly modified in order to cover the entirety of the window width
        /// </summary>
        /// <param name="color">Color of the line</param>
        /// <param name="thickness">Thickness of the line</param>
        /// <param name="padding">Verical padding</param>
        public static void DrawLine(Color color, int thickness = 2, int padding = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding+thickness));
            float lineCenter=r.x+(r.width/2);
            r.x=0;
            r.width=lineCenter*2;
            r.height = thickness;
            r.y+=padding/2;
            r.x-=2;
            r.width +=6;
            EditorGUI.DrawRect(r, color);
        }

        /// <summary>
        /// Draws a selector driven by a material property
        /// </summary>
        /// <param name="options">String array of the available options</param>
        /// <param name="selectedOption">Material property containing the selected value</param>
        /// <param name="label">Label of the selector</param>
        /// <param name="materialEditor">Material editor of the current window</param>
        public static void DrawSelector(string[] options, MaterialProperty selectedOption, GUIContent label, MaterialEditor materialEditor)
        {
            GUIContent[] GUIoptions = new GUIContent[options.Length];
            int i = 0;
            foreach (string option in options)
            {
                GUIoptions[i] = new GUIContent(option, option);
                i++;
            }
            int bMode = (int)selectedOption.floatValue;
            EditorGUI.BeginChangeCheck();
            bMode = EditorGUILayout.Popup(label, (int)bMode, GUIoptions);
            if (EditorGUI.EndChangeCheck())
            {
                materialEditor.RegisterPropertyChangeUndo(label.text);
                selectedOption.floatValue = (float)bMode;
            }
        }

        /// <summary>
        /// Draws a toggle property
        /// </summary>
        /// <param name="boolProperty">boolean property</param>
        /// <param name="label">Label of the property</param>
        public static bool ProperToggle(ref MaterialProperty boolProperty, GUIContent label)
        {
            bool isToggleEnabled = boolProperty.floatValue>0;
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = boolProperty.hasMixedValue;
            isToggleEnabled = EditorGUILayout.Toggle(label, isToggleEnabled);
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
            {
                boolProperty.floatValue = floatBoolean(isToggleEnabled);
            }
            return isToggleEnabled;
            
        }

        /// <summary>
        /// Draws a color property with a color box that doesn't look retarded
        /// </summary>
        /// <param name="colorProperty">Material property that contains the color property that needs to be drawn, has to be passed by reference</param>
        /// <param name="label">Label of the property</param>
        public static void ProperColorBox(ref MaterialProperty colorProperty, GUIContent label)
        {
            Color boxColor = colorProperty.colorValue;
            EditorGUI.BeginChangeCheck();
            bool hdr = false;
            if (colorProperty.flags == MaterialProperty.PropFlags.HDR)
            {
                hdr = true;
            }
            Rect colorPropertyRect = EditorGUILayout.GetControlRect();
            colorPropertyRect.width = EditorGUIUtility.labelWidth + 50.0f;
            EditorGUI.showMixedValue = colorProperty.hasMixedValue;
            #if UNITY_2018_1_OR_NEWER
                boxColor = EditorGUI.ColorField(colorPropertyRect, label, boxColor, true, true, hdr);
            #else
                boxColor = EditorGUI.ColorField(colorPropertyRect, label, boxColor, true, true, hdr, new ColorPickerHDRConfig(0, 65536, 0, 3));
            #endif
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
            {
                colorProperty.colorValue = boxColor;
            }
        }

        /// <summary>
        /// Draws the default header of the various windows
        /// </summary>
        /// <param name="windowWidth">the width of the current window</param>
        /// <param name="padding">Vertical padding</param>
        public static void DrawHeader(float windowWidth, int padding)
		{
			Texture2D icon=TSConstants.Logo;
			GUILayout.Space(padding);
			GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
                windowWidth-=10;
                int width;
                int height;
                if(windowWidth<icon.width)
                {
                    width=(int)windowWidth;
                    height=icon.height*width/icon.width;
                }
                else
                {
                    width=icon.width;
                    height=icon.height;
                }
				GUILayout.Label(icon, GUILayout.Width(width),GUILayout.Height(height));
				GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.Space(padding);
		}

        /// <summary>
        /// Draw the default footer of the various windows
        /// </summary>
        public static void DrawFooter()
		{
			GUILayout.FlexibleSpace();
			GUILayout.BeginHorizontal();
					GUILayout.BeginHorizontal();
						if (GUILayout.Button(new GUIContent(TSConstants.GithubIcon, "Check the official GitHub!"), "label", GUILayout.Width(32), GUILayout.Height(43)))
						{
							Application.OpenURL("https://github.com/Cibbi/Toony-standard");
						}

						EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
						if (GUILayout.Button(new GUIContent(TSConstants.PatreonIcon, "Want to gift me pizza every month? Become a patreon!"), "label", GUILayout.Width(32), GUILayout.Height(32)))
						{
							Application.OpenURL("https://www.patreon.com/Cibbi");
						}
						EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
					GUILayout.EndHorizontal();
					GUILayout.FlexibleSpace();
					GUIStyle aboutLabelStyle = new GUIStyle(EditorStyles.miniLabel);
					aboutLabelStyle.alignment = TextAnchor.LowerRight;
					aboutLabelStyle.fontStyle = FontStyle.Italic;
					aboutLabelStyle.hover.textColor = Color.magenta;
					GUILayout.Label(TSConstants.Version, aboutLabelStyle, GUILayout.Height(32));

				GUILayout.EndHorizontal();
		}
    }


}