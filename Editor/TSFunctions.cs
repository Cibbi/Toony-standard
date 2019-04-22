using UnityEditor;
using UnityEngine;
using System;
using System.Collections;

namespace Cibbi.ToonyStandard
{
    public static class TSFunctions
    {
        public static void SetKeyword(Material m, string keyword, bool state)
        {
            if (state)
                m.EnableKeyword(keyword);
            else
                m.DisableKeyword(keyword);
        }

        public static float floatBoolean(bool boolean)
        {
            if (boolean)
                return 1;
            else
                return 0;
        }

        public static bool BooleanFloat(float floatBool)
        {
            if (floatBool != 0)
                return true;
            else
                return false;
        }
        
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

        public static void ProperColorBox(ref MaterialProperty colorProperty, GUIContent text)
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
            boxColor = EditorGUI.ColorField(colorPropertyRect, text, boxColor, true, true, hdr, new ColorPickerHDRConfig(0, 65536, 0, 3));
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
            {
                colorProperty.colorValue = boxColor;
            }
        }
    }


}