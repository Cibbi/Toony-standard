using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.IO;

namespace Cibbi.ToonyStandard
{
    /// <summary>
    /// Parameters needed by an ordered section packed into a single structure
    /// </summary>
    public struct BoxParameters
    {
        public MaterialProperty box;
        public MaterialProperty index;

        public BoxParameters(MaterialProperty box, MaterialProperty index)
        {
            this.box=box;
            this.index=index;
        }
    }
    public abstract class OrderedSection
    {
        protected bool isOpen;
        protected bool isEnabled;
        private GUIContent sectionTitle;
        private Color sectionBgColor;
        private SectionStyle sectionStyle;
        public int pushState;

        private bool isUp;
        private bool isDown;

        /// <summary>
        /// Main constructor
        /// </summary>
        /// <param name="sectionTitle">Title of the section</param>
        /// <param name="open">Is the section expanded?</param>
        /// <param name="enabled">IS the section enabled?</param>
        /// <returns></returns>
        public OrderedSection(GUIContent sectionTitle, bool open, bool enabled) 
        {
            this.sectionTitle = sectionTitle;
            this.isOpen = open;
            this.isEnabled = enabled;
            pushState=0;

            isUp=false;
            isDown=false;

            TSSettings settings=JsonUtility.FromJson<TSSettings>(File.ReadAllText(TSConstants.SettingsJSONPath));
			sectionStyle=(SectionStyle)settings.sectionStyle;
			this.sectionBgColor=settings.sectionColor;
        }

        /// <summary>
        /// Draws the section
        /// </summary>
        /// <param name="materialEditor">Material editor provided by the custom inspector window</param>
        /// <param name="properties">Material properties provided by the custom inspector window</param>
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
                try
                {
                    SectionContent(materialEditor, properties);
                }
                catch(ArgumentException)
                {
                    //do nothing since the argumentException thrown is not going to cause problems
                    //since the only time it gets thrown is when an undo operation is done
                    //and that is only a visualization error of the editor on a single cycle
                }
               
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

        /// <summary>
        /// Displays the content of the section, must be ovewritten by a child class
        /// </summary>
        /// <param name="materialEditor">Material editor provided by the custom inspector window</param>
        /// <param name="properties">Material properties provided by the custom inspector window</param>
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
            GUILayout.Space(38.0f);
            EditorGUILayout.LabelField(sectionTitle, TSConstants.Styles.sectionTitleCenter); 
            DrawUpDownButtons();
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
            DrawUpDownButtons();    
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
            DrawUpDownButtons();     
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

        public void DrawUpDownButtons()
        {  
            isUp = EditorGUILayout.Toggle(isUp, TSConstants.Styles.upStyle, GUILayout.Width(15.0f),GUILayout.Height(15.0f));
            isDown = EditorGUILayout.Toggle(isDown,TSConstants.Styles.downStyle, GUILayout.Width(15.0f));
            if(isUp)
            {
                pushState=-1;
                isUp=false;
            }
            else if(isDown)
            {
                pushState=1;
                isDown=false;
            }


        }


        public virtual void OnAdd()
        {

        }
        
        public virtual bool CanBeEnabled(MaterialProperty[] properties)
        {
            return true;
        }

        protected abstract MaterialProperty GetIndex();

        protected abstract MaterialProperty GetBox();

        /// <summary>
        /// Returns all materials targeted by the index property
        /// </summary>
        /// <returns>An array containing all materials targeted by the index property</returns>
        public System.Object[] GetIndexTargets()
        {
            return GetIndex().targets;
        }

        /// <summary>
        /// Returns the index property name
        /// </summary>
        /// <returns>The index property name</returns>
        public string GetIndexName()
        {
            return GetIndex().name;
        }

        /// <summary>
        /// Return the index float value
        /// </summary>
        /// <returns>The index float value</returns>
        public int GetIndexNumber()
        {
            return (int)GetIndex().floatValue;
        }

        /// <summary>
        /// Sets the index value
        /// </summary>
        /// <param name="index">Value to set</param>
        public void SetIndexNumber(int index)
        {
           GetIndex().floatValue = index;
        }
        /// <summary>
        /// Checks if the index value has mixed values
        /// </summary>
        /// <returns>True if it has mixed value, false otherwise</returns>
        public bool IsIndexMixed()
        {
            return GetIndex().hasMixedValue;
        }

        /// <summary>
        /// Checks if the box value has mixed values
        /// </summary>
        /// <returns>True if it has mixed value, false otherwise</returns>
        public bool IsBoxMixed()
        {
            return GetBox().hasMixedValue;
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