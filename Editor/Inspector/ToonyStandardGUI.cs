using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.IO;

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
            MaterialProperty _SSSOn;
            MaterialProperty _StencilOn;
            MaterialProperty _OutlineOn;

            MaterialProperty _ToonRampBox;
            MaterialProperty _RimLightBox;
            MaterialProperty _SpecularBox;
            MaterialProperty _DetailBox;
            MaterialProperty _SSSBox;
            MaterialProperty _StencilBox;
            MaterialProperty _OutlineBox;

            MainSection main;
            BasicMainSection basicMain;
            OrderedSectionGroup group;

            bool isFirstCycle=true;

            InspectorLevel inspectorLevel;
            public TexturePacker packer;

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
                    packer = new TexturePacker(TexturePacker.Resolution.M_512x512, new string[]{"Metallic", "Smoothness","Ambient occlusion", "Detail map"}, GetTextureDestinationPath((Material)_RampOn.targets[0],"MSOD.png"));
                    main = new MainSection(properties, inspectorLevel, packer, this);
                    break;
                case InspectorLevel.Expert:
                    packer = new TexturePacker(TexturePacker.Resolution.M_512x512, new string[]{"Metallic", "Smoothness","Ambient occlusion", "Detail map"}, GetTextureDestinationPath((Material)_RampOn.targets[0],"MSOD.png"));
                    main = new MainSection(properties, inspectorLevel, packer, this);
                    break;
            }

            foreach (Material mat in FindProperty("_Mode", properties).targets)
            {
                // Setup various keyword based settings
                SetupMaterialWithBlendMode(mat, (BlendMode)mat.GetFloat("_Mode"), mat.GetFloat("_OutlineOn")>0);

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
                group.addSection(new OutlineSection(properties, TSFunctions.BooleanFloat(_OutlineBox.floatValue), TSFunctions.BooleanFloat(_OutlineOn.floatValue)));
            }
            else
            {
                group.addSection(new RampSection(this, properties, TSFunctions.BooleanFloat(_ToonRampBox.floatValue), TSFunctions.BooleanFloat(_RampOn.floatValue)));
                group.addSection(new RimLightSection(properties, TSFunctions.BooleanFloat(_RimLightBox.floatValue), TSFunctions.BooleanFloat(_RimLightOn.floatValue)));
                group.addSection(new SpecularSection(properties, inspectorLevel, this, TSFunctions.BooleanFloat(_SpecularBox.floatValue), TSFunctions.BooleanFloat(_SpecularOn.floatValue)));
                group.addSection(new DetailSection(properties, TSFunctions.BooleanFloat(_DetailBox.floatValue), TSFunctions.BooleanFloat(_DetailMapOn.floatValue)));
                group.addSection(new SubsurfaceSection(properties, TSFunctions.BooleanFloat(_SSSBox.floatValue), TSFunctions.BooleanFloat(_SSSOn.floatValue)));
                group.addSection(new OutlineSection(properties, TSFunctions.BooleanFloat(_OutlineBox.floatValue), TSFunctions.BooleanFloat(_OutlineOn.floatValue)));
            }   

            if(inspectorLevel == InspectorLevel.Expert)
            {
                 group.addSection(new StencilSection(properties, TSFunctions.BooleanFloat(_StencilBox.floatValue), TSFunctions.BooleanFloat(_StencilOn.floatValue)));
            }
            else
            {
                FindProperty("_StencilID", properties).floatValue = 0;
                FindProperty("_StencilComp", properties).floatValue = 0;
                FindProperty("_StencilOp", properties).floatValue = 0;
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
                TSFunctions.SetKeyword(mat, "_SPECULARHIGHLIGHTS_OFF", !(mat.GetFloat(_SpecularOn.name) != 0));
                TSFunctions.SetKeyword(mat, "_DETAIL_MULX2", mat.GetFloat(_DetailMapOn.name) != 0);
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
            _SSSOn = FindProperty("_SSSOn", properties);
            _StencilOn = FindProperty("_StencilOn", properties);
            _OutlineOn = FindProperty("_OutlineOn", properties);

            _ToonRampBox = FindProperty("_ToonRampBox", properties);
            _RimLightBox = FindProperty("_RimLightBox", properties);
            _SpecularBox = FindProperty("_SpecularBox", properties);
            _DetailBox = FindProperty("_DetailBox", properties);
            _SSSBox = FindProperty("_SSSBox", properties);
            _StencilBox = FindProperty("_StencilBox", properties);
            _OutlineBox = FindProperty("_OutlineBox", properties);
        }

        public void GenerateRampMinMax(MaterialProperty[] properties)
        {
            foreach(Material m in FindProperty("_MainRampMin", properties).targets)
            { 
                Texture2D ramp = (Texture2D)m.GetTexture("_Ramp");
                Color min= new Color(100,100,100,0);
                Color max= new Color(0,0,0,1);
                if(ramp != null)
                {
                    #if UNITY_2018_1_OR_NEWER
                    if(!ramp.isReadable)
                    {
                        SetTextureImporterFormat(ramp, true);
                    }
                    foreach (Color c in ramp.GetPixels())
                    {
                        if(min.r > c.r) {min.r = c.r;}
                        if(min.g > c.g) {min.g = c.g;}
                        if(min.b > c.b) {min.b = c.b;}
                        if(max.r < c.r) {max.r = c.r;}
                        if(max.g < c.g) {max.g = c.g;}
                        if(max.b < c.b) {max.b = c.b;}
                        
                    }
                    //Debug.Log("1: m "+min+" max "+max);
                    //min = ramp.GetPixel(0, 0);
                    //max = ramp.GetPixel(ramp.width, ramp.height);
                    //Debug.Log("2: m "+min+" max "+max);
                    #else

                    
                    try
                    {
                    foreach (Color c in ramp.GetPixels())
                    {
                        if(min.r > c.r) {min.r = c.r;}
                        if(min.g > c.g) {min.g = c.g;}
                        if(min.b > c.b) {min.b = c.b;}
                        if(max.r < c.r) {max.r = c.r;}
                        if(max.g < c.g) {max.g = c.g;}
                        if(max.b < c.b) {max.b = c.b;}
                        
                    }
                        //min = ramp.GetPixel(0, 0);
                        //max = ramp.GetPixel(ramp.width, ramp.height);
                    }
                    catch(UnityException)
                    {
                        SetTextureImporterFormat(ramp, true);
                        foreach (Color c in ramp.GetPixels())
                        {
                            if(min.r > c.r) {min.r = c.r;}
                            if(min.g > c.g) {min.g = c.g;}
                            if(min.b > c.b) {min.b = c.b;}
                            if(max.r < c.r) {max.r = c.r;}
                            if(max.g < c.g) {max.g = c.g;}
                            if(max.b < c.b) {max.b = c.b;}
                            
                        }
                        //min = ramp.GetPixel(0, 0);
                        //max = ramp.GetPixel(ramp.width, ramp.height);
                    }
                    #endif
                }
                else
                {
                    min = new Color(0.9f,0.9f,0.9f,1);
                    max = Color.white;
                }
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
        
        public string GetTextureDestinationPath(Material mat, string name)
        {
            string path = AssetDatabase.GetAssetPath(mat);
            string[] pieces = path.Split('/');
            ArrayUtility.RemoveAt(ref pieces, pieces.Length - 1);
            path = string.Join("/", pieces);
            ArrayUtility.RemoveAt(ref pieces, pieces.Length - 1);
            string pathTexture = string.Join("/", pieces);
            if(Directory.Exists(Application.dataPath + pathTexture.Substring(-1==pathTexture.IndexOf("/")?0:pathTexture.IndexOf("/")) + "/Textures"))
            {
                path = pathTexture + "/Textures/" + mat.name + name;
            }
            else
            {
                path = path + "/" + mat.name + name;
            }
            return path;
        }

        public void RegenerateMSOD()
        {
            foreach (Material mat in _RampOn.targets)
            {
                string path = GetTextureDestinationPath(mat, "MSOD.png");
                if(mat.GetTexture("_MetallicMap") != null || mat.GetTexture("_GlossinessMap") != null || mat.GetTexture("_OcclusionMap") != null || mat.GetTexture("_DetailMask") != null)
                {   
                    packer.resolution = TexturePacker.Resolution.XS_128x128;
                    packer.rTexture = (Texture2D)mat.GetTexture("_MetallicMap");
                    if(packer.rTexture != null)
                        while(packer.rTexture.width>(float)packer.resolution||packer.rTexture.height>(float)packer.resolution)
                        {
                            if(packer.RiseResolutionByOneLevel()==0)
                                break;
                        }
                    packer.gTexture = (Texture2D)mat.GetTexture("_GlossinessMap");
                    if(packer.gTexture != null)
                        while(packer.gTexture.width>(float)packer.resolution||packer.gTexture.height>(float)packer.resolution)
                        {
                            if(packer.RiseResolutionByOneLevel()==0)
                                break;
                        }
                    packer.bTexture = (Texture2D)mat.GetTexture("_OcclusionMap");
                    if(packer.bTexture != null)
                        while(packer.bTexture.width>(float)packer.resolution||packer.bTexture.height>(float)packer.resolution)
                        {
                            if(packer.RiseResolutionByOneLevel()==0)
                                break;
                        }
                    packer.aTexture = (Texture2D)mat.GetTexture("_DetailMask");
                    if(packer.aTexture != null)
                        while(packer.aTexture.width>(float)packer.resolution||packer.aTexture.height>(float)packer.resolution)
                        {
                            if(packer.RiseResolutionByOneLevel()==0)
                                break;
                        }

                    packer.PackTexture(path);
                    mat.SetTexture("_MSOD", packer.resultTex);
                }
                else
                {
                    mat.SetTexture("_MSOD", null);
                }
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

        public static void SetupMaterialWithBlendMode(Material material, BlendMode blendMode, bool outlined)
        {
            string shaderName="";
            switch (blendMode)
            {
                case BlendMode.Opaque:
                    shaderName = "Hidden/Cibbis shaders/toony standard/Opaque";
                    //material.DisableKeyword("_ALPHATEST_ON");
                    //material.DisableKeyword("_ALPHABLEND_ON");
                    //material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    break;
                case BlendMode.Cutout:
                    shaderName = "Hidden/Cibbis shaders/toony standard/Cutout";
                    //material.EnableKeyword("_ALPHATEST_ON");
                    //material.DisableKeyword("_ALPHABLEND_ON");
                    //material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    break;
                case BlendMode.Fade:
                    shaderName = "Hidden/Cibbis shaders/toony standard/Fade";
                    //material.DisableKeyword("_ALPHATEST_ON");
                    //material.EnableKeyword("_ALPHABLEND_ON");
                    //material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    break;
                case BlendMode.Transparent:
                    shaderName = "Hidden/Cibbis shaders/toony standard/Transparent";
                    //material.DisableKeyword("_ALPHATEST_ON");
                    //material.DisableKeyword("_ALPHABLEND_ON");
                    //material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                    break;
            }
            if(outlined)
            {
                shaderName += "Outlined";
            }
             material.shader = Shader.Find(shaderName);
        }  
    }
}