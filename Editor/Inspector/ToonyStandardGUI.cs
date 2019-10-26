using UnityEditor;
using UnityEngine;
using System;
using System.Collections;

namespace Cibbi.ToonyStandard
{
    public class ToonyStandardGUI : ShaderGUI
    {
        public enum BlendMode
        {
            Opaque,
            Cutout,
            Fade,   // Old school alpha-blending mode, fresnel does not affect amount of transparency
            Transparent // Physically plausible transparency mode, implemented as alpha pre-multiply
        }

        /// <summary>
        /// Contains all the GUIContents used by this inspector
        /// </summary>
        
        #region Properties

            MaterialProperty _RampOn;
            MaterialProperty _RimLightOn;
            MaterialProperty _SpecularOn;
            MaterialProperty _DetailMapOn;

            MaterialProperty _ToonRampBox;
            MaterialProperty _RimLightBox;
            MaterialProperty _SpecularBox;
            MaterialProperty _DetailBox;

            MainSection main;
            BasicMainSection basicMain;
            OrderedSectionGroup group;

            bool isFirstCycle=true;

            InspectorLevel inspectorLevel;

        #endregion

        /// <summary>
        /// Initializzation that happens the first time the window is created
        /// </summary>
        /// <param name="materialEditor">Material editor provided by the custom inspector</param>
        /// <param name="properties">Array of materialProperties provided by the custom inspector</param>
        public void Start(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            //Temporary code for transitioning between 2017 to 2018
            if(FindProperty("_NeedsFix", properties).floatValue==0.5)
            {
                #if UNITY_2018_1_OR_NEWER
                    FindProperty("_NeedsFix", properties).floatValue=0;
                #else
                    FindProperty("_NeedsFix", properties).floatValue=1;
                #endif
            }


            EditorGUIUtility.labelWidth = 0f;
            //material = materialEditor.target as Material;

            //Initialize properties
            FindProperties(properties);

            //Initializes the ramp section based on the inspector level
            inspectorLevel=(InspectorLevel)EditorPrefs.GetInt("TSInspectorLevel");

            switch(inspectorLevel)
            {
                case InspectorLevel.Basic:
                    basicMain = new BasicMainSection(properties);
                    break;
                case InspectorLevel.Normal:
                    main = new MainSection(properties);
                    break;
                case InspectorLevel.Expert:
                    break;
            }

            foreach (Material mat in FindProperty("_Mode", properties).targets)
            {
                // Setup various keyword based settings
                SetupMaterialWithBlendMode(mat, (BlendMode)mat.GetFloat("_Mode"));

                // Setup emission
                MaterialEditor.FixupEmissiveFlag(mat);
                bool shouldEmissionBeEnabled = (mat.globalIlluminationFlags & MaterialGlobalIlluminationFlags.EmissiveIsBlack) == 0;
                TSFunctions.SetKeyword(mat, "_EMISSION", shouldEmissionBeEnabled);
                if (shouldEmissionBeEnabled)
                {
                    mat.SetOverrideTag("IsEmissive", "true");
                }
                else
                {
                    mat.SetOverrideTag("IsEmissive", "false");
                }
            }

            GenerateRampMinMax(properties);

            // Add sections based on the inspector level
            group=new OrderedSectionGroup();
            if(inspectorLevel==InspectorLevel.Basic)
            {
                group.addSection(new BasicSpecularSection(properties, TSFunctions.BooleanFloat(_SpecularBox.floatValue), TSFunctions.BooleanFloat(_SpecularOn.floatValue)));
            }
            else
            {
                group.addSection(new RampSection(this, properties, TSFunctions.BooleanFloat(_ToonRampBox.floatValue), TSFunctions.BooleanFloat(_RampOn.floatValue)));
                group.addSection(new RimLightSection(properties, TSFunctions.BooleanFloat(_RimLightBox.floatValue), TSFunctions.BooleanFloat(_RimLightOn.floatValue)));
                group.addSection(new SpecularSection(properties, TSFunctions.BooleanFloat(_SpecularBox.floatValue), TSFunctions.BooleanFloat(_SpecularOn.floatValue)));
                group.addSection(new DetailSection(properties, TSFunctions.BooleanFloat(_DetailBox.floatValue), TSFunctions.BooleanFloat(_DetailMapOn.floatValue)));
            }       
        }


        /// <summary>
        /// Draws the GUI
        /// </summary>
        /// <param name="materialEditor">Material editor provided by the custom inspector</param>
        /// <param name="properties">Array of materialProperties provided by the custom inspector</param>
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {   
            //if is the first cycle it runs, do the initializzations needed on the start function
            if (isFirstCycle)
            {
                Start(materialEditor, properties);
                isFirstCycle=false;
            }
            //Fetching properties is needed only if the start function has not run since the start function already fetches them once
            //else
            //{
            //    FindProperties(properties);
            //}
            TSFunctions.DrawHeader(EditorGUIUtility.currentViewWidth,10);
            
            //Temporary code for converting back HDR colors
            #if UNITY_2018_1_OR_NEWER
            if(FindProperty("_NeedsFix", properties).floatValue==1)
            {
                EditorGUILayout.BeginHorizontal();
                if(GUILayout.Button("Convert HDR colors back to 2017 look"))
                {
                    foreach(MaterialProperty m in properties)
                    {
                        if(m.flags==MaterialProperty.PropFlags.HDR)
                        {
                            m.colorValue=m.colorValue.linear;
                        }
                    }
                    FindProperty("_NeedsFix", properties).floatValue=0;
                }
                if(GUILayout.Button("Keep current colors"))
                {
                    FindProperty("_NeedsFix", properties).floatValue=0;
                }
                EditorGUILayout.EndHorizontal();
            }                
            #endif

            //if a keyword is used to apply the effects on the shader caused by enabling/disabling a section, it needs to be set every update
            foreach (Material mat in _SpecularOn.targets)
            {
                TSFunctions.SetKeyword(mat, "_ENABLE_SPECULAR", mat.GetFloat(_SpecularOn.name) != 0);
                TSFunctions.SetKeyword(mat, "_DETAIL_MAP", mat.GetFloat(_DetailMapOn.name) != 0);
            }

            //draw main section
            if(inspectorLevel==InspectorLevel.Basic)
            {
                basicMain.DrawSection(materialEditor);
            }
            else
            {
                main.DrawSection(materialEditor);
            }     
            group.DrawSectionsList(materialEditor,properties);
            group.DrawAddButton();

            TSFunctions.DrawFooter();
        }

        /// <summary>
        /// Fetches all the properties, must be done very OnGUI update to be sure property changes done by external code gets correctly updated
        /// </summary>
         /// <param name="properties">Array of materialProperties provided by the custom inspector</param>
        public void FindProperties(MaterialProperty[] properties)
        {
            _RampOn = FindProperty("_RampOn", properties);
            _RimLightOn = FindProperty("_RimLightOn", properties);          
            _SpecularOn = FindProperty("_SpecularOn", properties);
            _DetailMapOn = FindProperty("_DetailMapOn", properties);

            _ToonRampBox = FindProperty("_ToonRampBox", properties);
            _RimLightBox = FindProperty("_RimLightBox", properties);
            _SpecularBox = FindProperty("_SpecularBox", properties);
            _DetailBox = FindProperty("_DetailBox", properties);
        }

        public void GenerateRampMinMax(MaterialProperty[] properties)
        {
            foreach(Material m in FindProperty("_MainRampMin", properties).targets)
            { 
                Texture2D ramp = (Texture2D)m.GetTexture("_Ramp");
                #if UNITY_2018_1_OR_NEWER
                if(!ramp.isReadable)
                {
                    SetTextureImporterFormat(ramp, true);
                }
                Color min = ramp.GetPixel(0, 0);
                Color max = ramp.GetPixel(ramp.width, ramp.height);
                #else

                Color min;
                Color max;
                try
                {
                    min = ramp.GetPixel(0, 0);
                    max = ramp.GetPixel(ramp.width, ramp.height);
                }
                catch(UnityException)
                {
                    SetTextureImporterFormat(ramp, true);
                    min = ramp.GetPixel(0, 0);
                    max = ramp.GetPixel(ramp.width, ramp.height);
                }
                #endif
                float intensity = m.GetFloat("_ShadowIntensity");
                if(intensity == 0)
                {
                    intensity = 0.001f;
                }
                Color remapMin = new Color (1-intensity, 1-intensity, 1-intensity, 1).gamma;

                min *= m.GetColor("_RampColor");
                max *= m.GetColor("_RampColor");

                min = remap(min, Color.black ,Color.white, remapMin, Color.white);
                max = remap(max, Color.black ,Color.white, remapMin, Color.white);

                m.SetColor("_MainRampMin", min); 
                m.SetColor("_MainRampMax", max); 
            }
        }
        private static Color remap(Color value, Color oldMin, Color oldMax, Color newMin, Color newMax) 
        {
            float r =(value.r - oldMin.r) / (oldMax.r - oldMin.r) * (newMax.r - newMin.r) + newMin.r;
            float g =(value.g - oldMin.g) / (oldMax.g - oldMin.g) * (newMax.g - newMin.g) + newMin.g;
            float b =(value.b - oldMin.b) / (oldMax.b - oldMin.b) * (newMax.b - newMin.b) + newMin.b;
	        return new Color(r,g,b,1);
        }

        public static void SetTextureImporterFormat( Texture2D texture, bool isReadable)
        {
            if ( null == texture ) return;

            string assetPath = AssetDatabase.GetAssetPath( texture );
            var tImporter = AssetImporter.GetAtPath( assetPath ) as TextureImporter;
            if ( tImporter != null )
            {
                tImporter.textureType = TextureImporterType.Default;

                tImporter.isReadable = isReadable;

                AssetDatabase.ImportAsset( assetPath );
                AssetDatabase.Refresh();
            }
        }

        public static void SetupMaterialWithBlendMode(Material material, BlendMode blendMode)
        {
            switch (blendMode)
            {
                case BlendMode.Opaque:
                    material.shader = Shader.Find("Hidden/Cibbis shaders/toony standard/Opaque");
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    break;
                case BlendMode.Cutout:
                    material.shader = Shader.Find("Hidden/Cibbis shaders/toony standard/Cutout");
                    material.EnableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    break;
                case BlendMode.Fade:
                    material.shader = Shader.Find("Hidden/Cibbis shaders/toony standard/Fade");
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    break;
                case BlendMode.Transparent:
                    material.shader = Shader.Find("Hidden/Cibbis shaders/toony standard/Transparent");
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                    break;
            }
        }
    }
}