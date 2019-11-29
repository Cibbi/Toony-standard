using UnityEditor;
using UnityEngine;
using System;
using System.Collections;

namespace Cibbi.ToonyStandard
{
    public class SpecularSection : OrderedSection
    {
        public enum IndirectSpecular
        {
            None,
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
            public static GUIContent title = new GUIContent("Specular Options", "Various options for specular calculations, can be disabled");

            public static GUIContent indirectSpecular = new GUIContent("Indirect fallback", "Defines the fallback of the indirect specular in case the probe is not baked \n\nNone: does not fallback \n\nMatcap: uses a matcap texture \n\nCubemap uses a cubemap \n\nColor: uses a single color");
            public static GUIContent indirectOverride = new GUIContent("Always use fallback", "The fallback will always be used");
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

            public static GUIContent GradientEditorButton = new GUIContent("Open gradient editor", "Open the gradient editor for creating a custom toon ramp");
            public static void ToggleGradientEditorToggle(bool isOpen)
            {
                if(isOpen)
                {
                    Styles.GradientEditorButton.text = "Close gradient editor";
                    Styles.GradientEditorButton.tooltip = "Close the gradient editor";
                }
                else
                {
                    Styles.GradientEditorButton.text = "Open gradient editor";
                    Styles.GradientEditorButton.tooltip = "Open the gradient editor for creating a custom toon ramp";
                }
            }
        }

        MaterialProperty _indirectSpecular;
        MaterialProperty _IndirectOverride;
        MaterialProperty _workflow;
        MaterialProperty _SpMode;
        MaterialProperty _GlossinessMap;
        MaterialProperty _Glossiness;
        MaterialProperty _MetallicMap;
        MaterialProperty _Metallic;
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

        MaterialProperty _ToonyHighlights;

        MaterialProperty _SpecularBox;
        MaterialProperty _SpecularOn;

        ToonyStandardGUI inspector;
        InspectorLevel level;

        GradientEditor gradientEditor;

        bool isGradientEditorOpen;
        bool needToStorePreviousRamp;

        Texture PreviousRamp;

        public SpecularSection(MaterialProperty[] properties, InspectorLevel level, ToonyStandardGUI gui, bool open, bool enabled) : base(Styles.title, open, enabled)
        {
            FindProperties(properties);
            this.inspector = gui;
            this.level = level;

            foreach (Material mat in _SpecularOn.targets)
            {
                TSFunctions.SetKeyword(mat, "_SPECULARHIGHLIGHTS_OFF", !(mat.GetFloat(_SpecularOn.name) != 0));
                SetupWorkflow(mat, (Workflow)_workflow.floatValue);
                SetupSpMode(mat, (SpMode)_SpMode.floatValue);
            }

            gradientEditor = new GradientEditor();
            isGradientEditorOpen = false;
            needToStorePreviousRamp = true;
            Selection.selectionChanged += ResetRampTexture;
        }

        private void FindProperties(MaterialProperty[] properties)
        {
            _indirectSpecular = FindProperty("_IndirectSpecular", properties);
            _IndirectOverride = FindProperty("_IndirectOverride", properties);
            _workflow = FindProperty("_Workflow", properties);
            _SpMode = FindProperty("_SpMode", properties);
            _GlossinessMap = FindProperty("_GlossinessMap", properties);
            _Glossiness = FindProperty("_Glossiness", properties);
            _MetallicMap = FindProperty("_MetallicMap", properties);
            _Metallic = FindProperty("_Metallic", properties);
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

            _ToonyHighlights = FindProperty("_ToonyHighlights", properties);

            _SpecularBox = FindProperty("_SpecularBox", properties);
            _SpecularOn = FindProperty("_SpecularOn", properties);
        }

        public override void SectionContent(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            FindProperties(properties);

            bool isToonyHighlightsEnabled;
            EditorGUILayout.Space();

            TSFunctions.DrawSelector(Enum.GetNames(typeof(Workflow)), _workflow, Styles.workflow, materialEditor);
            if ((Workflow)_workflow.floatValue == Workflow.Metallic)
            {
                if(level==InspectorLevel.Normal)
                {   
                    Rect r = TSFunctions.GetControlRectForSingleLine(); 
                    EditorGUI.BeginChangeCheck();
                    materialEditor.TexturePropertyMiniThumbnail(r,_MetallicMap, Styles.metallic.text,Styles.metallic.tooltip);
                    if(EditorGUI.EndChangeCheck())
                    {
                        inspector.RegenerateMSOT();
                    }
                    TSFunctions.ProperSlider(MaterialEditor.GetRectAfterLabelWidth(r), ref _Metallic);

                }
                else
                {
                    materialEditor.ShaderProperty(_Metallic, Styles.metallic);
                }
            }
            else if ((Workflow)_workflow.floatValue == Workflow.Specular)
            {
                materialEditor.TexturePropertySingleLine(Styles.specular, _MetallicMap);
            }

            if(level==InspectorLevel.Normal)
            {   
                Rect r = TSFunctions.GetControlRectForSingleLine(); 
                EditorGUI.BeginChangeCheck();
                materialEditor.TexturePropertyMiniThumbnail(r,_GlossinessMap, Styles.smoothness.text, Styles.smoothness.tooltip);
                if(EditorGUI.EndChangeCheck())
                {
                    inspector.RegenerateMSOT();
                }
                TSFunctions.ProperSlider(MaterialEditor.GetRectAfterLabelWidth(r), ref _Glossiness);
            }
            else
            {
                materialEditor.ShaderProperty(_Glossiness, Styles.smoothness);
            }

            TSFunctions.DrawSelector(Enum.GetNames(typeof(SpMode)), _SpMode, Styles.spMode, materialEditor);
            if ((SpMode)_SpMode.floatValue == SpMode.Anisotropic)
            {
                materialEditor.TexturePropertySingleLine(Styles.anisotropy, _AnisotropyMap, _Anisotropy);
                materialEditor.TexturePropertySingleLine(Styles.tangent, _TangentMap);
            }
            else if ((SpMode)_SpMode.floatValue == SpMode.Fake)
            {
                materialEditor.TexturePropertySingleLine(Styles.fakeHighlights, _FakeHightlights);
                materialEditor.ShaderProperty(_FakeHighlightIntensity, Styles.fakeHighlightIntensity);
            }

            TSFunctions.DrawSelector(Enum.GetNames(typeof(IndirectSpecular)), _indirectSpecular, Styles.indirectSpecular, materialEditor);
            if ((IndirectSpecular)_indirectSpecular.floatValue == IndirectSpecular.Matcap)
            {
                materialEditor.TexturePropertySingleLine(Styles.matcap, _Matcap);
            }
            else if ((IndirectSpecular)_indirectSpecular.floatValue == IndirectSpecular.Cubemap)
            {
                materialEditor.TexturePropertySingleLine(Styles.cubemap, _Cubemap);
            }
            else if ((IndirectSpecular)_indirectSpecular.floatValue == IndirectSpecular.Color)
            {
                TSFunctions.ProperColorBox(ref _IndirectColor,Styles.indirectColor);
            }
            if((IndirectSpecular)_indirectSpecular.floatValue != IndirectSpecular.None)
            {
                TSFunctions.ProperToggle(ref _IndirectOverride, Styles.indirectOverride);
            }
            else
            {
                _IndirectOverride.floatValue = 0;
            }

            isToonyHighlightsEnabled=TSFunctions.ProperToggle(ref _ToonyHighlights,Styles.toonyHighlight);

            if (isToonyHighlightsEnabled)
            {
                EditorGUILayout.BeginHorizontal();
                materialEditor.TexturePropertySingleLine(Styles.highlightRamp, _HighlightRamp, _HighlightRampColor);
                
                if(GUILayout.Button(Styles.GradientEditorButton))
                {
                    EditorGUILayout.EndHorizontal();
                    isGradientEditorOpen=!isGradientEditorOpen;
                    Styles.ToggleGradientEditorToggle(isGradientEditorOpen);
                }
                else
                {
                    EditorGUILayout.EndHorizontal();
                }
                if(isGradientEditorOpen)
                {
                    EditorGUILayout.BeginVertical("box");
                    if(needToStorePreviousRamp)
                    {
                        needToStorePreviousRamp = false;
                        PreviousRamp = _HighlightRamp.textureValue;
                    }
                    if(_HighlightRamp.textureValue != (Texture)gradientEditor.GetGradientTexture())
                    {
                        _HighlightRamp.textureValue = (Texture)gradientEditor.GetGradientTexture();
                    }
                    if(gradientEditor.DrawGUI())
                    {
                        materialEditor.Repaint();
                    }
                    if(GUILayout.Button("Save and apply"))
                    {
                        string path = inspector.GetTextureDestinationPath((Material)_HighlightRamp.targets[0],"_highlight_ramp.png");
                        _HighlightRamp.textureValue = (Texture) gradientEditor.SaveGradient(path);
                        needToStorePreviousRamp = true;
                        PreviousRamp = null;
                        isGradientEditorOpen = false;
                    }
                    EditorGUILayout.EndVertical();
                }
                else
                {
                    if(PreviousRamp != null)
                    {
                        _HighlightRamp.textureValue = PreviousRamp;
                        PreviousRamp = null;
                        needToStorePreviousRamp = true;
                    }
                }
                
                materialEditor.ShaderProperty(_HighlightRampOffset, Styles.hightlightRampOffset);
                materialEditor.ShaderProperty(_HighlightIntensity, Styles.highlightIntensity);
            }

            materialEditor.TexturePropertySingleLine(Styles.highlightPattern, _HighlightPattern);
            EditorGUI.indentLevel++;
            materialEditor.TextureScaleOffsetProperty(_HighlightPattern);
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();
        }

        public override void EndBoxCheck(bool isOpen, bool isEnabled)
        {
            _SpecularBox.floatValue = TSFunctions.floatBoolean(isOpen);
           
            if (!isEnabled)
            {   
                if(!_SpecularOn.hasMixedValue)
                {
                    foreach (Material mat in _SpecularOn.targets)
                    {
                        TSFunctions.SetKeyword(mat, "_SPECULARHIGHLIGHTS_OFF", !isEnabled);
                        _SpecularOn.floatValue = TSFunctions.floatBoolean(!mat.IsKeywordEnabled("_SPECULARHIGHLIGHTS_OFF"));
                    }
                }
            }

            foreach (Material mat in _workflow.targets)
            {
                SetupWorkflow(mat, (Workflow)_workflow.floatValue);
            }
            foreach (Material mat in _SpMode.targets)
            {
                SetupSpMode(mat, (SpMode)_SpMode.floatValue);
            }
        }

        public override void OnAdd()
        {
            foreach (Material mat in _SpecularOn.targets)
            {
                TSFunctions.SetKeyword(mat, "_SPECULARHIGHLIGHTS_OFF", !(mat.GetFloat(_SpecularOn.name) != 0));
                SetupWorkflow(mat, (Workflow)_workflow.floatValue);
                SetupSpMode(mat, (SpMode)_SpMode.floatValue);
            }
        }

        protected override MaterialProperty GetIndex()
        {
            return _SpecularOn;
        }

        protected override MaterialProperty GetBox()
        {
            return _SpecularBox;
        }

        public static void SetupWorkflow(Material material, Workflow workflow)
        {
            switch (workflow)
            {
                case Workflow.Metallic:

                    material.DisableKeyword("_SPECGLOSSMAP");
                    break;
                case Workflow.Specular:
                    material.EnableKeyword("_SPECGLOSSMAP");
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

        public void ResetRampTexture()
        {
             if(PreviousRamp != null)
            {
                _HighlightRamp.textureValue = PreviousRamp;
                PreviousRamp = null;
                needToStorePreviousRamp = true;
            }
            Selection.selectionChanged -= ResetRampTexture;
        }
    }
}