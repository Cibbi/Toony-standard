using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.IO;

namespace Cibbi.ToonyStandard
{

    public abstract class Section
    {
        protected bool isOpen;
        protected bool isEnabled;
        private GUIContent sectionTitle;
        private Color sectionBgColor;
        private SectionStyle sectionStyle;

        /// <summary>
        /// Default section constructor
        /// </summary>
        /// <param name="sectionTitle">Title of the section</param>
        /// <param name="open">If is open or closed upon creation</param>
        /// <param name="content">Delegate function for drawing the section content</param>
        /// <param name="changesCheck">Delegate fucntion for checks that need to be done knowing if the box is open or enabled at all</param>
        public Section(GUIContent sectionTitle, bool open)
        {
            this.sectionTitle = sectionTitle;
            this.isOpen = open;

            TSSettings settings=JsonUtility.FromJson<TSSettings>(File.ReadAllText(TSConstants.SettingsJSONPath));
			sectionStyle=(SectionStyle)settings.sectionStyle;
			this.sectionBgColor=settings.sectionColor;
        }

        protected void BeginSection()
        {
            isEnabled=true;
            Color bCol = GUI.backgroundColor;
            GUI.backgroundColor = sectionBgColor;
            switch(sectionStyle)
            {
                case SectionStyle.Bubbles:
                    drawBubblesSection(bCol);
                    break;
                case SectionStyle.Foldout:
                    drawFoldoutSection(bCol);
                    break;
                case SectionStyle.Box:
                    drawBoxSection(bCol);
                    break;
            }
        }

        protected void EndSection()
        {
            if(sectionStyle==SectionStyle.Bubbles)
            {
                EditorGUILayout.EndVertical();
            }
        }

        /// <summary>
        /// Draws the section
        /// </summary>
        /// <param name="materialEditor">Material editor provided by the custom inspector window</param>
        public void DrawSection(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            isEnabled=true;
            EditorGUI.BeginChangeCheck();
            Color bCol = GUI.backgroundColor;
            GUI.backgroundColor = sectionBgColor;
            switch(sectionStyle)
            {
                case SectionStyle.Bubbles:
                    drawBubblesSection(bCol);
                    break;
                case SectionStyle.Foldout:
                    drawFoldoutSection(bCol);
                    break;
                case SectionStyle.Box:
                    drawBoxSection(bCol);
                    break;
            }
            if (isOpen)
            {
               SectionContent(materialEditor, properties);
            }
            if(sectionStyle==SectionStyle.Bubbles)
            {
                EditorGUILayout.EndVertical();
            }
            //
            //EditorGUILayout.Space();

            if (EditorGUI.EndChangeCheck())
            {
                EndBoxCheck(this.isOpen,this.isEnabled);
            }
        }

        public abstract void SectionContent(MaterialEditor materialEditor, MaterialProperty[] properties);

        public abstract void EndBoxCheck(bool isOpen, bool isEnabled);

        /// <summary>
        /// Draws the section header with the bubble style
        /// </summary>
        /// <param name="bCol">original background color</param>
        private void drawBubblesSection(Color bCol)
        {
            EditorGUILayout.BeginVertical("Button");
            GUI.backgroundColor = bCol;
            Rect r = EditorGUILayout.BeginHorizontal();
            isOpen=EditorGUILayout.Toggle(isOpen, EditorStyles.foldout, GUILayout.MaxWidth(15.0f));
            EditorGUILayout.LabelField(sectionTitle, TSConstants.Styles.sectionTitleCenter);      
            isEnabled=EditorGUILayout.Toggle(isEnabled, TSConstants.Styles.deleteStyle, GUILayout.MaxWidth(15.0f));
            isOpen = GUI.Toggle(r, isOpen, GUIContent.none, new GUIStyle());  
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws the section header with the foldout style
        /// </summary>
        /// <param name="bCol">original background color</param>
        private void drawFoldoutSection(Color bCol)
        {
            TSFunctions.DrawLine(new Color(0.35f,0.35f,0.35f,1),1,0);
            GUI.backgroundColor = bCol;
            
            Rect r = EditorGUILayout.BeginHorizontal();
            isOpen=EditorGUILayout.Toggle(isOpen, EditorStyles.foldout, GUILayout.MaxWidth(15.0f));
            EditorGUILayout.LabelField(sectionTitle, TSConstants.Styles.sectionTitle);      
            isEnabled=EditorGUILayout.Toggle(isEnabled, TSConstants.Styles.deleteStyle, GUILayout.MaxWidth(15.0f));
            isOpen = GUI.Toggle(r, isOpen, GUIContent.none, new GUIStyle());  
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws the section header with the box style
        /// </summary>
        /// <param name="bCol">original background color</param>
        private void drawBoxSection(Color bCol)
        {
            EditorGUILayout.BeginVertical("box");
            GUI.backgroundColor = bCol;
            
            Rect r = EditorGUILayout.BeginHorizontal();
            isOpen=EditorGUILayout.Toggle(isOpen, EditorStyles.foldout, GUILayout.MaxWidth(15.0f));
            EditorGUILayout.LabelField(sectionTitle, TSConstants.Styles.sectionTitle);      
            isEnabled=EditorGUILayout.Toggle(isEnabled, TSConstants.Styles.deleteStyle, GUILayout.MaxWidth(15.0f));
            isOpen = GUI.Toggle(r, isOpen, GUIContent.none, new GUIStyle());  
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Gets the section title GUIContent
        /// </summary>
        /// <returns>The section title GUIContent</returns>
        public GUIContent getSectionTitle()
        {
            return sectionTitle;
        }

        protected static MaterialProperty FindProperty(string propertyName, MaterialProperty[] properties)
        {
            return FindProperty(propertyName, properties, true);
        }

        protected static MaterialProperty FindProperty(string propertyName, MaterialProperty[] properties, bool propertyIsMandatory)
        {
            for (var i = 0; i < properties.Length; i++)
                if (properties[i] != null && properties[i].name == propertyName)
                    return properties[i];

            // We assume all required properties can be found, otherwise something is broken
            if (propertyIsMandatory)
                throw new ArgumentException("Could not find MaterialProperty: '" + propertyName + "', Num properties: " + properties.Length);
            return null;
        }

    }
}