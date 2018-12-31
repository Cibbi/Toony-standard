using UnityEditor;
using UnityEngine;
using System;
using Cibbi.SimpleInspectors;

public class ToonyStandardGUI : SimpleInspector 
{
    /* static bool rampOpen;
    static bool specularOpen;
    static bool detailOpen;
    static bool rimOpen;*/

    bool rimNeedsReset;

    public enum BlendMode
    {
        Opaque,
        Cutout,
        Fade,   // Old school alpha-blending mode, fresnel does not affect amount of transparency
        Transparent // Physically plausible transparency mode, implemented as alpha pre-multiply
    }
    public enum IndirectSpecular
    {
        Probe,
        Matcap,
        Cubemap,
        Color
    }
    public enum Workflow
    {
        Metallic,
        Specular
    }
    public enum SpMode
    {
        Standard,
        Anisotropic,
        Fake
    }

	private static class Styles
    {
        public static GUIContent cullMode = new GUIContent("Cull mode", "Controls which face of the mesh is rendered \n\nOff: Double sided \n\nFront: Single sided (internal parts showing) \n\nBack: Single sided");
        public static GUIContent blendMode = new GUIContent("Blend mode", "Blend state \n\nOpaque: Opaque object \n\nCutout: Opaque object with cutout parts decided by the alpha channel of the main texture"+
                                                            " \n\nFade: Transparent object that does completely fade out at 0 opacity \n\nTransparent: Transparent object that is still visible at 0 opacity due to the fresnel effect, more realistic than fade");
        public static GUIContent mainTex = new GUIContent("Main texture", "Main texture (RGB channels) and transparency (A channel)");
        public static GUIContent cutOff = new GUIContent("Alpha cutoff", "Transparency threshold to cut out");
        public static GUIContent normal = new GUIContent("Normal", "Normal Map");
        public static GUIContent emission = new GUIContent("Color", "Emission map and Color");
        public static GUIContent occlusion = new GUIContent("Occlusion", "Occlusion map and intensity");  
        
        public static GUIContent ramp = new GUIContent("Toon ramp", "Toon ramp texture");
        public static GUIContent rampOffset = new GUIContent("Ramp offset", "Applies an offset that shifts the ramp texture, usefull to avoid to make different toon ramps that are really similar");
        public static GUIContent occlusionOffset = new GUIContent("Occlusion ramp offset", "Uses the occlusion texture to apply an additional offset to the toon ramp on specific zones");
        public static GUIContent occlusionOffsetIntensity = new GUIContent("Occlusion offset intensity", "intensity of the occlusion driven ramp offset");
        public static GUIContent shadowIntensity = new GUIContent("Shadow intensity", "Defines how intense the toon ramp is");
        public static GUIContent fakeLight = new GUIContent("Fake light", "If enabled and in a world without a main directional light, will fake one based on parameters below");
        
        public static GUIContent rimStrength = new GUIContent("Rim strength", "Defines how far the rim light extends");
        public static GUIContent rimSharpness = new GUIContent("Rim sharpness", "Defines how sharp the rim is");
        public static GUIContent rimIntensity = new GUIContent("Rim intensity", "Defines the intensity of the rim, below 0 will make a rim darker than the base");

        public static GUIContent indirectSpecular = new GUIContent("Indirect source", "Defines the source of the indirect specular \n\nProbe: uses the reflection probe in the world \n\nMatcap: uses a matcap texture \n\nCubemap uses a cubemap \n\nColor: uses a single color");
        public static GUIContent workflow = new GUIContent("Workflow", "Defines the workflow type \n\nMetallic: uses a texture that defines the metalness \n\nSpecular: uses a specular map");
        public static GUIContent spMode = new GUIContent("Specular mode", "Defines the type of specular used \n\nStandard: uses the model used in the standard shader \n\nAnisotropic: uses anisotropic reflections \n\nFake: uses a texture as highlights");
        public static GUIContent smoothness = new GUIContent("Smoothness", "Smoothness map and intensity, usually the slider is set to 1 when using a smoothness texture");
        public static GUIContent metallic = new GUIContent("Metallic", "Metallic map and intensity, usually the slider is set to 1 when using a metallic texture");
        public static GUIContent specular = new GUIContent("Specular map", "Specular map");
        public static GUIContent tangent = new GUIContent("Tangent", "Tangent map");
        public static GUIContent anisotropy = new GUIContent("Anisotropy", "anisotropy map and intensity, usually the slider is set to 1 when using a anisotropy texture");
        public static GUIContent fakeHighlights = new GUIContent("Fake highlights", "Texture that will be used for having an highlight effect instead of being calculated");
        public static GUIContent matcap = new GUIContent("Matcap", "Matcap texture that will be used like a reflection probe");
        public static GUIContent cubemap = new GUIContent("Cubemap", "Cubemap that will be used as a reflection probe");
        public static GUIContent indirectColor = new GUIContent("Color", "Color that will be used for the indirect");
        public static GUIContent toonyHighlight = new GUIContent("Toony highlights", "Make the the current highlights toony style");
        public static GUIContent highlightRamp = new GUIContent("Highlight ramp", "Highlight ramp texture and color tint");
        public static GUIContent hightlightRampOffset = new GUIContent("Highlight ramp offset", "Applies an offset that shifts the ramp texture, usefull to avoid to make different highlight ramps that are really similar");
        public static GUIContent highlightIntensity = new GUIContent("Highlight intensity", "Defines how intense the highlight ramp is");
        public static GUIContent fakeHighlightIntensity = new GUIContent("Highlight intensity", "Defines how intense the fake highlight is");
        public static GUIContent highlightPattern = new GUIContent("Highlight pattern", "Pattern mask for the highlights (clearly not inspired by Xiexe's shader)");
        
        public static GUIContent detailMask = new GUIContent("Detail mask", "Detail mask used to decide where the detail map should be visible or not");
        public static GUIContent detailIntensity = new GUIContent("Detail intensity", "used to decide the intensity of the detail map");
        public static GUIContent detailPattern = new GUIContent("Detail pattern", "Detail texture and color");
        public static GUIContent detailNormal = new GUIContent("Detail normal", "Detail Normal Map with scale property");

        public static GUIContent rampOptions = new GUIContent("Toon Ramp Options", "Various options for the toon ramp");
        public static GUIContent rimOptions = new GUIContent("Rim Light Options", "Various options for rim light, can be disabled");
        public static GUIContent specularOptions = new GUIContent("Specular Options", "Various options for specular calculations, can be disabled");
        public static GUIContent detailOptions = new GUIContent("Detail Options", "Various options for detail textures, can be disabled");     
    }

	 	StoredSelectorProperty _blendMode;
        StoredShaderProperty _Cull;

        StoredTextureProperty _MainTex;
        StoredShaderProperty _Cutoff;
        StoredTextureProperty _BumpMap;
        StoredTextureProperty _Emission;
        StoredTextureProperty _Occlusion;
        StoredTileAndOffset _MainTileAndOffset;

        StoredTextureProperty _Ramp;
        StoredShaderProperty _RampOffset;
        StoredShaderProperty _ShadowIntensity;
        StoredShaderProperty _OcclusionOffsetIntensity;
        StoredShaderProperty _FakeLightColor;
        StoredShaderProperty _FakeLightX;
        StoredShaderProperty _FakeLightY;
        StoredShaderProperty _FakeLightZ;
        
        StoredShaderProperty _RimStrength;
        StoredShaderProperty _RimSharpness;
        StoredShaderProperty _RimIntensity;

        StoredSelectorProperty _indirectSpecular;
        StoredSelectorProperty _workflow;
        StoredSelectorProperty _SpMode;       
        StoredTextureProperty _GlossinessMap;
        StoredTextureProperty _MetallicMap;
        StoredTextureProperty _SpecularMap;
        StoredTextureProperty _AnisotropyMap;
        StoredTextureProperty _TangentMap;
        StoredTextureProperty _FakeHightlights;
        StoredTextureProperty _Matcap;
        StoredTextureProperty _Cubemap;
        StoredShaderProperty _IndirectColor;
        StoredTextureProperty _HighlightRamp;
        StoredShaderProperty _HighlightRampOffset;
        StoredShaderProperty _HighlightIntensity;
        StoredShaderProperty _FakeHighlightIntensity;
        StoredTextureProperty _HighlightPattern;
       
        StoredTextureProperty _DetailMask;
        StoredShaderProperty _DetailIntensity;
        StoredTextureProperty _DetailTexture;
        StoredTextureProperty _DetailBumpMap;
        StoredTileAndOffset _DetailTileAndOffset;

		Material material;

        StoredToggle fakeLight;
        StoredToggle occlusionOffset;
        StoredToggle toonyHighlights;

        PropertiesBox rampOptions;
        PropertiesBox rimOptions;
        PropertiesBox specularOptions;
        PropertiesBox detailOptions;

        StoredPropertyList occlusionOffsetOptions;
        StoredPropertyList fakeLightOptions;
        StoredPropertyList metallicWorkflow;
        StoredPropertyList specularWorkflow;
        StoredPropertyList anisotropicOptions;
        StoredPropertyList fakeHighlightOptions;
        StoredPropertyList matcapOptions;
        StoredPropertyList cubemapOptions;
        StoredPropertyList indirectColorOptions;
        StoredPropertyList toonyHighlightsOptions;

        MaterialProperty _ToonyHighlights;
        MaterialProperty _FakeLight;
        MaterialProperty _OcclusionOffset;
        MaterialProperty _EnableSpecular;
		MaterialProperty _DetailMap;
		MaterialProperty _ToonRampBox;
		MaterialProperty _RimLightBox;
		MaterialProperty _SpecularBox;
		MaterialProperty _DetailBox;

        Texture2D icon;

        Texture2D gitHubIcon;
        Texture2D patreonIcon;
        //Texture2D paypalIcon;

        //Vector2 scrollPos;

    public override void Start(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        string[] icons = AssetDatabase.FindAssets("ToonyStandardLogo t:Texture2D", null);
		if (icons.Length>0) 
        {
            string [] pieces=AssetDatabase.GUIDToAssetPath(icons[0]).Split('/');
            ArrayUtility.RemoveAt(ref pieces,pieces.Length-1);
            string path=string.Join("/",pieces);
			icon=AssetDatabase.LoadAssetAtPath<Texture2D>(path+"/ToonyStandardLogo.png");
            gitHubIcon=AssetDatabase.LoadAssetAtPath<Texture2D>(path+"/GitHubIcon.png");
            patreonIcon=AssetDatabase.LoadAssetAtPath<Texture2D>(path+"/PatreonIcon.png");
            //paypalIcon=AssetDatabase.LoadAssetAtPath<Texture2D>(path+"/PaypalIcon.png");
             
		}

        material = materialEditor.target as Material;
        //initialize properties
        FindProperties(properties);
        rimNeedsReset=false;
		material = materialEditor.target as Material;

        SetKeyword(material, "_ENABLE_SPECULAR",_EnableSpecular.floatValue==1);
        SetKeyword(material, "_DETAIL_MAP",_DetailMap.floatValue==1);

        //setup various keyword based settings
        SetupMaterialWithBlendMode((Material)material, (BlendMode)material.GetFloat("_Mode"));
        SetupIndirectSource((Material)material, (IndirectSpecular)material.GetFloat("_IndirectSpecular"));
        SetupWorkflow((Material)material, (Workflow)material.GetFloat("_Workflow"));

        //setup emission
        MaterialEditor.FixupEmissiveFlag(material);
        bool shouldEmissionBeEnabled = (material.globalIlluminationFlags & MaterialGlobalIlluminationFlags.EmissiveIsBlack) == 0;
        SetKeyword(material, "_EMISSION", shouldEmissionBeEnabled);
        if (shouldEmissionBeEnabled)
        {
            material.SetOverrideTag("IsEmissive", "true");
        }
        else
        {
            material.SetOverrideTag("IsEmissive", "false");
        }

        UpdateComponents();

        
    }

    public override void Update(MaterialEditor materialEditor, MaterialProperty[] properties)
    { 

        
        //scrollPos = EditorGUILayout.BeginScrollView(scrollPos,GUILayout.MinHeight(500),GUILayout.MaxHeight(2000));
        
        
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label(icon,GUILayout.Width(icon.width),GUILayout.Height(icon.height));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        


        FindProperties(properties);
        UpdateComponents();

        rimOptions=new PropertiesBox(Styles.rimOptions,BooleanFloat(_RimLightBox.floatValue),true,_RimIntensity.GetStoredProperty().floatValue!=0);
        rimOptions.AddProperty(_RimIntensity);
        rimOptions.AddProperty(_RimStrength);
        rimOptions.AddProperty(_RimSharpness);

        //draw blend mode
        EditorGUI.BeginChangeCheck();
        _blendMode.DrawProperty(materialEditor);
        if (EditorGUI.EndChangeCheck())
        {
            SetupMaterialWithBlendMode(material, (BlendMode)_blendMode.getSelectedOption());        
        }
        
        //draw cull mode
        _Cull.DrawProperty(materialEditor);
        EditorGUILayout.Space();

        //draw main properties
        _MainTex.DrawProperty(materialEditor);
        if((BlendMode)_blendMode.getSelectedOption()==BlendMode.Cutout)
        {
            EditorGUI.indentLevel+=MaterialEditor.kMiniTextureFieldLabelIndentLevel + 1;
            _Cutoff.DrawProperty(materialEditor);
            EditorGUI.indentLevel-=MaterialEditor.kMiniTextureFieldLabelIndentLevel + 1;
        }
        _BumpMap.DrawProperty(materialEditor);
        _Occlusion.DrawProperty(materialEditor);


        //emission
        EditorGUI.BeginChangeCheck();
        if (materialEditor.EmissionEnabledProperty())
        {
            //bool hadEmissionTexture = _Emission.GetGetExtraProperty1().textureValue != null;
            // Texture and HDR color controls
            _Emission.DrawProperty(materialEditor);
            // If texture was assigned and color was black set color to white
            //float brightness = _Emission.GetGetExtraProperty1().colorValue.maxColorComponent;
           // if (_Emission.GetGetStoredTextureProperty().textureValue != null && !hadEmissionTexture && brightness <= 0f)
            //    _Emission.GetGetExtraProperty1().colorValue = Color.white;
            // change the GI flag and fix it up with emissive as black if necessary
            // materialEditor.LightmapEmissionFlagsProperty(, true);
            materialEditor.LightmapEmissionProperty(MaterialEditor.kMiniTextureFieldLabelIndentLevel);
        }
        if(EditorGUI.EndChangeCheck())
        {
            MaterialEditor.FixupEmissiveFlag(material);
            bool shouldEmissionBeEnabled = (material.globalIlluminationFlags & MaterialGlobalIlluminationFlags.EmissiveIsBlack) == 0;
            SetKeyword(material, "_EMISSION", shouldEmissionBeEnabled);
            if (shouldEmissionBeEnabled)
            {
                material.SetOverrideTag("IsEmissive", "true");
            }
            else
            {
                material.SetOverrideTag("IsEmissive", "false");
            }
        }
        _MainTileAndOffset.DrawProperty(materialEditor);

        EditorGUILayout.Space();

        //ramp options box
        EditorGUI.BeginChangeCheck();
        rampOptions.DrawBox(materialEditor);
        if (EditorGUI.EndChangeCheck())
        {
            _ToonRampBox.floatValue=floatBoolean(rampOptions.IsOpen());
            //SetKeyword(material, "_FAKE_LIGHT",fakeLight.IsEnabled());
            _OcclusionOffset.floatValue=floatBoolean(occlusionOffset.IsEnabled());
            occlusionOffsetOptions.Enable(occlusionOffset.IsEnabled());
            _FakeLight.floatValue=floatBoolean(fakeLight.IsEnabled());
            fakeLightOptions.Enable(fakeLight.IsEnabled());  

            if(!occlusionOffset.IsEnabled())
            {
                _OcclusionOffsetIntensity.GetStoredProperty().floatValue=0;
            }      
        }
        EditorGUILayout.Space();

        //rim light options box
        EditorGUI.BeginChangeCheck();
        rimOptions.DrawBox(materialEditor);
        if (EditorGUI.EndChangeCheck())
        {
            _RimLightBox.floatValue=floatBoolean(rimOptions.IsOpen());
            //SetKeyword(material, "_RIM_LIGHT",rimOptions.IsEnabled());
            if(rimOptions.IsEnabled() && rimNeedsReset)
            {
                _RimIntensity.GetStoredProperty().floatValue=1;
                rimNeedsReset=false;
                
            }
            else if (!rimOptions.IsEnabled())
            {
                _RimIntensity.GetStoredProperty().floatValue=0;
                rimNeedsReset=true;
            }      
        }
        EditorGUILayout.Space();

        //Specular options box
        EditorGUI.BeginChangeCheck();
        specularOptions.DrawBox(materialEditor);
        if (EditorGUI.EndChangeCheck())
        {
            _SpecularBox.floatValue=floatBoolean(specularOptions.IsOpen());
            SetKeyword(material, "_ENABLE_SPECULAR",specularOptions.IsEnabled());
            _EnableSpecular.floatValue=floatBoolean(material.IsKeywordEnabled("_ENABLE_SPECULAR"));
            SetupWorkflow(material,(Workflow)_workflow.getSelectedOption());
            SetupSpMode(material,(SpMode)_SpMode.getSelectedOption());
            //SetupIndirectSource(material,(IndirectSpecular)_indirectSpecular.getSelectedOption());

            metallicWorkflow.Enable((Workflow)_workflow.getSelectedOption()==Workflow.Metallic);
            specularWorkflow.Enable((Workflow)_workflow.getSelectedOption()==Workflow.Specular);
            anisotropicOptions.Enable((SpMode)_SpMode.getSelectedOption()==SpMode.Anisotropic);
            matcapOptions.Enable((IndirectSpecular)_indirectSpecular.getSelectedOption()==IndirectSpecular.Matcap);
            cubemapOptions.Enable((IndirectSpecular)_indirectSpecular.getSelectedOption()==IndirectSpecular.Cubemap);
            //SetKeyword(material,"_TOONY_HIGHLIGHTS",toonyHighlights.IsEnabled()); 
            _ToonyHighlights.floatValue=floatBoolean(toonyHighlights.IsEnabled());
            toonyHighlightsOptions.Enable(toonyHighlights.IsEnabled());
        }
        EditorGUILayout.Space();

        //detail options box
        EditorGUI.BeginChangeCheck();
        detailOptions.DrawBox(materialEditor);
        if (EditorGUI.EndChangeCheck())
        {
            _DetailBox.floatValue=floatBoolean(detailOptions.IsOpen());
            SetKeyword(material, "_DETAIL_MAP",detailOptions.IsEnabled());  
            _DetailMap.floatValue=floatBoolean(material.IsKeywordEnabled("_DETAIL_MAP"));     
        }
        EditorGUILayout.Space();

        //EditorGUILayout.EndScrollView();
        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent(gitHubIcon,"Check the official GitHub!"),"label", GUILayout.Width(32), GUILayout.Height(43)))
            {
                Application.OpenURL("https://github.com/Cibbi/Toony-standard");
            }
            
            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
            if (GUILayout.Button(new GUIContent(patreonIcon,"Want to gift me pizza every month? Become a patreon!"),"label", GUILayout.Width(32), GUILayout.Height(32)))
            {
               Application.OpenURL("https://www.patreon.com/Cibbi");
            }
            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            GUIStyle aboutLabelStyle = new GUIStyle(EditorStyles.miniLabel);
        	aboutLabelStyle.alignment = TextAnchor.LowerRight;
            aboutLabelStyle.fontStyle = FontStyle.Italic;
            aboutLabelStyle.hover.textColor=Color.magenta;
            //sectionStyle.normal.textColor=new Color(.7f,.7f,.7f);
            //EditorGUILayout.LabelField("Toony Standard prerelase 3",sectionStyle,GUILayout.Height(32));
            if(GUILayout.Button("Toony Standard 1.0",aboutLabelStyle,GUILayout.Height(32)))
            {
                ToonyStandardAboutWindow window = EditorWindow.GetWindow (typeof(ToonyStandardAboutWindow)) as ToonyStandardAboutWindow;
				window.minSize = new Vector2 (475, 200);
                window.maxSize = new Vector2 (475, 200);
                window.titleContent = new GUIContent("About");
            }

            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
        GUILayout.EndHorizontal();


    }

	public void FindProperties(MaterialProperty[] properties)
	{
        _blendMode=new StoredSelectorProperty(Styles.blendMode,Enum.GetNames(typeof(BlendMode)),FindProperty("_Mode", properties));
        _Cull = new StoredShaderProperty(Styles.cullMode,FindProperty("_Cull", properties));

        _MainTex =new StoredTextureProperty(Styles.mainTex,ShaderGUI.FindProperty("_MainTex", properties),ShaderGUI.FindProperty("_Color", properties));
        _Cutoff = new StoredShaderProperty(Styles.cutOff,FindProperty("_Cutoff", properties));
        _BumpMap = new StoredTextureProperty(Styles.normal,ShaderGUI.FindProperty("_BumpMap", properties),ShaderGUI.FindProperty("_BumpScale", properties)); 
        _Emission =new StoredTextureProperty(Styles.emission,ShaderGUI.FindProperty("_EmissionMap", properties),ShaderGUI.FindProperty("_EmissionColor", properties)); 
        _Occlusion =new StoredTextureProperty(Styles.occlusion,ShaderGUI.FindProperty("_OcclusionMap", properties),ShaderGUI.FindProperty("_Occlusion", properties)); 
        _MainTileAndOffset = new StoredTileAndOffset(ShaderGUI.FindProperty("_MainTex", properties));
        
        _Ramp = new StoredTextureProperty(Styles.ramp,ShaderGUI.FindProperty("_Ramp", properties),ShaderGUI.FindProperty("_RampColor", properties));         
        _RampOffset = new StoredShaderProperty(Styles.rampOffset,ShaderGUI.FindProperty("_RampOffset", properties));
        _ShadowIntensity = new StoredShaderProperty(Styles.shadowIntensity,ShaderGUI.FindProperty("_ShadowIntensity", properties));
        _OcclusionOffsetIntensity = new StoredShaderProperty(Styles.occlusionOffsetIntensity,ShaderGUI.FindProperty("_OcclusionOffsetIntensity", properties));
        _FakeLightColor = new StoredShaderProperty("Fake light color", ShaderGUI.FindProperty("_FakeLightColor", properties));
        _FakeLightX = new StoredShaderProperty("X", ShaderGUI.FindProperty("_FakeLightX", properties));
        _FakeLightY = new StoredShaderProperty("Y", ShaderGUI.FindProperty("_FakeLightY", properties));
        _FakeLightZ = new StoredShaderProperty("Z",ShaderGUI.FindProperty("_FakeLightZ", properties)); 

        _RimStrength = new StoredShaderProperty(Styles.rimStrength, ShaderGUI.FindProperty("_RimStrength", properties));
        _RimSharpness = new StoredShaderProperty(Styles.rimSharpness, ShaderGUI.FindProperty("_RimSharpness", properties));
        _RimIntensity = new StoredShaderProperty(Styles.rimIntensity, ShaderGUI.FindProperty("_RimIntensity", properties));

        _indirectSpecular = new StoredSelectorProperty(Styles.indirectSpecular,Enum.GetNames(typeof(IndirectSpecular)),FindProperty("_IndirectSpecular", properties)); 
        _workflow = new StoredSelectorProperty(Styles.workflow,Enum.GetNames(typeof(Workflow)),FindProperty("_Workflow", properties));
        _SpMode = new StoredSelectorProperty(Styles.spMode,Enum.GetNames(typeof(SpMode)),FindProperty("_SpMode", properties));
        _GlossinessMap = new StoredTextureProperty(Styles.smoothness,ShaderGUI.FindProperty("_GlossinessMap", properties),ShaderGUI.FindProperty("_Glossiness", properties)); 
        _MetallicMap = new StoredTextureProperty(Styles.metallic,ShaderGUI.FindProperty("_MetallicMap", properties),ShaderGUI.FindProperty("_Metallic", properties));
        _SpecularMap = new StoredTextureProperty(Styles.specular,ShaderGUI.FindProperty("_MetallicMap", properties));
        _AnisotropyMap = new StoredTextureProperty(Styles.anisotropy,ShaderGUI.FindProperty("_AnisotropyMap", properties),ShaderGUI.FindProperty("_Anisotropy", properties));
        _TangentMap = new StoredTextureProperty(Styles.tangent,ShaderGUI.FindProperty("_TangentMap", properties));
        _FakeHightlights = new StoredTextureProperty(Styles.fakeHighlights,ShaderGUI.FindProperty("_FakeHighlights", properties));
        _Matcap = new StoredTextureProperty(Styles.matcap,ShaderGUI.FindProperty("_Matcap", properties));
        _Cubemap = new StoredTextureProperty(Styles.cubemap,ShaderGUI.FindProperty("_Cubemap", properties));
        _IndirectColor = new StoredShaderProperty(Styles.indirectColor,FindProperty("_IndirectColor", properties));
        _HighlightRamp = new StoredTextureProperty(Styles.highlightRamp,ShaderGUI.FindProperty("_HighlightRamp", properties),ShaderGUI.FindProperty("_HighlightRampColor", properties));
        _HighlightRampOffset = new StoredShaderProperty(Styles.hightlightRampOffset,ShaderGUI.FindProperty("_HighlightRampOffset", properties));
        _HighlightIntensity = new StoredShaderProperty(Styles.highlightIntensity,ShaderGUI.FindProperty("_HighlightIntensity", properties));  
        _FakeHighlightIntensity = new StoredShaderProperty(Styles.fakeHighlightIntensity,ShaderGUI.FindProperty("_FakeHighlightIntensity", properties));  
        _HighlightPattern = new StoredTextureProperty(Styles.highlightPattern,ShaderGUI.FindProperty("_HighlightPattern", properties));

        _DetailMask = new StoredTextureProperty(Styles.detailMask,ShaderGUI.FindProperty("_DetailMask", properties));
        _DetailIntensity = new StoredShaderProperty(Styles.detailIntensity,ShaderGUI.FindProperty("_DetailIntensity", properties));  
        _DetailTexture = new StoredTextureProperty(Styles.detailPattern, ShaderGUI.FindProperty("_DetailTexture", properties),ShaderGUI.FindProperty("_DetailColor", properties));
        _DetailBumpMap = new StoredTextureProperty(Styles.detailNormal,ShaderGUI.FindProperty("_DetailBumpMap", properties),ShaderGUI.FindProperty("_DetailBumpScale", properties));
        _DetailTileAndOffset = new StoredTileAndOffset(ShaderGUI.FindProperty("_DetailTexture", properties));

        _HighlightPattern.ShowTextureScaleAndOffset(true);

        _ToonyHighlights = ShaderGUI.FindProperty("_ToonyHighlights", properties);
        _OcclusionOffset = ShaderGUI.FindProperty("_OcclusionOffset", properties);
        _FakeLight = ShaderGUI.FindProperty("_FakeLight", properties);
		_EnableSpecular = ShaderGUI.FindProperty("_EnableSpecular", properties);
		_DetailMap = ShaderGUI.FindProperty("_DetailMap", properties);
		_ToonRampBox = ShaderGUI.FindProperty("_ToonRampBox", properties);
		_RimLightBox = ShaderGUI.FindProperty("_RimLightBox", properties);
		_SpecularBox = ShaderGUI.FindProperty("_SpecularBox", properties);
		_DetailBox = ShaderGUI.FindProperty("_DetailBox", properties);
	}


    public void UpdateComponents()
    {
        //initialize toggles
        fakeLight=new StoredToggle(Styles.fakeLight,_FakeLight.floatValue==1);
        occlusionOffset=new StoredToggle(Styles.occlusionOffset,_OcclusionOffset.floatValue==1);
        toonyHighlights=new StoredToggle(Styles.toonyHighlight,_ToonyHighlights.floatValue==1);

        //initialize lists
        occlusionOffsetOptions= new StoredPropertyList(occlusionOffset.IsEnabled(),true);
        occlusionOffsetOptions.AddProperty(_OcclusionOffsetIntensity);

        fakeLightOptions = new StoredPropertyList(fakeLight.IsEnabled(),true);
        fakeLightOptions.AddProperty(_FakeLightColor);
        fakeLightOptions.AddProperty(_FakeLightX);
        fakeLightOptions.AddProperty(_FakeLightY);
        fakeLightOptions.AddProperty(_FakeLightZ);

        metallicWorkflow = new StoredPropertyList((Workflow)_workflow.getSelectedOption()==Workflow.Metallic,false);
        metallicWorkflow.AddProperty(_MetallicMap);

        specularWorkflow = new StoredPropertyList((Workflow)_workflow.getSelectedOption()==Workflow.Specular,false);
        specularWorkflow.AddProperty(_SpecularMap);

        anisotropicOptions=new StoredPropertyList((SpMode)_SpMode.getSelectedOption()==SpMode.Anisotropic,false);
        anisotropicOptions.AddProperty(_TangentMap);
        anisotropicOptions.AddProperty(_AnisotropyMap);

        fakeHighlightOptions=new StoredPropertyList((SpMode)_SpMode.getSelectedOption()==SpMode.Fake,false);
        fakeHighlightOptions.AddProperty(_FakeHightlights);
        fakeHighlightOptions.AddProperty(_FakeHighlightIntensity);

        matcapOptions = new StoredPropertyList((IndirectSpecular)_indirectSpecular.getSelectedOption()==IndirectSpecular.Matcap,false);
        matcapOptions.AddProperty(_Matcap);

        cubemapOptions = new StoredPropertyList((IndirectSpecular)_indirectSpecular.getSelectedOption()==IndirectSpecular.Cubemap,false);
        cubemapOptions.AddProperty(_Cubemap);

        indirectColorOptions = new StoredPropertyList((IndirectSpecular)_indirectSpecular.getSelectedOption()==IndirectSpecular.Color,false);
        indirectColorOptions.AddProperty(_IndirectColor);

        toonyHighlightsOptions = new StoredPropertyList(toonyHighlights.IsEnabled(),false);
        toonyHighlightsOptions.AddProperty(_HighlightRamp);
        toonyHighlightsOptions.AddProperty(_HighlightRampOffset);
        toonyHighlightsOptions.AddProperty(_HighlightIntensity);

        //initialize boxes
        rampOptions=new PropertiesBox(Styles.rampOptions,BooleanFloat(_ToonRampBox.floatValue),false,false);
        rampOptions.AddProperty(_Ramp);
        rampOptions.AddProperty(_RampOffset);
        rampOptions.AddProperty(_ShadowIntensity);
        rampOptions.AddProperty(occlusionOffset);
        rampOptions.AddProperty(occlusionOffsetOptions);
        rampOptions.AddProperty(fakeLight);
        rampOptions.AddProperty(fakeLightOptions);

        //rimOptions=new PropertiesBox(Styles.rimOptions,rimOpen,true,material.IsKeywordEnabled("_RIM_LIGHT"));
        rimOptions=new PropertiesBox(Styles.rimOptions,BooleanFloat(_RimLightBox.floatValue),true,_RimIntensity.GetStoredProperty().floatValue!=0);
        rimOptions.AddProperty(_RimIntensity);
        rimOptions.AddProperty(_RimStrength);
        rimOptions.AddProperty(_RimSharpness);

        rimNeedsReset = !rimOptions.IsEnabled();

        specularOptions=new PropertiesBox(Styles.specularOptions,BooleanFloat(_SpecularBox.floatValue),true,material.IsKeywordEnabled("_ENABLE_SPECULAR"));
        specularOptions.AddProperty(_workflow);
        specularOptions.AddProperty(metallicWorkflow);
        specularOptions.AddProperty(specularWorkflow);
        specularOptions.AddProperty(_GlossinessMap);
        specularOptions.AddProperty(_SpMode);
        specularOptions.AddProperty(anisotropicOptions);
        specularOptions.AddProperty(fakeHighlightOptions);
        specularOptions.AddProperty(_indirectSpecular);
        specularOptions.AddProperty(matcapOptions);
        specularOptions.AddProperty(cubemapOptions);
        specularOptions.AddProperty(indirectColorOptions);
        specularOptions.AddProperty(toonyHighlights);
        specularOptions.AddProperty(toonyHighlightsOptions);
        specularOptions.AddProperty(_HighlightPattern);

        detailOptions=new PropertiesBox(Styles.detailOptions,BooleanFloat(_DetailBox.floatValue),true,material.IsKeywordEnabled("_DETAIL_MAP"));
        detailOptions.AddProperty(_DetailMask);
        detailOptions.AddProperty(_DetailIntensity);
        detailOptions.AddProperty(_DetailTexture);
        detailOptions.AddProperty(_DetailBumpMap);
        detailOptions.AddProperty(_DetailTileAndOffset);

        occlusionOffset.Enable(_OcclusionOffset.floatValue==1);
        occlusionOffsetOptions.Enable(occlusionOffset.IsEnabled());
        fakeLight.Enable(_FakeLight.floatValue==1);
        fakeLightOptions.Enable(fakeLight.IsEnabled());
        toonyHighlights.Enable(_ToonyHighlights.floatValue==1);
        toonyHighlightsOptions.Enable(toonyHighlights.IsEnabled());
        rimOptions.Enable(_RimIntensity.GetStoredProperty().floatValue!=0);       
        specularOptions.Enable(material.IsKeywordEnabled("_ENABLE_SPECULAR"));       
        detailOptions.Enable(material.IsKeywordEnabled("_DETAIL_MAP"));		


    }

    public static float floatBoolean(bool boolean)
    {
        if(boolean)
            return 1;
        else
            return 0;
    }

    public static bool BooleanFloat(float floatBool)
    {
        if(floatBool==1)
            return true;
        else
            return false;
    }
    
    public static void SetupMaterialWithBlendMode(Material material, BlendMode blendMode)
    {
        switch (blendMode)
        {
            case BlendMode.Opaque:
                material.shader=Shader.Find("Hidden/Cibbis shaders/toony standard/Opaque");
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                break;
            case BlendMode.Cutout:
                material.shader=Shader.Find("Hidden/Cibbis shaders/toony standard/Cutout");
                material.EnableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                break;
            case BlendMode.Fade:
                material.shader=Shader.Find("Hidden/Cibbis shaders/toony standard/Fade");
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                break;
            case BlendMode.Transparent:
                material.shader=Shader.Find("Hidden/Cibbis shaders/toony standard/Transparent");
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                break;
        }
    }

    public static void SetupIndirectSource(Material material, IndirectSpecular indirect)
    {
        switch (indirect)
        {
            case IndirectSpecular.Probe:
               
                material.DisableKeyword("_CUSTOM_INDIRECT");
                break;
            case IndirectSpecular.Matcap:
                material.EnableKeyword("_CUSTOM_INDIRECT");
                break;
            case IndirectSpecular.Cubemap:
                material.EnableKeyword("_CUSTOM_INDIRECT");
                break;
        }
    }
    
    public static void SetupWorkflow(Material material, Workflow workflow)
    {
        switch (workflow)
        {
            case Workflow.Metallic:
               
                material.DisableKeyword("_SPECULAR_WORKFLOW");
                break;
            case Workflow.Specular:
                material.EnableKeyword("_SPECULAR_WORKFLOW");
                break;
        }
    }
    
    public static void SetupSpMode(Material material, SpMode spMode)
    {
        switch (spMode)
        {
            case SpMode.Standard:
               
                material.DisableKeyword("_ANISOTROPIC_SPECULAR");
                material.DisableKeyword("_FAKE_SPECULAR");
                break;
            case SpMode.Anisotropic:
                material.EnableKeyword("_ANISOTROPIC_SPECULAR");
                material.DisableKeyword("_FAKE_SPECULAR");
                break;
            case SpMode.Fake:
                material.DisableKeyword("_ANISOTROPIC_SPECULAR");
                material.EnableKeyword("_FAKE_SPECULAR");
                break;
        }
    }

}
