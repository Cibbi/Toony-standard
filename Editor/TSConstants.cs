// This file is a collection of constant values used around

using UnityEngine;
using UnityEditor;

namespace Cibbi.ToonyStandard
{
    public static class TSConstants
    {
        public static string Version = "Toony Standard Master Build 20190508";
        public static string localJSONPath = Application.dataPath+"/Cibbi's shaders/Toony standard/version.json";
        public static string settingsJSONPath = Application.dataPath+"/Cibbi's shaders/Toony standard/Editor/settings.json";
        public static string oldSettingsJSONPath = Application.dataPath+"/Cibbi's shaders/Toony standard/Editor/oldSettings.json";
        
        public static Texture2D deleteIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Cibbi's shaders/Toony standard/Editor/Resources/DeleteIcon.png");
        public static Texture2D logo = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Cibbi's shaders/Toony standard/Editor/Resources/ToonyStandardLogo.png");
        public static Texture2D githubIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Cibbi's shaders/Toony standard/Editor/Resources/GitHubIcon.png");
        public static Texture2D patreonIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Cibbi's shaders/Toony standard/Editor/Resources/PatreonIcon.png");
        public static Texture2D defaultRamp = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Cibbi's shaders/Toony standard/Editor/Resources/RampDefault.png");


        public static class Styles
        {
             private static GUIStyle deleteStyleLocal;
            public static GUIStyle deleteStyle
            {
                get
                {
                    if(deleteStyleLocal==null)
                    {
                        deleteStyleLocal=new GUIStyle();
                        deleteStyleLocal.normal.background=TSConstants.deleteIcon;
                    }
                    return deleteStyleLocal;
                }
            }

            private static GUIStyle sectionTitleCenterLocal;
            public static GUIStyle sectionTitleCenter
            {
                get
                {
                    if(sectionTitleCenterLocal==null)
                    {
                        sectionTitleCenterLocal=new GUIStyle(EditorStyles.boldLabel);
                        sectionTitleCenterLocal.alignment = TextAnchor.MiddleCenter;
                    }
                    return sectionTitleCenterLocal;
                }
            }

            private static GUIStyle sectionTitleLocal;
            public static GUIStyle sectionTitle
            { 
                get
                {
                    if(sectionTitleLocal==null)
                    {
                        sectionTitleLocal=new GUIStyle(EditorStyles.boldLabel);
                        sectionTitleLocal.alignment = TextAnchor.MiddleLeft;
                    }
                    return sectionTitleLocal;
                }
            }

            private static GUIStyle centerLabelLocal;
            public static GUIStyle centerLabel
            {
                get
                {
                    if(centerLabelLocal==null)
                    {
                        centerLabelLocal=new GUIStyle(EditorStyles.label);
                        centerLabelLocal.alignment = TextAnchor.MiddleCenter;
                    }
                    return centerLabelLocal;
                }
            }

            private static GUIStyle multilineLabelCenterLocal;
            public static GUIStyle multilineLabelCenter
            {
                get
                {
                    if(multilineLabelCenterLocal==null)
                    {
                        multilineLabelCenterLocal=new GUIStyle(EditorStyles.label);
                        multilineLabelCenterLocal.alignment = TextAnchor.MiddleCenter;
                        multilineLabelCenterLocal.wordWrap=true;
                    }
                    return multilineLabelCenterLocal;
                }
            }

            private static GUIStyle multilineLabelLocal;
            public static GUIStyle multilineLabel
            {
                get
                {
                    if(multilineLabelLocal==null)
                    {
                        multilineLabelLocal=new GUIStyle(EditorStyles.label);
                        multilineLabelLocal.wordWrap=true;
                    }
                    return multilineLabelLocal;
                }
            }
        }

       
    }
}