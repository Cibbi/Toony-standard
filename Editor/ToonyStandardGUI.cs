using UnityEditor;
using UnityEngine;
using System;


public class ToonyStandardGUI : ShaderGUI 
{
    bool rimNeedsReset;

    //bool isFirstCycle = false;

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
        public static GUIContent fakeLightColor = new GUIContent("Fake light color", "Color of the fake light");
        public static GUIContent fakeLightX = new GUIContent("X", "X component of the direction vector");
        public static GUIContent fakeLightY = new GUIContent("Y", "Y component of the direction vector");
        public static GUIContent fakeLightZ = new GUIContent("Z", "Z component of the direction vector");

        
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

        GUIStyle sectionStyle;

	 	MaterialProperty _blendMode;
        MaterialProperty _Cull;

        MaterialProperty _MainTex;
        MaterialProperty _Color;
        MaterialProperty _Cutoff;
        MaterialProperty _BumpMap;
        MaterialProperty _BumpScale;
        MaterialProperty _Emission;
        MaterialProperty _EmissionColor;
        MaterialProperty _OcclusionMap;
        MaterialProperty _Occlusion;

        MaterialProperty _Ramp;
        MaterialProperty _RampColor;
        MaterialProperty _RampOffset;
        MaterialProperty _ShadowIntensity;
        MaterialProperty _OcclusionOffsetIntensity;
        MaterialProperty _FakeLightColor;
        MaterialProperty _FakeLightX;
        MaterialProperty _FakeLightY;
        MaterialProperty _FakeLightZ;
        
        MaterialProperty _RimStrength;
        MaterialProperty _RimSharpness;
        MaterialProperty _RimIntensity;

        MaterialProperty _indirectSpecular;
        MaterialProperty _workflow;
        MaterialProperty _SpMode;       
        MaterialProperty _GlossinessMap;
        MaterialProperty _Glossiness;
        MaterialProperty _MetallicMap;
        MaterialProperty _Metallic;
        //MaterialProperty _SpecularMap;
        MaterialProperty _AnisotropyMap;
        MaterialProperty _Anisotropy;
        MaterialProperty _TangentMap;
        MaterialProperty _FakeHightlights;
        MaterialProperty _Matcap;
        MaterialProperty _Cubemap;
        MaterialProperty _IndirectColor;
        MaterialProperty _HighlightRamp;
        MaterialProperty _HighlightRampColor;
        MaterialProperty _HighlightRampOffset;
        MaterialProperty _HighlightIntensity;
        MaterialProperty _FakeHighlightIntensity;
        MaterialProperty _HighlightPattern;
       
        MaterialProperty _DetailMask;
        MaterialProperty _DetailIntensity;
        MaterialProperty _DetailTexture;
        MaterialProperty _DetailColor;
        MaterialProperty _DetailBumpMap;
        MaterialProperty _DetailBumpScale;
        MaterialProperty _DetailTileAndOffset;

		Material material;

        //StoredToggle toonyHighlights;

        //PropertiesBox rimOptions;
        // specularOptions;
        //PropertiesBox detailOptions;

        MaterialProperty occlusionOffsetOptions;
        MaterialProperty fakeLightOptions;
        MaterialProperty metallicWorkflow;
        MaterialProperty specularWorkflow;
        MaterialProperty anisotropicOptions;
        MaterialProperty fakeHighlightOptions;
        MaterialProperty matcapOptions;
        MaterialProperty cubemapOptions;
        MaterialProperty indirectColorOptions;
        MaterialProperty toonyHighlightsOptions;

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

    public void Start(MaterialEditor materialEditor, MaterialProperty[] properties)
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
             
		}

        sectionStyle = new GUIStyle(EditorStyles.boldLabel);
        sectionStyle.alignment = TextAnchor.MiddleCenter;

        material = materialEditor.target as Material;
        //initialize properties
        FindProperties(properties);
        rimNeedsReset=false;

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
    }

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    { 
        if(icon==null)
		{
			Start(materialEditor, properties);
			//isFirstCycle=false;
		}

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label(icon,GUILayout.Width(icon.width),GUILayout.Height(icon.height));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
    
        FindProperties(properties);
        //draw main section
        DrawMainSection(materialEditor);   
        //ramp options box
        DrawRampOptionsSection(materialEditor);
        //rim light options box
        DrawRimLightOptionsSection(materialEditor);
        //Specular options box
        DrawSpecularOptionsSection(materialEditor);
        //Detail options box
        DrawDetailOptionsSection(materialEditor);
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
            if(GUILayout.Button("Toony Standard MasterBuild 20190221",aboutLabelStyle,GUILayout.Height(32)))
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
        _blendMode = FindProperty("_Mode", properties);
        _Cull = FindProperty("_Cull", properties);

        _MainTex = FindProperty("_MainTex", properties);
        _Color = FindProperty("_Color", properties);
        _Cutoff = FindProperty("_Cutoff", properties);
        _BumpMap = FindProperty("_BumpMap", properties);
        _BumpScale = FindProperty("_BumpScale", properties); 
        _Emission = FindProperty("_EmissionMap", properties);
        _EmissionColor = FindProperty("_EmissionColor", properties); 
        _OcclusionMap = FindProperty("_OcclusionMap", properties);
        _Occlusion = FindProperty("_Occlusion", properties); 
        //_MainTileAndOffset = new StoredTileAndOffset(ShaderGUI.FindProperty("_MainTex", properties));
        
        _Ramp = FindProperty("_Ramp", properties);
        _RampColor = FindProperty("_RampColor", properties);         
        _RampOffset = FindProperty("_RampOffset", properties);
        _ShadowIntensity = FindProperty("_ShadowIntensity", properties);
        _OcclusionOffsetIntensity = FindProperty("_OcclusionOffsetIntensity", properties);
        _FakeLightColor = FindProperty("_FakeLightColor", properties);
        _FakeLightX = FindProperty("_FakeLightX", properties);
        _FakeLightY = FindProperty("_FakeLightY", properties);
        _FakeLightZ = FindProperty("_FakeLightZ", properties); 

        _RimStrength = FindProperty("_RimStrength", properties);
        _RimSharpness = FindProperty("_RimSharpness", properties);
        _RimIntensity = FindProperty("_RimIntensity", properties);

        _indirectSpecular = FindProperty("_IndirectSpecular", properties); 
        _workflow = FindProperty("_Workflow", properties);
        _SpMode = FindProperty("_SpMode", properties);
        _GlossinessMap = FindProperty("_GlossinessMap", properties);
        _Glossiness = FindProperty("_Glossiness", properties); 
        _MetallicMap = FindProperty("_MetallicMap", properties);
        _Metallic = FindProperty("_Metallic", properties);
        //_SpecularMap = new StoredTextureProperty(Styles.specular,ShaderGUI.FindProperty("_MetallicMap", properties));
        _AnisotropyMap = FindProperty("_AnisotropyMap", properties);
        _Anisotropy = FindProperty("_Anisotropy", properties);
        _TangentMap = FindProperty("_TangentMap", properties);
        _FakeHightlights = FindProperty("_FakeHighlights", properties);
        _Matcap = FindProperty("_Matcap", properties);
        _Cubemap = FindProperty("_Cubemap", properties);
        _IndirectColor = FindProperty("_IndirectColor", properties);
        _HighlightRamp = FindProperty("_HighlightRamp", properties);
        _HighlightRampColor = FindProperty("_HighlightRampColor", properties);
        _HighlightRampOffset = FindProperty("_HighlightRampOffset", properties);
        _HighlightIntensity = FindProperty("_HighlightIntensity", properties);  
        _FakeHighlightIntensity = FindProperty("_FakeHighlightIntensity", properties);  
        _HighlightPattern = FindProperty("_HighlightPattern", properties);

        _DetailMask = FindProperty("_DetailMask", properties);
        _DetailIntensity = FindProperty("_DetailIntensity", properties);  
        _DetailTexture = FindProperty("_DetailTexture", properties);
        _DetailColor = FindProperty("_DetailColor", properties);
        _DetailBumpMap = FindProperty("_DetailBumpMap", properties);
        _DetailBumpScale = FindProperty("_DetailBumpScale", properties);
        //_DetailTileAndOffset = new StoredTileAndOffset(ShaderGUI.FindProperty("_DetailTexture", properties));

        //_HighlightPattern.ShowTextureScaleAndOffset(true);

        _ToonyHighlights = FindProperty("_ToonyHighlights", properties);
        _OcclusionOffset = FindProperty("_OcclusionOffset", properties);
        _FakeLight = FindProperty("_FakeLight", properties);
		_EnableSpecular = FindProperty("_EnableSpecular", properties);
		_DetailMap = FindProperty("_DetailMap", properties);
		_ToonRampBox = FindProperty("_ToonRampBox", properties);
		_RimLightBox = FindProperty("_RimLightBox", properties);
		_SpecularBox = FindProperty("_SpecularBox", properties);
		_DetailBox = FindProperty("_DetailBox", properties);
	}

    public void DrawMainSection(MaterialEditor materialEditor)
    {
        EditorGUI.BeginChangeCheck();
        DrawSelector(Enum.GetNames(typeof(BlendMode)),_blendMode,Styles.blendMode,materialEditor);
        if (EditorGUI.EndChangeCheck())
        {
            foreach(Material mat in _blendMode.targets)
            {
                SetupMaterialWithBlendMode(mat, (BlendMode)_blendMode.floatValue);  
            }      
        }
        
        //draw cull mode
        materialEditor.ShaderProperty(_Cull,Styles.cullMode);
        EditorGUILayout.Space();

        //draw main properties
        materialEditor.TexturePropertySingleLine(Styles.mainTex,_MainTex,_Color);
        if((BlendMode)_blendMode.floatValue==BlendMode.Cutout)
        {
            EditorGUI.indentLevel+=MaterialEditor.kMiniTextureFieldLabelIndentLevel + 1;
            materialEditor.ShaderProperty(_Cutoff,Styles.cutOff);
            EditorGUI.indentLevel-=MaterialEditor.kMiniTextureFieldLabelIndentLevel + 1;
        }
        materialEditor.TexturePropertySingleLine(Styles.normal,_BumpMap,_BumpScale);
        materialEditor.TexturePropertySingleLine(Styles.occlusion,_OcclusionMap,_Occlusion);


        //emission
        EditorGUI.BeginChangeCheck();
        if (materialEditor.EmissionEnabledProperty())
        {
            //bool hadEmissionTexture = _Emission.GetGetExtraProperty1().textureValue != null;
            // Texture and HDR color controls
            materialEditor.TexturePropertySingleLine(Styles.emission,_Emission,_EmissionColor);
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
            foreach(Material mat in _Emission.targets)
            {
                MaterialEditor.FixupEmissiveFlag(mat);
                bool shouldEmissionBeEnabled = (mat.globalIlluminationFlags & MaterialGlobalIlluminationFlags.EmissiveIsBlack) == 0;
                SetKeyword(mat, "_EMISSION", shouldEmissionBeEnabled);
                if (shouldEmissionBeEnabled)
                {
                    mat.SetOverrideTag("IsEmissive", "true");
                }
                else
                {
                    mat.SetOverrideTag("IsEmissive", "false");
                }
            }
        }
        materialEditor.TextureScaleOffsetProperty(_MainTex);

        EditorGUILayout.Space();
    }

    public void DrawRampOptionsSection(MaterialEditor materialEditor)
    {  
        EditorGUI.BeginChangeCheck();       
		Color bCol=GUI.backgroundColor;
        GUI.backgroundColor = new Color (0.9f, 0.9f, 0.9f, 0.75f);
		EditorGUILayout.BeginVertical ("Button");
		GUI.backgroundColor = bCol;
        bool isOpen=BooleanFloat(_ToonRampBox.floatValue);
        bool isOcclusionOffsetEnabled=BooleanFloat(_OcclusionOffset.floatValue);
        bool isFakeLightEnabled=BooleanFloat(_FakeLight.floatValue);
		Rect r = EditorGUILayout.BeginHorizontal();   
		EditorGUILayout.LabelField("", GUILayout.MaxWidth(10.0f));
        EditorGUILayout.LabelField(Styles.rampOptions, sectionStyle);
		isOpen = GUI.Toggle(r, isOpen, GUIContent.none, new GUIStyle()); 
		EditorGUILayout.Toggle(isOpen, EditorStyles.foldout, GUILayout.MaxWidth(15.0f));
        EditorGUILayout.EndHorizontal();
        if (isOpen)
        {
            EditorGUILayout.Space();
			materialEditor.TexturePropertySingleLine(Styles.ramp,_Ramp,_RampColor);
            materialEditor.ShaderProperty(_RampOffset,Styles.rampOffset);
            materialEditor.ShaderProperty(_ShadowIntensity,Styles.shadowIntensity);
            isOcclusionOffsetEnabled=EditorGUILayout.Toggle(Styles.occlusionOffset, isOcclusionOffsetEnabled);
            if(isOcclusionOffsetEnabled)
            {
                materialEditor.ShaderProperty(_OcclusionOffsetIntensity,Styles.occlusionOffsetIntensity);
            }
            isFakeLightEnabled=EditorGUILayout.Toggle(Styles.fakeLight, isFakeLightEnabled);
            if(isFakeLightEnabled)
            {
                materialEditor.ShaderProperty(_FakeLightColor,Styles.fakeLightColor);
                materialEditor.ShaderProperty(_FakeLightX,Styles.fakeLightX);
                materialEditor.ShaderProperty(_FakeLightY,Styles.fakeLightY);
                materialEditor.ShaderProperty(_FakeLightZ,Styles.fakeLightZ);
            }

        }
		EditorGUILayout.EndVertical();
		EditorGUILayout.Space();

        if (EditorGUI.EndChangeCheck())
        {
            _ToonRampBox.floatValue=floatBoolean(isOpen);
            _OcclusionOffset.floatValue=floatBoolean(isOcclusionOffsetEnabled);
            _FakeLight.floatValue=floatBoolean(isFakeLightEnabled); 

            if(!isOcclusionOffsetEnabled)
            {
                _OcclusionOffsetIntensity.floatValue=0;
            }      
        }
        EditorGUILayout.Space();
    }

    public void DrawRimLightOptionsSection(MaterialEditor materialEditor)
     {
        EditorGUI.BeginChangeCheck();
    
        Color bCol=GUI.backgroundColor;
        GUI.backgroundColor = new Color (0.9f, 0.9f, 0.9f, 0.75f);
		EditorGUILayout.BeginVertical ("Button");
		GUI.backgroundColor = bCol;
        bool isOpen=BooleanFloat(_RimLightBox.floatValue);
        bool isEnabled=_RimIntensity.floatValue!=0;
		Rect r = EditorGUILayout.BeginHorizontal();
        isEnabled = EditorGUILayout.Toggle(isEnabled, EditorStyles.radioButton, GUILayout.MaxWidth(15.0f));
		isOpen = GUI.Toggle(r, isOpen, GUIContent.none, new GUIStyle()); 
		
        EditorGUILayout.LabelField(Styles.rimOptions, sectionStyle);
		isOpen = GUI.Toggle(r, isOpen, GUIContent.none, new GUIStyle()); 
		EditorGUILayout.Toggle(isOpen, EditorStyles.foldout, GUILayout.MaxWidth(15.0f));
        EditorGUILayout.EndHorizontal();
        if (isOpen)
        {
            EditorGUILayout.Space();
            EditorGUI.BeginDisabledGroup(!isEnabled);			
            materialEditor.ShaderProperty(_RimIntensity,Styles.rimIntensity);
            materialEditor.ShaderProperty(_RimStrength,Styles.rimStrength);
            materialEditor.ShaderProperty(_RimSharpness,Styles.rimSharpness);
            EditorGUI.EndDisabledGroup();
        }
		EditorGUILayout.EndVertical();
		EditorGUILayout.Space();

        if (EditorGUI.EndChangeCheck())
        {
            _RimLightBox.floatValue=floatBoolean(isOpen);
            if(isEnabled && rimNeedsReset)
            {
                _RimIntensity.floatValue=1;
                rimNeedsReset=false;
                
            }
            else if (!isEnabled)
            {
                _RimIntensity.floatValue=0;
                rimNeedsReset=true;
            }      
        }
        EditorGUILayout.Space();

        //rimNeedsReset = !isEnabled;
     }

    public void DrawSpecularOptionsSection(MaterialEditor materialEditor)
    {
        EditorGUI.BeginChangeCheck();
        Color bCol=GUI.backgroundColor;
        GUI.backgroundColor = new Color (0.9f, 0.9f, 0.9f, 0.75f);
		EditorGUILayout.BeginVertical ("Button");
		GUI.backgroundColor = bCol;
        bool isOpen=BooleanFloat(_SpecularBox.floatValue);
        bool isEnabled=BooleanFloat(_EnableSpecular.floatValue);
		Rect r = EditorGUILayout.BeginHorizontal();
        isEnabled = EditorGUILayout.Toggle(isEnabled, EditorStyles.radioButton, GUILayout.MaxWidth(15.0f));
		//isOpen = GUI.Toggle(r, isOpen, GUIContent.none, new GUIStyle()); 
        EditorGUILayout.LabelField(Styles.specularOptions, sectionStyle);
		isOpen = GUI.Toggle(r, isOpen, GUIContent.none, new GUIStyle()); 
		EditorGUILayout.Toggle(isOpen, EditorStyles.foldout, GUILayout.MaxWidth(15.0f));
        EditorGUILayout.EndHorizontal();
        if (isOpen)
        {
            EditorGUILayout.Space();
            EditorGUI.BeginDisabledGroup(!isEnabled);			
            
            DrawSelector(Enum.GetNames(typeof(Workflow)),_workflow,Styles.workflow,materialEditor);
            if((Workflow)_workflow.floatValue==Workflow.Metallic)
            {
                materialEditor.TexturePropertySingleLine(Styles.metallic,_MetallicMap,_Metallic);
            }
            else if((Workflow)_workflow.floatValue==Workflow.Specular)
            {
                materialEditor.TexturePropertySingleLine(Styles.specular,_MetallicMap);
            }

            materialEditor.TexturePropertySingleLine(Styles.smoothness,_GlossinessMap,_Glossiness);

            DrawSelector(Enum.GetNames(typeof(SpMode)),_SpMode,Styles.spMode,materialEditor);
            if((SpMode)_SpMode.floatValue==SpMode.Anisotropic)
            {
                materialEditor.TexturePropertySingleLine(Styles.anisotropy,_AnisotropyMap,_Anisotropy);
                materialEditor.TexturePropertySingleLine(Styles.tangent,_TangentMap);
            }
            else if((SpMode)_SpMode.floatValue==SpMode.Fake)
            {
                materialEditor.TexturePropertySingleLine(Styles.fakeHighlights,_FakeHightlights);
                materialEditor.TexturePropertySingleLine(Styles.fakeHighlightIntensity,_FakeHighlightIntensity);
            }

            DrawSelector(Enum.GetNames(typeof(IndirectSpecular)),_indirectSpecular,Styles.indirectSpecular,materialEditor);
            if((IndirectSpecular)_indirectSpecular.floatValue==IndirectSpecular.Matcap)
            {
                materialEditor.TexturePropertySingleLine(Styles.matcap,_Matcap);
            }
            else if((IndirectSpecular)_indirectSpecular.floatValue==IndirectSpecular.Cubemap)
            {
                materialEditor.TexturePropertySingleLine(Styles.cubemap,_Cubemap);
            }
            else if((IndirectSpecular)_indirectSpecular.floatValue==IndirectSpecular.Color)
            {
                materialEditor.ShaderProperty(_IndirectColor,Styles.indirectColor);
            }

            _ToonyHighlights.floatValue = floatBoolean(EditorGUILayout.Toggle(Styles.toonyHighlight, BooleanFloat(_ToonyHighlights.floatValue)));
            if(BooleanFloat(_ToonyHighlights.floatValue))
            {
                materialEditor.TexturePropertySingleLine(Styles.highlightRamp,_HighlightRamp,_HighlightRampColor);
                materialEditor.ShaderProperty(_HighlightRampOffset,Styles.hightlightRampOffset);
                materialEditor.ShaderProperty(_HighlightIntensity,Styles.highlightIntensity);
            }

            materialEditor.TexturePropertySingleLine(Styles.highlightPattern,_HighlightPattern);
            EditorGUI.indentLevel++;
            materialEditor.TextureScaleOffsetProperty(_HighlightPattern);
            EditorGUI.indentLevel--;

            EditorGUI.EndDisabledGroup();
        }
		EditorGUILayout.EndVertical();
		EditorGUILayout.Space();

        if (EditorGUI.EndChangeCheck())
        {

            _SpecularBox.floatValue=floatBoolean(isOpen);
            foreach(Material mat in _EnableSpecular.targets)
            {
                SetKeyword(mat, "_ENABLE_SPECULAR",isEnabled);       
            }
            _EnableSpecular.floatValue=floatBoolean(material.IsKeywordEnabled("_ENABLE_SPECULAR"));
            foreach(Material mat in _workflow.targets)
            {
                SetupWorkflow(mat,(Workflow)_workflow.floatValue);
            }
            foreach(Material mat in _SpMode.targets)
            {
                SetupSpMode(mat,(SpMode)_SpMode.floatValue);
            }
            foreach(Material mat in _indirectSpecular.targets)
            {
                SetupIndirectSource(mat,(IndirectSpecular)_indirectSpecular.floatValue);
            }

            /* metallicWorkflow.Enable((Workflow)_workflow.getSelectedOption()==Workflow.Metallic);
            specularWorkflow.Enable((Workflow)_workflow.getSelectedOption()==Workflow.Specular);
            anisotropicOptions.Enable((SpMode)_SpMode.getSelectedOption()==SpMode.Anisotropic);
            matcapOptions.Enable((IndirectSpecular)_indirectSpecular.getSelectedOption()==IndirectSpecular.Matcap);
            cubemapOptions.Enable((IndirectSpecular)_indirectSpecular.getSelectedOption()==IndirectSpecular.Cubemap);*/
            //SetKeyword(material,"_TOONY_HIGHLIGHTS",toonyHighlights.IsEnabled()); 
            //_ToonyHighlights.floatValue=floatBoolean(toonyHighlights.IsEnabled());
            //toonyHighlightsOptions.Enable(toonyHighlights.IsEnabled());
        }
        EditorGUILayout.Space();

    }

    public void DrawDetailOptionsSection(MaterialEditor materialEditor)
    {
        EditorGUI.BeginChangeCheck();
    
        Color bCol=GUI.backgroundColor;
        GUI.backgroundColor = new Color (0.9f, 0.9f, 0.9f, 0.75f);
		EditorGUILayout.BeginVertical ("Button");
		GUI.backgroundColor = bCol;
        bool isOpen=BooleanFloat(_DetailBox.floatValue);
        bool isEnabled=BooleanFloat(_DetailMap.floatValue);
		Rect r = EditorGUILayout.BeginHorizontal();
        isEnabled = EditorGUILayout.Toggle(isEnabled, EditorStyles.radioButton, GUILayout.MaxWidth(15.0f));
		isOpen = GUI.Toggle(r, isOpen, GUIContent.none, new GUIStyle()); 
		
        EditorGUILayout.LabelField(Styles.detailOptions, sectionStyle);
		isOpen = GUI.Toggle(r, isOpen, GUIContent.none, new GUIStyle()); 
		EditorGUILayout.Toggle(isOpen, EditorStyles.foldout, GUILayout.MaxWidth(15.0f));
        EditorGUILayout.EndHorizontal();
        if (isOpen)
        {
            EditorGUILayout.Space();
            EditorGUI.BeginDisabledGroup(!isEnabled);			
            materialEditor.TexturePropertySingleLine(Styles.detailMask,_DetailMask);
            materialEditor.ShaderProperty(_DetailIntensity,Styles.detailIntensity);
            materialEditor.TexturePropertySingleLine(Styles.detailPattern,_DetailTexture,_DetailColor);
            materialEditor.TexturePropertySingleLine(Styles.detailNormal,_DetailBumpMap,_DetailBumpScale);
            materialEditor.TextureScaleOffsetProperty(_DetailTexture);
            EditorGUI.EndDisabledGroup();
        }
		EditorGUILayout.EndVertical();
		EditorGUILayout.Space();

        if (EditorGUI.EndChangeCheck())
        {
            _DetailBox.floatValue=floatBoolean(isOpen);
            foreach(Material mat in _DetailMap.targets)
            {
                SetKeyword(mat, "_DETAIL_MAP",isEnabled);  
            }
            _DetailMap.floatValue=floatBoolean(material.IsKeywordEnabled("_DETAIL_MAP"));     
        }
        EditorGUILayout.Space();
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

    protected void SetKeyword(Material m, string keyword, bool state)
	{
		if (state)
			m.EnableKeyword(keyword);
		else
			m.DisableKeyword(keyword);
	}

    public void DrawSelector(string[] options, MaterialProperty selectedOption, GUIContent label, MaterialEditor materialEditor)
        {
            GUIContent[] GUIoptions= new GUIContent[options.Length];
            int i=0;
            foreach(string option in options)
            {   
                GUIoptions[i]=new GUIContent(option, option);
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

}