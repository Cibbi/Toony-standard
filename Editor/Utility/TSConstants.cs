// This file is a collection of constant values used around

using UnityEngine;
using UnityEditor;

namespace Cibbi.ToonyStandard
{
    public static class TSConstants
    {
        public static string Version = "Toony Standard master build 20191105";
        public static string TSEPInspectorLevel = "TSInspectorLevel";
        public static string TSEPNotFirstTime = "TSNotFirstTime";

        private static string localShaderFolder;
        private static string localShaderAssetsFolder;
        private static string localJSONPath;
        private static string settingsJSONPath;
        private static string oldSettingsJSONPath;

        private static Texture2D deleteIcon;
        private static Texture2D upIcon;
        private static Texture2D downIcon;
        private static Texture2D upColor;
        private static Texture2D upColorSelected;
        private static Texture2D upColorInternal;
        private static Texture2D logo;
        private static Texture2D githubIcon;
        private static Texture2D patreonIcon;
        private static Texture2D defaultRamp;
        private static ComputeShader packerCompute;

        public static string LocalShaderFolder
        {
            get
            {
                if(localShaderFolder==null)
                    InitializeFolderReferences();
                return localShaderFolder;
            }

            set
            {
                localShaderFolder = value;
            }
        }
        public static string LocalShaderAssetsFolder
        {
            get
            {
                if(localShaderAssetsFolder==null)
                    InitializeFolderReferences();
                return localShaderAssetsFolder;
            }

            set
            {
                localShaderAssetsFolder = value;
            }
        }
        public static string LocalJSONPath
        {
            get
            {
                if(localJSONPath==null)
                    InitializeFolderReferences();
                return localJSONPath;
            }

            set
            {
                localJSONPath = value;
            }
        }
        public static string SettingsJSONPath
        {
            get
            {
                if(settingsJSONPath==null)
                    InitializeFolderReferences();
                return settingsJSONPath;
            }

            set
            {
                settingsJSONPath = value;
            }
        }
        public static string OldSettingsJSONPath
        {
            get
            {
                if(oldSettingsJSONPath==null)
                    InitializeFolderReferences();
                return oldSettingsJSONPath;
            }

            set
            {
                oldSettingsJSONPath = value;
            }
        }


        public static Texture2D DeleteIcon
        {
            get
            {
                if(deleteIcon==null)
                    InitializeFolderReferences();
                return deleteIcon;
            }

            set
            {
                deleteIcon = value;
            }
        }
        public static Texture2D UpIcon
        {
            get
            {
                if(upIcon==null)
                    InitializeFolderReferences();
                return upIcon;
            }

            set
            {
                deleteIcon = value;
            }
        }
        public static Texture2D DownIcon
        {
            get
            {
                if(downIcon==null)
                    InitializeFolderReferences();
                return downIcon;
            }

            set
            {
                downIcon = value;
            }
        }
        public static Texture2D UpColor
        {
            get
            {
                if(upColor==null)
                    InitializeFolderReferences();
                return upColor;
            }

            set
            {
                upColor = value;
            }
        }
        public static Texture2D UpColorSelected
        {
            get
            {
                if(upColorSelected==null)
                    InitializeFolderReferences();
                return upColorSelected;
            }

            set
            {
                upColorSelected = value;
            }
        }
        public static Texture2D UpColorInternal
        {
            get
            {
                if(upColorInternal==null)
                    InitializeFolderReferences();
                return upColorInternal;
            }

            set
            {
                upColorInternal = value;
            }
        }
        public static Texture2D Logo
        {
            get
            {
                if(logo==null)
                    InitializeFolderReferences();
                return logo;
            }

            set
            {
                logo = value;
            }
        }
        public static Texture2D GithubIcon
        {
            get
            {
                if(githubIcon==null)
                    InitializeFolderReferences();
                return githubIcon;
            }

            set
            {
                githubIcon = value;
            }
        }
        public static Texture2D PatreonIcon
        {
            get
            {
                if(patreonIcon==null)
                    InitializeFolderReferences();
                return patreonIcon;
            }

            set
            {
                patreonIcon = value;
            }
        }
        public static Texture2D DefaultRamp
        {
            get
            {
                if(defaultRamp==null)
                    InitializeFolderReferences();
                return defaultRamp;
            }

            set
            {
                defaultRamp = value;
            }
        }
        public static ComputeShader PackerCompute
        {
            get
            {
                if(packerCompute==null)
                    InitializeFolderReferences();
                return packerCompute;
            }

            set
            {
                packerCompute = value;
            }
        }

        public static void InitializeFolderReferences()
        {
            string assetsPath="";
            string[] logopath = AssetDatabase.FindAssets("ToonyStandardLogo t:Texture2D", null);
            if (logopath.Length > 0)
            {
                string[] pieces = AssetDatabase.GUIDToAssetPath(logopath[0]).Split('/');
                ArrayUtility.RemoveAt(ref pieces, pieces.Length - 1);
                ArrayUtility.RemoveAt(ref pieces, pieces.Length - 1);
                ArrayUtility.RemoveAt(ref pieces, pieces.Length - 1);
                assetsPath = string.Join("/", pieces);
            }

            string path=Application.dataPath+assetsPath.Substring(assetsPath.IndexOf("/"));

            localShaderFolder=path;
            localShaderAssetsFolder = assetsPath;
            localJSONPath = path + "/version.json";
            settingsJSONPath = path + "/Editor/settings.json";
            oldSettingsJSONPath =path + "/Editor/oldSettings.json";
        
            deleteIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(assetsPath + "/Editor/Resources/DeleteIcon.png");
            upIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(assetsPath + "/Editor/Resources/UpIcon.png");
            downIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(assetsPath + "/Editor/Resources/DownIcon.png");
            upColor = AssetDatabase.LoadAssetAtPath<Texture2D>(assetsPath + "/Editor/Resources/upColor.png");
            upColorSelected = AssetDatabase.LoadAssetAtPath<Texture2D>(assetsPath + "/Editor/Resources/upColorSelected.png");
            upColorInternal = AssetDatabase.LoadAssetAtPath<Texture2D>(assetsPath + "/Editor/Resources/upColorInternal.png");
            logo = AssetDatabase.LoadAssetAtPath<Texture2D>(assetsPath + "/Editor/Resources/ToonyStandardLogo.png");
            githubIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(assetsPath + "/Editor/Resources/GitHubIcon.png");
            patreonIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(assetsPath + "/Editor/Resources/PatreonIcon.png");
            defaultRamp = AssetDatabase.LoadAssetAtPath<Texture2D>(assetsPath + "/Editor/Resources/RampDefault.png");
            packerCompute = AssetDatabase.LoadAssetAtPath<ComputeShader>(assetsPath + "/Editor/Utility/PackChannels.compute");
        }

        public static class TSWindowLabels
        {
            public static GUIContent InspectorLevel = new GUIContent("Inspector level", "Sets the level of features the shader is able to expose \n\nBasic: only basic stuff for new users \n\nNormal: the standard feature sets intended for this shader \n\nExpert: allows access to features meant for experienced users");
            public static GUIContent UpdateStream = new GUIContent("Update stream", "Selects which stream of updates the updater has to look for \n\nRelease: will check the official releases \n\nBeta: will check beta releases");
            public static GUIContent SectionStyle = new GUIContent("Section style", "Selects the appearance of the inspector \n\nBubbles: bubble looking sections \n\nBox: Sections will have A boxed header \n\nFoldoud: simplistic unity-like look");
            public static GUIContent Color = new GUIContent("Color", "Color of the section (may affects different things based on the style)");
            public static GUIContent DisableAutoUpdates = new GUIContent("Disable auto updates", "Disables auto update checks on startup, you can still update manually with the updater");	
        }


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
                        deleteStyleLocal.normal.background=TSConstants.DeleteIcon;
                    }
                    return deleteStyleLocal;
                }
            }

            private static GUIStyle upStyleLocal;
            public static GUIStyle upStyle
            {
                get
                {
                    if(upStyleLocal==null)
                    {
                        upStyleLocal=new GUIStyle();
                        upStyleLocal.normal.background=TSConstants.UpIcon;
                    }
                    return upStyleLocal;
                }
            }

            private static GUIStyle downStyleLocal;
            public static GUIStyle downStyle
            {
                get
                {
                    if(downStyleLocal==null)
                    {
                        downStyleLocal=new GUIStyle();
                        downStyleLocal.normal.background=TSConstants.DownIcon;
                    }
                    return downStyleLocal;
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