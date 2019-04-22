using UnityEditor;
using UnityEngine;
using System;
using System.Collections;

namespace Cibbi.ToonyStandard
{
    public delegate void SectionContent(MaterialEditor materialEditor);
    public delegate void ChangesCheck(bool isOpen, bool isEnabled);

    public class Section
    {
        private bool isOpen;
        private bool isEnabled;
        private GUIContent sectionTitle;
        private Color sectionBgColor;
        private GUIStyle sectionStyle;
        private SectionContent content;
        private ChangesCheck changesCheck;

        public Section(GUIContent sectionTitle, Color sectionBgColor, GUIStyle sectionStyle, bool open, SectionContent content, ChangesCheck changesCheck)
        {
            this.sectionTitle = sectionTitle;
            this.sectionBgColor = sectionBgColor;
            this.sectionStyle = sectionStyle;
            this.isOpen = open;
            this.content = content;
            this.changesCheck = changesCheck;
        }

        public void DrawSection(MaterialEditor materialEditor)
        {
            isEnabled=true;
            EditorGUI.BeginChangeCheck();
            Color bCol = GUI.backgroundColor;
            GUI.backgroundColor = sectionBgColor;
            EditorGUILayout.BeginVertical("Button");
            GUI.backgroundColor = bCol; 
            Rect r = EditorGUILayout.BeginHorizontal();
            isEnabled=EditorGUILayout.Toggle(isEnabled, EditorStyles.radioButton, GUILayout.MaxWidth(15.0f));
            EditorGUILayout.LabelField(sectionTitle, sectionStyle);
            isOpen = GUI.Toggle(r, isOpen, GUIContent.none, new GUIStyle());
            EditorGUILayout.Toggle(isOpen, EditorStyles.foldout, GUILayout.MaxWidth(15.0f));
            EditorGUILayout.EndHorizontal();
            if (isOpen)
            {
                content(materialEditor);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            if (EditorGUI.EndChangeCheck())
            {
                changesCheck(isOpen, isEnabled);
            }
        }

        public GUIContent getSectionTitle()
        {
            return sectionTitle;
        }

    }
}